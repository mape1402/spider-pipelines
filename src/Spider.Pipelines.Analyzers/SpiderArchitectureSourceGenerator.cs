namespace Spider.Pipelines.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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

            foreach (var invocation in receiver.ComposeFlowInvocations)
                TryReadFlow(context.Compilation, invocation, manifest);

            foreach (var invocation in receiver.AttachInvocations)
                TryReadPipeline(context.Compilation, invocation, manifest);

            context.AddSource(
                "SpiderGeneratedArchitecture.g.cs",
                SourceText.From(GenerateSource(manifest), Encoding.UTF8));
        }

        private static void TryReadFlow(Compilation compilation, InvocationExpressionSyntax composeInvocation, ManifestModel manifest)
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
            var flowEvidence = CreateEvidence(semanticModel, composeInvocation, null);
            var metadata = new Dictionary<string, string>
            {
                ["request"] = GetTypeName(semanticModel, genericTypes[0]),
                ["hasResponse"] = (genericTypes.Count == 2).ToString(CultureInfo.InvariantCulture).ToLowerInvariant()
            };

            if (genericTypes.Count == 2)
                metadata["response"] = GetTypeName(semanticModel, genericTypes[1]);

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
                    manifest.AddComponent(new ComponentModel(profileId, "spider.flow-profile", profileName, new Dictionary<string, string>(), CreateEvidence(semanticModel, invocation, null)));
                    manifest.AddRelation(new RelationModel(flowId, profileId, "uses-profile"));
                    continue;
                }

                if (!IsFlowStep(invocationName))
                    continue;

                stepIndex++;
                var displayName = GetStepDisplayName(invocation);
                var kind = GetFlowStepKind(invocationName);
                var stepId = flowId + "." + stepIndex.ToString("000", CultureInfo.InvariantCulture) + "-" + Normalize(displayName);
                var stepMetadata = CreateFlowStepMetadata(semanticModel, invocation, invocationName);
                var evidence = CreateEvidence(semanticModel, invocation, GetDelegateExpression(invocation));

                manifest.AddComponent(new ComponentModel(stepId, kind, displayName, stepMetadata, evidence));
                manifest.AddRelation(new RelationModel(flowId, stepId, "contains"));

                if (previousStepId != null)
                    manifest.AddRelation(new RelationModel(previousStepId, stepId, "next"));

                previousStepId = stepId;
            }
        }

        private static void TryReadPipeline(Compilation compilation, InvocationExpressionSyntax attachInvocation, ManifestModel manifest)
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

            manifest.AddComponent(new ComponentModel(pipelineId, "spider.pipeline", requestDisplayName, metadata, CreateEvidence(semanticModel, attachInvocation, null)));

            var counts = CountPipelineStages(attachInvocation);
            AddPipelineStage(manifest, pipelineId, "pre-process", "Pre-process", counts.PreProcess, 1, null);
            AddPipelineStage(manifest, pipelineId, "middleware", "Middleware", counts.Middleware, 2, null);
            AddPipelineStage(manifest, pipelineId, "target", "Target", 1, 3, new Dictionary<string, string>
            {
                ["hasOverride"] = (counts.Override > 0).ToString(CultureInfo.InvariantCulture)
            });
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

        private static Dictionary<string, string> CreateFlowStepMetadata(SemanticModel semanticModel, InvocationExpressionSyntax invocation, string invocationName)
        {
            var metadata = new Dictionary<string, string>
            {
                ["operation"] = invocationName
            };

            var genericArguments = GetGenericArguments(invocation);
            if (genericArguments.Count > 0)
                metadata["genericArguments"] = string.Join(",", genericArguments.Select(argument => GetTypeName(semanticModel, argument)));

            var delegateExpression = GetDelegateExpression(invocation);
            if (delegateExpression != null)
                metadata["delegate"] = GetExpressionDisplayName(delegateExpression);

            if (invocationName == "ContinueIf" && invocation.ArgumentList.Arguments.Count > 1)
                metadata["otherwise"] = invocation.ArgumentList.Arguments[1].Expression.ToString();

            return metadata;
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

        private static string GetStringArgument(InvocationExpressionSyntax invocation, int index)
        {
            if (invocation.ArgumentList.Arguments.Count <= index)
                return null;

            var expression = invocation.ArgumentList.Arguments[index].Expression;
            if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                return literal.Token.ValueText;

            return null;
        }

        private static ExpressionSyntax GetDelegateExpression(InvocationExpressionSyntax invocation)
        {
            if (invocation.ArgumentList.Arguments.Count == 0)
                return null;

            return invocation.ArgumentList.Arguments[0].Expression;
        }

        private static string GetStepDisplayName(InvocationExpressionSyntax invocation)
        {
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

        private static string GetFlowStepKind(string invocationName)
        {
            if (invocationName == "ContinueIf")
                return "spider.flow-condition";

            if (invocationName == "Branch")
                return "spider.flow-branch";

            return "spider.flow-step";
        }

        private static EvidenceModel[] CreateEvidence(SemanticModel semanticModel, SyntaxNode node, ExpressionSyntax referencedExpression)
        {
            var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
            var containingType = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            var containingMember = node.Ancestors().OfType<MemberDeclarationSyntax>()
                .FirstOrDefault(member => !(member is TypeDeclarationSyntax));

            return new[]
            {
                new EvidenceModel(
                    "source-generator",
                    lineSpan.Path ?? string.Empty,
                    lineSpan.StartLinePosition.Line + 1,
                    containingType?.Identifier.ValueText ?? string.Empty,
                    GetEvidenceMemberName(semanticModel, containingMember, referencedExpression))
            };
        }

        private static string GetEvidenceMemberName(SemanticModel semanticModel, MemberDeclarationSyntax containingMember, ExpressionSyntax referencedExpression)
        {
            if (referencedExpression != null)
            {
                var symbol = semanticModel.GetSymbolInfo(referencedExpression).Symbol;
                if (symbol != null)
                    return symbol.Name;

                if (referencedExpression is LambdaExpressionSyntax)
                    return "<lambda>";
            }

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
