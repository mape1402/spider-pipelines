namespace Spider.Pipelines.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Generates Spider architecture metadata from ComposeFlow and pipeline fluent calls at compilation time.
    /// </summary>
    [Generator]
    public sealed class SpiderArchitectureSourceGenerator : ISourceGenerator
    {
        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForSyntaxNotifications(() => new Receiver());

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is Receiver receiver))
                return;

            var manifest = new ManifestModel();
            var sourceRoots = CreateSourceRoots(context);

            foreach (var invocation in receiver.ComposeFlowInvocations)
                TryReadFlow(context.Compilation, invocation, manifest, sourceRoots);

            foreach (var invocation in receiver.AttachInvocations)
                TryReadPipeline(context.Compilation, invocation, manifest, sourceRoots);

            AddInvokedFlowRelations(manifest);

            context.AddSource(
                "SpiderGeneratedArchitecture.g.cs",
                SourceText.From(GenerateSource(manifest), Encoding.UTF8));
        }

        private static void TryReadFlow(
            Compilation compilation,
            InvocationExpressionSyntax composeInvocation,
            ManifestModel manifest,
            IReadOnlyCollection<string> sourceRoots)
        {
            var chain = BuildChain(composeInvocation);
            if (chain.Count == 0)
                return;

            var composeName = GetInvocationName(composeInvocation);
            if (composeName != "ComposeFlow")
                return;

            var genericTypes = GetGenericArguments(composeInvocation);
            if (genericTypes.Count != 1 && genericTypes.Count != 2)
                return;

            var semanticModel = compilation.GetSemanticModel(composeInvocation.SyntaxTree);
            var flowName = GetStringArgument(composeInvocation, 0) ?? "Unnamed flow";
            var flowId = "spider.flow:" + Normalize(flowName);
            var flowEvidence = CreateEvidence(semanticModel, composeInvocation, null, sourceRoots);
            var metadata = new Dictionary<string, string>
            {
                ["request"] = GetTypeName(semanticModel, genericTypes[0]),
                ["hasResponse"] = (genericTypes.Count == 2).ToString(CultureInfo.InvariantCulture).ToLowerInvariant()
            };
            var declaringMemberSymbolId = GetEnclosingSymbolId(semanticModel, composeInvocation);
            if (!string.IsNullOrWhiteSpace(declaringMemberSymbolId))
                metadata["declaringMemberSymbolId"] = declaringMemberSymbolId;

            if (genericTypes.Count == 2)
                metadata["response"] = GetTypeName(semanticModel, genericTypes[1]);

            ApplyMetadataInvocations(metadata, chain.Skip(1).Where(invocation => IsMetadataInvocation(GetInvocationName(invocation))));

            manifest.AddComponent(new ComponentModel(flowId, "spider.flow", flowName, metadata, flowEvidence));

            string previousStepId = null;
            var stepIndex = 0;

            foreach (var invocation in chain.Skip(1))
            {
                var invocationName = GetInvocationName(invocation);
                if (invocationName == "UsingProfile")
                {
                    var profileName = GetStringArgument(invocation, 0) ?? "Unnamed profile";
                    var profileId = "spider.flow-profile:" + Normalize(profileName);
                    manifest.AddComponent(new ComponentModel(profileId, "spider.flow-profile", profileName, new Dictionary<string, string>(), CreateEvidence(semanticModel, invocation, null, sourceRoots)));
                    manifest.AddRelation(new RelationModel(flowId, profileId, "uses-profile"));
                    continue;
                }

                if (IsMetadataInvocation(invocationName))
                    continue;

                if (!IsFlowStep(invocationName))
                    continue;

                stepIndex++;
                var kind = GetFlowStepKind(invocationName);
                var stepMetadata = CreateFlowStepMetadata(semanticModel, invocation, invocationName);
                var displayName = GetDisplayName(stepMetadata, GetStepDisplayName(semanticModel, invocation, invocationName));
                var stepId = flowId + "." + stepIndex.ToString("000", CultureInfo.InvariantCulture) + "-" + Normalize(displayName);
                var evidence = CreateEvidence(semanticModel, invocation, GetDelegateExpression(invocation), sourceRoots);

                manifest.AddComponent(new ComponentModel(stepId, kind, displayName, stepMetadata, evidence));
                manifest.AddRelation(new RelationModel(flowId, stepId, "contains"));

                if (invocationName == "Branch")
                    AddBranchRoutes(manifest, semanticModel, invocation, stepId, sourceRoots);

                if (previousStepId != null)
                    manifest.AddRelation(new RelationModel(previousStepId, stepId, "next"));

                previousStepId = stepId;
            }
        }

        private static void TryReadPipeline(
            Compilation compilation,
            InvocationExpressionSyntax attachInvocation,
            ManifestModel manifest,
            IReadOnlyCollection<string> sourceRoots)
        {
            var attachName = GetInvocationName(attachInvocation);
            if (attachName != "Attach")
                return;

            var genericTypes = GetGenericArguments(attachInvocation);
            if (genericTypes.Count != 1 && genericTypes.Count != 2)
                return;

            var semanticModel = compilation.GetSemanticModel(attachInvocation.SyntaxTree);
            var requestName = GetTypeName(semanticModel, genericTypes[0]);
            var responseName = genericTypes.Count == 2 ? GetTypeName(semanticModel, genericTypes[1]) : null;
            var serviceName = GetBridgeServiceName(semanticModel, attachInvocation);
            var targetMethod = GetExecuteAsyncTargetMethod(semanticModel, attachInvocation);
            var requestDisplayName = GetShortTypeName(requestName);
            var pipelineId = responseName == null
                ? "spider.pipeline:" + Normalize(requestDisplayName)
                : "spider.pipeline:" + Normalize(requestDisplayName) + "-to-" + Normalize(GetShortTypeName(responseName));

            var metadata = new Dictionary<string, string>
            {
                ["request"] = requestName,
                ["hasResponse"] = (responseName != null).ToString(CultureInfo.InvariantCulture).ToLowerInvariant()
            };

            if (responseName != null)
                metadata["response"] = responseName;

            if (!string.IsNullOrWhiteSpace(serviceName))
                metadata["service"] = serviceName;

            manifest.AddComponent(new ComponentModel(pipelineId, "spider.pipeline", requestDisplayName, metadata, CreateEvidence(semanticModel, attachInvocation, null, sourceRoots)));

            var counts = CountPipelineStages(attachInvocation);
            var targetMetadata = new Dictionary<string, string>
            {
                ["hasOverride"] = (counts.Override > 0).ToString(CultureInfo.InvariantCulture)
            };

            if (targetMethod != null)
            {
                targetMetadata["target"] = targetMethod.Name;
                var targetSymbolId = GetSymbolId(targetMethod);
                if (!string.IsNullOrWhiteSpace(targetSymbolId))
                    targetMetadata["targetSymbolId"] = targetSymbolId;
            }

            AddPipelineStage(manifest, pipelineId, "pre-process", "Pre-process", counts.PreProcess, 1, null);
            AddPipelineStage(manifest, pipelineId, "middleware", "Middleware", counts.Middleware, 2, null);
            AddPipelineStage(manifest, pipelineId, "target", "Target", 1, 3, targetMetadata);
            AddPipelineStage(manifest, pipelineId, "parallel", "Parallel", counts.Parallel, 4, null);
            AddPipelineStage(manifest, pipelineId, "post-success", "Post-process success", counts.Success, 5, null);
            AddPipelineStage(manifest, pipelineId, "post-failure", "Post-process failure", counts.Failure, 6, null);
        }

        private static void AddPipelineStage(
            ManifestModel manifest,
            string pipelineId,
            string stageName,
            string displayName,
            int count,
            int order,
            Dictionary<string, string> extraMetadata)
        {
            var stageId = pipelineId + "." + stageName;
            var metadata = new Dictionary<string, string>
            {
                ["stage"] = stageName,
                ["count"] = count.ToString(CultureInfo.InvariantCulture),
                ["order"] = order.ToString(CultureInfo.InvariantCulture)
            };

            if (extraMetadata != null)
            {
                foreach (var pair in extraMetadata)
                    metadata[pair.Key] = pair.Value;
            }

            manifest.AddComponent(new ComponentModel(stageId, "spider.pipeline-stage", displayName, metadata, new EvidenceModel[0]));
            manifest.AddRelation(new RelationModel(pipelineId, stageId, "contains"));
        }

        private static PipelineStageCounts CountPipelineStages(InvocationExpressionSyntax attachInvocation)
        {
            var counts = new PipelineStageCounts();
            var lambda = attachInvocation.ArgumentList.Arguments
                .Select(argument => argument.Expression)
                .OfType<LambdaExpressionSyntax>()
                .FirstOrDefault();

            if (lambda == null)
                return counts;

            foreach (var invocation in lambda.Body.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                switch (GetInvocationName(invocation))
                {
                    case "PreProcess":
                    case "OnPreProcess":
                        counts.PreProcess++;
                        break;
                    case "UseMiddleware":
                    case "OnMiddleware":
                        counts.Middleware++;
                        break;
                    case "UseOverride":
                    case "OnTargeting":
                        counts.Override++;
                        break;
                    case "Parallel":
                    case "OnParallel":
                        counts.Parallel++;
                        break;
                    case "OnSuccess":
                        counts.Success++;
                        break;
                    case "OnFailure":
                        counts.Failure++;
                        break;
                }
            }

            return counts;
        }

        private static string GetBridgeServiceName(SemanticModel semanticModel, InvocationExpressionSyntax attachInvocation)
        {
            if (!(attachInvocation.Expression is MemberAccessExpressionSyntax memberAccess) ||
                !(memberAccess.Expression is InvocationExpressionSyntax bridgeInvocation) ||
                GetInvocationName(bridgeInvocation) != "InitBridge")
            {
                return null;
            }

            var genericTypes = GetGenericArguments(bridgeInvocation);
            return genericTypes.Count == 1 ? GetTypeName(semanticModel, genericTypes[0]) : null;
        }

        private static IMethodSymbol GetExecuteAsyncTargetMethod(SemanticModel semanticModel, InvocationExpressionSyntax attachInvocation)
        {
            var executeInvocation = BuildChain(attachInvocation)
                .FirstOrDefault(invocation => GetInvocationName(invocation) == "ExecuteAsync");
            if (executeInvocation == null || executeInvocation.ArgumentList.Arguments.Count == 0)
                return null;

            var targetExpression = executeInvocation.ArgumentList.Arguments[0].Expression;
            var targetInvocation = targetExpression
                .DescendantNodesAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(invocation => invocation.Expression is MemberAccessExpressionSyntax);
            if (targetInvocation == null)
                return null;

            return semanticModel.GetSymbolInfo(targetInvocation).Symbol as IMethodSymbol ??
                   semanticModel.GetSymbolInfo(targetInvocation).CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
        }

        private static void AddInvokedFlowRelations(ManifestModel manifest)
        {
            var flowsByDeclaringMember = manifest.Components
                .Where(component => component.Kind == "spider.flow" &&
                                    TryGetMetadata(component, "declaringMemberSymbolId", out _))
                .GroupBy(component => component.Metadata["declaringMemberSymbolId"], StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => (IReadOnlyCollection<ComponentModel>)group.ToArray(), StringComparer.Ordinal);

            foreach (var component in manifest.Components.ToArray())
            {
                AddInvokedFlowRelation(manifest, flowsByDeclaringMember, component, "actionSymbolId");
                AddInvokedFlowRelation(manifest, flowsByDeclaringMember, component, "targetSymbolId");
            }
        }

        private static void AddInvokedFlowRelation(
            ManifestModel manifest,
            IReadOnlyDictionary<string, IReadOnlyCollection<ComponentModel>> flowsByDeclaringMember,
            ComponentModel component,
            string metadataKey)
        {
            if (!TryGetMetadata(component, metadataKey, out var symbolId) ||
                !flowsByDeclaringMember.TryGetValue(symbolId, out var invokedFlows))
            {
                return;
            }

            foreach (var invokedFlow in invokedFlows)
            {
                if (component.Id == invokedFlow.Id)
                    continue;

                if (component.Kind == "spider.pipeline-stage")
                {
                    manifest.AddRelation(new RelationModel(component.Id, invokedFlow.Id, "invokes-flow"));

                    var pipeline = manifest.FindParent(component.Id, "contains", "spider.pipeline");
                    if (pipeline != null)
                        manifest.AddRelation(new RelationModel(pipeline.Id, invokedFlow.Id, "pipeline-invokes-flow"));

                    continue;
                }

                manifest.AddRelation(new RelationModel(component.Id, invokedFlow.Id, "invokes-flow"));
            }
        }

        private static Dictionary<string, string> CreateFlowStepMetadata(SemanticModel semanticModel, InvocationExpressionSyntax invocation, string invocationName)
        {
            var metadata = new Dictionary<string, string>
            {
                ["operation"] = invocationName
            };

            var genericArguments = GetGenericArguments(invocation);
            if (genericArguments.Count > 0)
                metadata["genericArguments"] = string.Join(",", genericArguments.Select(argument => GetTypeName(semanticModel, argument)));

            if (invocationName == "Branch" && genericArguments.Count > 0)
                metadata["branchType"] = GetTypeName(semanticModel, genericArguments[0]);

            var delegateExpression = GetDelegateExpression(invocation);
            if (delegateExpression != null && !(invocationName == "Branch" && delegateExpression is LambdaExpressionSyntax))
                metadata["delegate"] = GetExpressionDisplayName(delegateExpression);

            var actionSymbolId = GetExpressionSymbolId(semanticModel, delegateExpression);
            if (!string.IsNullOrWhiteSpace(actionSymbolId) && !(invocationName == "Branch" && delegateExpression is LambdaExpressionSyntax))
                metadata["actionSymbolId"] = actionSymbolId;

            if (invocationName == "ContinueIf" && invocation.ArgumentList.Arguments.Count > 1)
                metadata["otherwise"] = invocation.ArgumentList.Arguments[1].Expression.ToString();

            ApplyStepMetadata(invocation, invocationName, metadata);

            return metadata;
        }

        private static void AddBranchRoutes(
            ManifestModel manifest,
            SemanticModel semanticModel,
            InvocationExpressionSyntax branchInvocation,
            string branchId,
            IReadOnlyCollection<string> sourceRoots)
        {
            var branchLambda = GetDelegateExpression(branchInvocation) as LambdaExpressionSyntax;
            if (branchLambda == null)
                return;

            var routeInvocations = GetTopLevelFluentInvocations(branchLambda)
                .Where(invocation =>
                {
                    var name = GetInvocationName(invocation);
                    return name == "When" || name == "Otherwise";
                })
                .OrderBy(GetInvocationNamePosition)
                .ToArray();

            var routeIndex = 0;
            foreach (var routeInvocation in routeInvocations)
            {
                routeIndex++;
                var routeName = GetInvocationName(routeInvocation);
                var isOtherwise = routeName == "Otherwise";
                var conditionExpression = !isOtherwise && routeInvocation.ArgumentList.Arguments.Count > 0
                    ? routeInvocation.ArgumentList.Arguments[0].Expression
                    : null;
                var configureExpressionIndex = isOtherwise ? 0 : 1;
                var configureExpression = routeInvocation.ArgumentList.Arguments.Count > configureExpressionIndex
                    ? routeInvocation.ArgumentList.Arguments[configureExpressionIndex].Expression as LambdaExpressionSyntax
                    : null;
                var routeDisplayName = isOtherwise
                    ? "Otherwise"
                    : "When " + GetExpressionDisplayName(conditionExpression);
                var routeMetadata = new Dictionary<string, string>
                {
                    ["routeKind"] = isOtherwise ? "otherwise" : "when",
                    ["order"] = routeIndex.ToString(CultureInfo.InvariantCulture)
                };

                if (conditionExpression != null)
                {
                    routeMetadata["condition"] = GetExpressionDisplayName(conditionExpression);
                    var conditionSymbolId = GetExpressionSymbolId(semanticModel, conditionExpression);
                    if (!string.IsNullOrWhiteSpace(conditionSymbolId))
                        routeMetadata["conditionSymbolId"] = conditionSymbolId;
                }

                ApplyRouteMetadata(configureExpression, routeMetadata);
                routeDisplayName = GetDisplayName(routeMetadata, routeDisplayName);

                var routeId = branchId + ".route." + routeIndex.ToString("00", CultureInfo.InvariantCulture) + "-" + Normalize(routeDisplayName);
                var routeEvidence = CreateEvidence(semanticModel, routeInvocation, conditionExpression, sourceRoots);
                manifest.AddComponent(new ComponentModel(routeId, "spider.flow-branch-route", routeDisplayName, routeMetadata, routeEvidence));
                manifest.AddRelation(new RelationModel(branchId, routeId, "branch-route", new Dictionary<string, string>
                {
                    ["order"] = routeIndex.ToString(CultureInfo.InvariantCulture)
                }));

                AddBranchRouteSteps(manifest, semanticModel, configureExpression, routeId, sourceRoots);
            }
        }

        private static void AddBranchRouteSteps(
            ManifestModel manifest,
            SemanticModel semanticModel,
            LambdaExpressionSyntax routeLambda,
            string routeId,
            IReadOnlyCollection<string> sourceRoots)
        {
            if (routeLambda == null)
                return;

            var stepInvocations = GetTopLevelFluentInvocations(routeLambda)
                .Where(invocation =>
                {
                    var name = GetInvocationName(invocation);
                    return IsFlowStep(name);
                })
                .OrderBy(GetInvocationNamePosition)
                .ToArray();

            string previousStepId = null;
            var stepIndex = 0;

            foreach (var stepInvocation in stepInvocations)
            {
                stepIndex++;
                var invocationName = GetInvocationName(stepInvocation);
                var kind = GetFlowStepKind(invocationName);
                var metadata = CreateFlowStepMetadata(semanticModel, stepInvocation, invocationName);
                var displayName = GetDisplayName(metadata, GetStepDisplayName(semanticModel, stepInvocation, invocationName));
                var stepId = routeId + "." + stepIndex.ToString("000", CultureInfo.InvariantCulture) + "-" + Normalize(displayName);
                metadata["order"] = stepIndex.ToString(CultureInfo.InvariantCulture);
                var evidence = CreateEvidence(semanticModel, stepInvocation, GetDelegateExpression(stepInvocation), sourceRoots);

                manifest.AddComponent(new ComponentModel(stepId, kind, displayName, metadata, evidence));
                manifest.AddRelation(new RelationModel(routeId, stepId, "route-contains"));

                if (invocationName == "Branch")
                    AddBranchRoutes(manifest, semanticModel, stepInvocation, stepId, sourceRoots);

                if (previousStepId != null)
                    manifest.AddRelation(new RelationModel(previousStepId, stepId, "route-next"));

                previousStepId = stepId;
            }
        }

        private static IReadOnlyList<InvocationExpressionSyntax> BuildChain(InvocationExpressionSyntax firstInvocation)
        {
            var invocations = new List<InvocationExpressionSyntax> { firstInvocation };
            var current = firstInvocation;

            while (current.Parent is MemberAccessExpressionSyntax memberAccess &&
                   memberAccess.Expression == current &&
                   memberAccess.Parent is InvocationExpressionSyntax nextInvocation)
            {
                invocations.Add(nextInvocation);
                current = nextInvocation;
            }

            return invocations;
        }

        private static void ApplyStepMetadata(
            InvocationExpressionSyntax invocation,
            string invocationName,
            IDictionary<string, string> metadata)
        {
            var configureExpression = GetStepMetadataExpression(invocation, invocationName);
            if (configureExpression != null)
                ApplyMetadataFromLambda(metadata, configureExpression);

            if (invocationName == "Branch")
            {
                var branchExpression = GetDelegateExpression(invocation) as LambdaExpressionSyntax;
                if (branchExpression != null)
                    ApplyMetadataFromLambda(metadata, branchExpression);
            }
        }

        private static LambdaExpressionSyntax GetStepMetadataExpression(InvocationExpressionSyntax invocation, string invocationName)
        {
            var metadataArgumentIndex = invocationName == "ContinueIf" ? 2 : 1;
            if (invocationName == "Branch" || invocation.ArgumentList.Arguments.Count <= metadataArgumentIndex)
                return null;

            return invocation.ArgumentList.Arguments[metadataArgumentIndex].Expression as LambdaExpressionSyntax;
        }

        private static void ApplyRouteMetadata(LambdaExpressionSyntax routeLambda, IDictionary<string, string> metadata)
        {
            if (routeLambda == null)
                return;

            ApplyMetadataFromLambda(metadata, routeLambda);
        }

        private static void ApplyMetadataFromLambda(IDictionary<string, string> metadata, LambdaExpressionSyntax lambda)
            => ApplyMetadataInvocations(metadata, GetTopLevelFluentInvocations(lambda));

        private static void ApplyMetadataInvocations(
            IDictionary<string, string> metadata,
            IEnumerable<InvocationExpressionSyntax> invocations)
        {
            foreach (var invocation in invocations)
            {
                var invocationName = GetInvocationName(invocation);
                if (!IsMetadataInvocation(invocationName))
                    continue;

                if (invocationName == "Named")
                {
                    var name = GetStringArgument(invocation, 0);
                    if (!string.IsNullOrWhiteSpace(name))
                        metadata["name"] = name;

                    continue;
                }

                if (invocationName == "Describe")
                {
                    var description = GetStringArgument(invocation, 0);
                    if (!string.IsNullOrWhiteSpace(description))
                        metadata["description"] = description;

                    continue;
                }

                if (invocationName == "Tags")
                {
                    MergeTags(metadata, GetStringArguments(invocation));
                    continue;
                }

                if (invocationName == "Metadata")
                {
                    var key = GetStringArgument(invocation, 0);
                    var value = GetStringArgument(invocation, 1);
                    if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                        metadata[key] = value;
                }
            }
        }

        private static IReadOnlyList<InvocationExpressionSyntax> GetTopLevelFluentInvocations(LambdaExpressionSyntax lambda)
        {
            if (lambda.Body is ExpressionSyntax expression)
                return UnwindFluentInvocations(expression);

            if (lambda.Body is BlockSyntax block)
            {
                return block.Statements
                    .OfType<ExpressionStatementSyntax>()
                    .SelectMany(statement => UnwindFluentInvocations(statement.Expression))
                    .OrderBy(GetInvocationNamePosition)
                    .ToArray();
            }

            return Array.Empty<InvocationExpressionSyntax>();
        }

        private static IReadOnlyList<InvocationExpressionSyntax> UnwindFluentInvocations(ExpressionSyntax expression)
        {
            var stack = new Stack<InvocationExpressionSyntax>();
            var current = expression;

            while (current is InvocationExpressionSyntax invocation)
            {
                stack.Push(invocation);

                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    current = memberAccess.Expression;
                    continue;
                }

                break;
            }

            return stack.ToArray();
        }

        private static void MergeTags(IDictionary<string, string> metadata, IEnumerable<string> tags)
        {
            var values = new List<string>();
            if (metadata.TryGetValue("tags", out var existing))
                values.AddRange(existing.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            values.AddRange(tags);
            var merged = values
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (merged.Length > 0)
                metadata["tags"] = string.Join(",", merged);
        }

        private static string GetDisplayName(IReadOnlyDictionary<string, string> metadata, string fallback)
        {
            if (metadata.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
                return name;

            return fallback;
        }

        private static IReadOnlyList<TypeSyntax> GetGenericArguments(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax genericName)
            {
                return genericName.TypeArgumentList.Arguments.ToArray();
            }

            if (invocation.Expression is GenericNameSyntax directGenericName)
                return directGenericName.TypeArgumentList.Arguments.ToArray();

            return Array.Empty<TypeSyntax>();
        }

        private static string GetInvocationName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.ValueText;

            if (invocation.Expression is IdentifierNameSyntax identifier)
                return identifier.Identifier.ValueText;

            if (invocation.Expression is GenericNameSyntax genericName)
                return genericName.Identifier.ValueText;

            return string.Empty;
        }

        private static int GetInvocationNamePosition(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.SpanStart;

            if (invocation.Expression is IdentifierNameSyntax identifier)
                return identifier.SpanStart;

            if (invocation.Expression is GenericNameSyntax genericName)
                return genericName.SpanStart;

            return invocation.SpanStart;
        }

        private static string GetStringArgument(InvocationExpressionSyntax invocation, int index)
        {
            if (invocation.ArgumentList.Arguments.Count <= index)
                return null;

            var expression = invocation.ArgumentList.Arguments[index].Expression;
            if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                return literal.Token.ValueText;

            return null;
        }

        private static IReadOnlyList<string> GetStringArguments(InvocationExpressionSyntax invocation)
        {
            var values = new List<string>();
            for (var index = 0; index < invocation.ArgumentList.Arguments.Count; index++)
            {
                var value = GetStringArgument(invocation, index);
                if (!string.IsNullOrWhiteSpace(value))
                    values.Add(value);
            }

            return values;
        }

        private static ExpressionSyntax GetDelegateExpression(InvocationExpressionSyntax invocation)
        {
            if (invocation.ArgumentList.Arguments.Count == 0)
                return null;

            return invocation.ArgumentList.Arguments[0].Expression;
        }

        private static string GetStepDisplayName(SemanticModel semanticModel, InvocationExpressionSyntax invocation, string invocationName)
        {
            if (invocationName == "Branch")
            {
                var genericArguments = GetGenericArguments(invocation);
                if (genericArguments.Count > 0)
                    return GetShortTypeName(GetTypeName(semanticModel, genericArguments[0])) + " branch";

                return "Branch";
            }

            var delegateExpression = GetDelegateExpression(invocation);
            if (delegateExpression == null)
                return GetInvocationName(invocation);

            return GetExpressionDisplayName(delegateExpression);
        }

        private static string GetExpressionDisplayName(ExpressionSyntax expression)
        {
            if (expression is IdentifierNameSyntax identifier)
                return identifier.Identifier.ValueText;

            if (expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.ValueText;

            if (expression is LambdaExpressionSyntax)
                return "lambda";

            return expression.ToString();
        }

        private static bool IsFlowStep(string invocationName)
            => invocationName == "Then" ||
               invocationName == "ThenWith" ||
               invocationName == "ContinueIf" ||
               invocationName == "Branch";

        private static bool IsMetadataInvocation(string invocationName)
            => invocationName == "Named" ||
               invocationName == "Describe" ||
               invocationName == "Tags" ||
               invocationName == "Metadata";

        private static string GetFlowStepKind(string invocationName)
        {
            if (invocationName == "ContinueIf")
                return "spider.flow-condition";

            if (invocationName == "Branch")
                return "spider.flow-branch";

            return "spider.flow-step";
        }

        private static EvidenceModel[] CreateEvidence(
            SemanticModel semanticModel,
            SyntaxNode node,
            ExpressionSyntax referencedExpression,
            IReadOnlyCollection<string> sourceRoots)
        {
            var evidenceNode = GetEvidenceNode(semanticModel, node, referencedExpression);
            return CreateEvidenceFromNode(evidenceNode, sourceRoots);
        }

        private static SyntaxNode GetEvidenceNode(SemanticModel semanticModel, SyntaxNode fallbackNode, ExpressionSyntax referencedExpression)
        {
            if (referencedExpression == null)
                return fallbackNode;

            if (referencedExpression is LambdaExpressionSyntax)
                return referencedExpression;

            var symbol = semanticModel.GetSymbolInfo(referencedExpression).Symbol ??
                         semanticModel.GetSymbolInfo(referencedExpression).CandidateSymbols.FirstOrDefault();
            var declaringSyntax = symbol?.DeclaringSyntaxReferences.FirstOrDefault();
            if (declaringSyntax == null)
                return fallbackNode;

            return declaringSyntax.GetSyntax();
        }

        private static string GetExpressionSymbolId(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            if (expression == null)
                return null;

            var symbol = semanticModel.GetSymbolInfo(expression).Symbol ??
                         semanticModel.GetSymbolInfo(expression).CandidateSymbols.FirstOrDefault();
            return GetSymbolId(symbol);
        }

        private static string GetEnclosingSymbolId(SemanticModel semanticModel, SyntaxNode node)
            => GetSymbolId(semanticModel.GetEnclosingSymbol(node.SpanStart));

        private static string GetSymbolId(ISymbol symbol)
        {
            if (symbol == null)
                return null;

            return DocumentationCommentId.CreateDeclarationId(symbol.OriginalDefinition) ??
                   DocumentationCommentId.CreateDeclarationId(symbol) ??
                   symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private static EvidenceModel[] CreateEvidenceFromNode(SyntaxNode node, IReadOnlyCollection<string> sourceRoots)
        {
            var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
            var containingType = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            var containingMember = node as MemberDeclarationSyntax ??
                                   node.Ancestors().OfType<MemberDeclarationSyntax>()
                                       .FirstOrDefault(member => !(member is TypeDeclarationSyntax));

            return new[]
            {
                new EvidenceModel(
                    "source-generator",
                    NormalizeSourcePath(lineSpan.Path, sourceRoots),
                    lineSpan.StartLinePosition.Line + 1,
                    containingType?.Identifier.ValueText ?? string.Empty,
                    GetContainingMemberName(containingMember))
            };
        }

        private static IReadOnlyCollection<string> CreateSourceRoots(GeneratorExecutionContext context)
        {
            var roots = new List<string>();

            AddGlobalOption(context, roots, "build_property.ProjectDir");
            AddGlobalOption(context, roots, "build_property.MSBuildProjectDirectory");
            AddGlobalOption(context, roots, "build_property.SolutionDir");

            return roots
                .Select(NormalizeDirectoryPath)
                .Where(root => !string.IsNullOrWhiteSpace(root))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static void AddGlobalOption(GeneratorExecutionContext context, ICollection<string> roots, string key)
        {
            string value;
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value))
                roots.Add(value);
        }

        private static string NormalizeSourcePath(string path, IReadOnlyCollection<string> sourceRoots)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            var normalizedPath = NormalizePath(path);
            if (!IsRootedSourcePath(path))
                return normalizedPath;

            foreach (var sourceRoot in sourceRoots ?? Array.Empty<string>())
            {
                var relativePath = TryMakeRelativePath(sourceRoot, normalizedPath);
                if (!string.IsNullOrWhiteSpace(relativePath))
                    return relativePath;
            }

            return GetFileName(normalizedPath);
        }

        private static string TryMakeRelativePath(string sourceRoot, string sourcePath)
        {
            var root = NormalizeDirectoryPath(sourceRoot);
            var path = NormalizePath(sourcePath);

            if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(path))
                return null;

            var comparison = HasWindowsDrive(root) || HasWindowsDrive(path)
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!root.EndsWith("/", StringComparison.Ordinal))
                root += "/";

            if (!path.StartsWith(root, comparison))
                return null;

            var relativePath = path.Substring(root.Length).TrimStart('/');
            return string.IsNullOrWhiteSpace(relativePath) ? GetFileName(path) : relativePath;
        }

        private static string NormalizeDirectoryPath(string path)
            => NormalizePath(path).TrimEnd('/');

        private static string NormalizePath(string path)
            => (path ?? string.Empty).Replace('\\', '/');

        private static bool IsRootedSourcePath(string path)
            => !string.IsNullOrWhiteSpace(path) &&
               (Path.IsPathRooted(path) || HasWindowsDrive(path));

        private static bool HasWindowsDrive(string path)
            => !string.IsNullOrWhiteSpace(path) &&
               path.Length >= 3 &&
               char.IsLetter(path[0]) &&
               path[1] == ':' &&
               (path[2] == '\\' || path[2] == '/');

        private static string GetFileName(string path)
        {
            var normalizedPath = NormalizePath(path);
            var index = normalizedPath.LastIndexOf('/');
            return index < 0 ? normalizedPath : normalizedPath.Substring(index + 1);
        }

        private static string GetContainingMemberName(MemberDeclarationSyntax containingMember)
        {
            if (containingMember is MethodDeclarationSyntax method)
                return method.Identifier.ValueText;

            if (containingMember is ConstructorDeclarationSyntax constructor)
                return constructor.Identifier.ValueText;

            if (containingMember is PropertyDeclarationSyntax property)
                return property.Identifier.ValueText;

            return string.Empty;
        }

        private static string GetTypeName(SemanticModel semanticModel, TypeSyntax typeSyntax)
        {
            var symbol = semanticModel.GetSymbolInfo(typeSyntax).Symbol as ITypeSymbol;
            return symbol == null ? typeSyntax.ToString() : symbol.ToDisplayString();
        }

        private static string GetShortTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return string.Empty;

            var index = typeName.LastIndexOf('.');
            return index < 0 ? typeName : typeName.Substring(index + 1);
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unnamed";

            var normalized = Regex.Replace(value.Trim(), "([a-z0-9])([A-Z])", "$1-$2");
            normalized = Regex.Replace(normalized, "[^A-Za-z0-9]+", "-");
            return normalized.Trim('-').ToLowerInvariant();
        }

        private static bool TryGetMetadata(ComponentModel component, string key, out string value)
            => component.Metadata.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value);

        private static string GenerateSource(ManifestModel manifest)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("namespace Spider.Pipelines.Generated");
            builder.AppendLine("{");
            builder.AppendLine("    internal static partial class SpiderGeneratedArchitecture");
            builder.AppendLine("    {");
            builder.AppendLine("        public static global::Spider.Pipelines.Architecture.SpiderArchitectureManifest BuildManifest()");
            builder.AppendLine("        {");
            builder.AppendLine("            return new global::Spider.Pipelines.Architecture.SpiderArchitectureManifest(");
            builder.AppendLine("                new global::Spider.Pipelines.Architecture.SpiderComponentDescriptor[]");
            builder.AppendLine("                {");

            foreach (var component in manifest.Components.OrderBy(component => component.Id, StringComparer.Ordinal))
                AppendComponent(builder, component);

            builder.AppendLine("                },");
            builder.AppendLine("                new global::Spider.Pipelines.Architecture.SpiderRelationDescriptor[]");
            builder.AppendLine("                {");

            foreach (var relation in manifest.Relations.OrderBy(relation => relation.Id, StringComparer.Ordinal))
                AppendRelation(builder, relation);

            builder.AppendLine("                });");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendComponent(StringBuilder builder, ComponentModel component)
        {
            builder.AppendLine("                    new global::Spider.Pipelines.Architecture.SpiderComponentDescriptor(");
            builder.AppendLine("                        " + Literal(component.Id) + ",");
            builder.AppendLine("                        " + Literal(component.Kind) + ",");
            builder.AppendLine("                        " + Literal(component.DisplayName) + ",");
            AppendDictionary(builder, component.Metadata, "                        ");
            builder.AppendLine(",");
            builder.AppendLine("                        new global::Spider.Pipelines.Architecture.SpiderEvidenceDescriptor[]");
            builder.AppendLine("                        {");

            foreach (var evidence in component.Evidence)
            {
                builder.AppendLine("                            new global::Spider.Pipelines.Architecture.SpiderEvidenceDescriptor(");
                builder.AppendLine("                                " + Literal(evidence.SourceKind) + ",");
                builder.AppendLine("                                " + Literal(evidence.FilePath) + ",");
                builder.AppendLine("                                " + evidence.LineNumber.ToString(CultureInfo.InvariantCulture) + ",");
                builder.AppendLine("                                " + Literal(evidence.TypeName) + ",");
                builder.AppendLine("                                " + Literal(evidence.MemberName) + "),");
            }

            builder.AppendLine("                        }),");
        }

        private static void AppendRelation(StringBuilder builder, RelationModel relation)
        {
            builder.AppendLine("                    new global::Spider.Pipelines.Architecture.SpiderRelationDescriptor(");
            builder.AppendLine("                        " + Literal(relation.Id) + ",");
            builder.AppendLine("                        " + Literal(relation.SourceId) + ",");
            builder.AppendLine("                        " + Literal(relation.TargetId) + ",");
            builder.AppendLine("                        " + Literal(relation.Kind) + ",");
            AppendDictionary(builder, relation.Metadata, "                        ");
            builder.AppendLine("),");
        }

        private static void AppendDictionary(StringBuilder builder, IReadOnlyDictionary<string, string> dictionary, string indent)
        {
            builder.AppendLine(indent + "new global::System.Collections.Generic.Dictionary<string, string>");
            builder.AppendLine(indent + "{");

            foreach (var pair in dictionary.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                builder.AppendLine(indent + "    [" + Literal(pair.Key) + "] = " + Literal(pair.Value) + ",");

            builder.Append(indent + "}");
        }

        private static string Literal(string value)
            => "@\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";

        private sealed class Receiver : ISyntaxReceiver
        {
            public List<InvocationExpressionSyntax> ComposeFlowInvocations { get; } = new List<InvocationExpressionSyntax>();

            public List<InvocationExpressionSyntax> AttachInvocations { get; } = new List<InvocationExpressionSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (!(syntaxNode is InvocationExpressionSyntax invocation))
                    return;

                var name = GetInvocationName(invocation);
                if (name == "ComposeFlow")
                    ComposeFlowInvocations.Add(invocation);
                else if (name == "Attach")
                    AttachInvocations.Add(invocation);
            }
        }

        private sealed class ManifestModel
        {
            private readonly Dictionary<string, ComponentModel> _components = new Dictionary<string, ComponentModel>(StringComparer.Ordinal);
            private readonly Dictionary<string, RelationModel> _relations = new Dictionary<string, RelationModel>(StringComparer.Ordinal);

            public IEnumerable<ComponentModel> Components => _components.Values;

            public IEnumerable<RelationModel> Relations => _relations.Values;

            public void AddComponent(ComponentModel component)
                => _components[component.Id] = component;

            public void AddRelation(RelationModel relation)
                => _relations[relation.Id] = relation;

            public ComponentModel FindParent(string childId, string relationKind, string parentKind)
            {
                var relation = _relations.Values.FirstOrDefault(candidate =>
                    candidate.Kind == relationKind &&
                    candidate.TargetId == childId &&
                    _components.TryGetValue(candidate.SourceId, out var parent) &&
                    parent.Kind == parentKind);

                return relation == null ? null : _components[relation.SourceId];
            }
        }

        private sealed class ComponentModel
        {
            public ComponentModel(string id, string kind, string displayName, IReadOnlyDictionary<string, string> metadata, IReadOnlyCollection<EvidenceModel> evidence)
            {
                Id = id;
                Kind = kind;
                DisplayName = displayName;
                Metadata = metadata ?? new Dictionary<string, string>();
                Evidence = evidence ?? Array.Empty<EvidenceModel>();
            }

            public string Id { get; }

            public string Kind { get; }

            public string DisplayName { get; }

            public IReadOnlyDictionary<string, string> Metadata { get; }

            public IReadOnlyCollection<EvidenceModel> Evidence { get; }
        }

        private sealed class RelationModel
        {
            public RelationModel(string sourceId, string targetId, string kind)
                : this(sourceId, targetId, kind, new Dictionary<string, string>())
            {
            }

            public RelationModel(string sourceId, string targetId, string kind, IReadOnlyDictionary<string, string> metadata)
            {
                SourceId = sourceId;
                TargetId = targetId;
                Kind = kind;
                Id = kind + ":" + sourceId + "->" + targetId;
                Metadata = metadata ?? new Dictionary<string, string>();
            }

            public string Id { get; }

            public string SourceId { get; }

            public string TargetId { get; }

            public string Kind { get; }

            public IReadOnlyDictionary<string, string> Metadata { get; }
        }

        private sealed class EvidenceModel
        {
            public EvidenceModel(string sourceKind, string filePath, int lineNumber, string typeName, string memberName)
            {
                SourceKind = sourceKind;
                FilePath = filePath;
                LineNumber = lineNumber;
                TypeName = typeName;
                MemberName = memberName;
            }

            public string SourceKind { get; }

            public string FilePath { get; }

            public int LineNumber { get; }

            public string TypeName { get; }

            public string MemberName { get; }
        }

        private sealed class PipelineStageCounts
        {
            public int PreProcess { get; set; }

            public int Middleware { get; set; }

            public int Override { get; set; }

            public int Parallel { get; set; }

            public int Success { get; set; }

            public int Failure { get; set; }
        }
    }
}
