namespace Spider.Pipelines.Samples.Web
{
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Flows;

    /// <summary>
    /// Coordinates complex credit offer flows used to stress the architecture graph sample.
    /// </summary>
    public sealed class CreditOfferWorkflow
    {
        private readonly ISpider _spider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditOfferWorkflow"/> class.
        /// </summary>
        /// <param name="spider">The Spider facade used to compose the sample flows.</param>
        public CreditOfferWorkflow(ISpider spider)
        {
            _spider = spider ?? throw new ArgumentNullException(nameof(spider));
        }

        /// <summary>
        /// Builds a complex credit offer using multiple branches and linked subflows.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The generated credit offer.</returns>
        public Task<CreditOffer> BuildComplexOfferAsync(
            CreditApplicationRequest request,
            CancellationToken cancellationToken)
            => _spider
                .ComposeFlow<CreditApplicationRequest, CreditOffer>("Complex loan origination")
                .Describe("Runs a richer loan origination process with bureau routing, fraud screening, pricing, contracts, and dispatch.")
                .Tags("complex-sample", "loan-origination", "graph")
                .Metadata("audience", "architecture review")
                .UsingProfile("Web verbose telemetry")
                .Then(CreditEvaluationFlowSteps.ValidateApplicationAsync, step => step
                    .Named("Validate application")
                    .Describe("Stops the flow when the incoming application is missing required identifiers.")
                    .Tags("validation", "guard"))
                .Then(CreditEvaluationFlowSteps.BuildBureauRequest, step => step
                    .Named("Build bureau request")
                    .Describe("Maps the application into the request used by all bureau routes.")
                    .Tags("mapping", "bureau"))
                .Branch<BureauDecision>(branch => branch
                    .Named("Resolve bureau decision")
                    .Describe("Chooses between cached, manual, and remote bureau paths.")
                    .Tags("branch", "bureau")
                    .When(ComplexCreditOfferSteps.CanUseCachedBureau, cached => cached
                        .Named("Cached bureau lane")
                        .Describe("Uses a local bureau decision for low exposure requests.")
                        .Tags("cache", "fast-path")
                        .Then(CreditEvaluationFlowSteps.UseCachedBureau, step => step
                            .Named("Use cached bureau decision")
                            .Tags("cache", "bureau")))
                    .When(ComplexCreditOfferSteps.RequiresManualBureauReview, manual => manual
                        .Named("Manual bureau lane")
                        .Describe("Models a manual bureau review route with an audit side effect.")
                        .Tags("manual-review", "bureau")
                        .Then(ComplexCreditOfferSteps.CreateManualBureauDecision, step => step
                            .Named("Create manual bureau decision")
                            .Tags("manual-review"))
                        .Then(ComplexCreditOfferSteps.RecordManualBureauReviewAsync, step => step
                            .Named("Record manual bureau audit")
                            .Tags("audit")))
                    .Otherwise(remote => remote
                        .Named("Remote bureau lane")
                        .Describe("Calls and evaluates a remote bureau provider for larger requests.")
                        .Tags("remote", "bureau")
                        .Then(CreditEvaluationFlowSteps.CallBureauAsync, step => step
                            .Named("Call bureau service")
                            .Tags("remote-call"))
                        .Then(CreditEvaluationFlowSteps.EvaluateBureauResponse, step => step
                            .Named("Evaluate bureau response")
                            .Tags("evaluation"))
                        .Branch<BureauDecision>(remoteReview => remoteReview
                            .Named("Classify remote bureau result")
                            .Describe("Nested branch inside the remote bureau route.")
                            .Tags("nested-branch", "remote", "bureau")
                            .When(ComplexCreditOfferSteps.RemoteBureauNeedsEscalation, escalation => escalation
                                .Named("Escalate remote result")
                                .Describe("Captures escalation work for a rejected remote bureau response.")
                                .Tags("escalation", "remote")
                                .Then(ComplexCreditOfferSteps.RecordRemoteBureauEscalation, step => step
                                    .Named("Record remote escalation")
                                    .Tags("audit", "escalation")))
                            .Otherwise(clear => clear
                                .Named("Remote result cleared")
                                .Describe("Captures audit work for a cleared remote bureau response.")
                                .Tags("audit", "remote")
                                .Then(ComplexCreditOfferSteps.RecordRemoteBureauClearance, step => step
                                    .Named("Record remote clearance")
                                    .Tags("audit", "clearance"))))))
                .ThenWith<CreditApplicationRequest, BureauDecision, UnderwritingPacket>(ComplexCreditOfferSteps.BuildUnderwritingPacket, step => step
                    .Named("Build underwriting packet")
                    .Describe("Combines the original request and bureau result into underwriting context.")
                    .Tags("underwriting", "mapping"))
                .Then(ComplexCreditOfferSteps.CaptureParallelUnderwritingSignalsAsync, step => step
                    .Named("Capture underwriting signals")
                    .Describe("Documents independent underwriting checks that conceptually run in parallel.")
                    .Tags("parallel", "signals")
                    .Metadata("role", "parallel"))
                .Then(ComplexCreditOfferSteps.InspectSupportingDocuments, step => step
                    .Named("Inspect supporting documents")
                    .Describe("Documents a foreach-style inspection over supporting documents.")
                    .Tags("foreach", "documents")
                    .Metadata("role", "foreach"))
                .Then(ComplexCreditOfferSteps.PrepareExposureBatch, step => step
                    .Named("Prepare exposure batch")
                    .Describe("Documents a batch of exposure checks before fraud screening.")
                    .Tags("batch", "exposure")
                    .Metadata("role", "batch"))
                .Branch<FraudScreeningResult>(branch => branch
                    .Named("Screen fraud risk")
                    .Describe("Routes fraud screening through fast-track, enhanced, or standard lanes.")
                    .Tags("branch", "fraud")
                    .When(ComplexCreditOfferSteps.CanFastTrackFraud, fast => fast
                        .Named("Fast-track screening")
                        .Describe("Uses the shortest path for low exposure, bureau-approved requests.")
                        .Tags("fast-path", "fraud")
                        .Then(ComplexCreditOfferSteps.FastTrackFraudScreening, step => step
                            .Named("Create fast-track result")
                            .Tags("fraud", "fast-path")))
                    .When(ComplexCreditOfferSteps.RequiresEnhancedFraudReview, enhanced => enhanced
                        .Named("Enhanced screening")
                        .Describe("Runs the deeper fraud path for high exposure or bureau rejection.")
                        .Tags("enhanced", "fraud")
                        .Then(ComplexCreditOfferSteps.RunEnhancedFraudScreeningAsync, step => step
                            .Named("Run enhanced fraud screening")
                            .Tags("fraud", "enhanced")))
                    .Otherwise(standard => standard
                        .Named("Standard screening")
                        .Describe("Runs the default fraud screening path.")
                        .Tags("standard", "fraud")
                        .Then(ComplexCreditOfferSteps.RunStandardFraudScreening, step => step
                            .Named("Run standard fraud screening")
                            .Tags("fraud", "standard"))))
                .ContinueIf(ComplexCreditOfferSteps.FraudCleared, Flow.Throw(() => new InvalidOperationException("Fraud screening blocked the application.")), step => step
                    .Named("Fraud clearance gate")
                    .Describe("Faults the flow when fraud screening does not clear the application.")
                    .Tags("guard", "fraud"))
                .ThenWith<CreditApplicationRequest, FraudScreeningResult, PricingInput>(ComplexCreditOfferSteps.BuildPricingInput, step => step
                    .Named("Build pricing input")
                    .Describe("Combines the original request and fraud result before pricing.")
                    .Tags("pricing", "mapping"))
                .Branch<CreditOffer>(branch => branch
                    .Named("Select pricing strategy")
                    .Describe("Chooses promotional, risk-adjusted, or standard pricing.")
                    .Tags("branch", "pricing")
                    .When(ComplexCreditOfferSteps.UsesPromotionalPricing, promotional => promotional
                        .Named("Promotional pricing")
                        .Describe("Applies promotional pricing to low exposure approved requests.")
                        .Tags("pricing", "promotion")
                        .Then(ComplexCreditOfferSteps.BuildPromotionalOffer, step => step
                            .Named("Build promotional offer")
                            .Tags("offer", "promotion")))
                    .When(ComplexCreditOfferSteps.UsesRiskAdjustedPricing, risk => risk
                        .Named("Risk-adjusted pricing")
                        .Describe("Applies conditional pricing to higher exposure requests.")
                        .Tags("pricing", "risk-adjusted")
                        .Then(ComplexCreditOfferSteps.BuildRiskAdjustedOffer, step => step
                            .Named("Build risk-adjusted offer")
                            .Tags("offer", "risk-adjusted")))
                    .Otherwise(standard => standard
                        .Named("Standard pricing")
                        .Describe("Applies the standard pricing lane.")
                        .Tags("pricing", "standard")
                        .Then(ComplexCreditOfferSteps.BuildStandardOffer, step => step
                            .Named("Build standard offer")
                            .Tags("offer", "standard"))))
                .Then(PrepareOfferForContractAsync, step => step
                    .Named("Prepare contract")
                    .Describe("Delegates contract preparation to a linked subflow.")
                    .Tags("contract", "linked-flow"))
                .Then(PrepareOfferDispatchAsync, step => step
                    .Named("Prepare dispatch")
                    .Describe("Delegates dispatch preparation to a linked subflow.")
                    .Tags("dispatch", "linked-flow"))
                .RunAsync(request, cancellationToken);

        /// <summary>
        /// Prepares contract metadata for an offer through a linked subflow.
        /// </summary>
        /// <param name="offer">The generated offer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The offer enriched with contract metadata.</returns>
        public Task<CreditOffer> PrepareOfferForContractAsync(
            CreditOffer offer,
            CancellationToken cancellationToken)
            => _spider
                .ComposeFlow<CreditOffer, CreditOffer>("Prepare offer contract")
                .Describe("Prepares the contract lane for generated offers and early-returns when no contract is required.")
                .Tags("contract", "subflow")
                .UsingProfile("Background telemetry")
                .ContinueIf(ComplexCreditOfferSteps.RequiresContract, Flow.Return<CreditOffer, CreditOffer>(ComplexCreditOfferSteps.SkipContract), step => step
                    .Named("Requires contract")
                    .Describe("Returns the offer immediately when contract generation is not required.")
                    .Tags("guard", "contract"))
                .Then(ComplexCreditOfferSteps.BuildContractEnvelope, step => step
                    .Named("Build contract envelope")
                    .Describe("Creates the contract envelope used by legal and standard routes.")
                    .Tags("contract", "mapping"))
                .Branch<ReviewedContractEnvelope>(branch => branch
                    .Named("Select contract review")
                    .Describe("Chooses legal or standard contract terms.")
                    .Tags("branch", "contract")
                    .When(ComplexCreditOfferSteps.RequiresLegalReview, legal => legal
                        .Named("Legal review")
                        .Describe("Adds legal review terms for conditional offers.")
                        .Tags("legal", "contract")
                        .Then(ComplexCreditOfferSteps.AttachLegalReviewTerms, step => step
                            .Named("Attach legal terms")
                            .Tags("legal")))
                    .Otherwise(standard => standard
                        .Named("Standard terms")
                        .Describe("Adds standard contract terms.")
                        .Tags("standard", "contract")
                        .Then(ComplexCreditOfferSteps.AttachStandardTerms, step => step
                            .Named("Attach standard terms")
                            .Tags("standard"))))
                .ThenWith<CreditOffer, ReviewedContractEnvelope, CreditOffer>(ComplexCreditOfferSteps.AttachContractToOffer, step => step
                    .Named("Attach contract to offer")
                    .Describe("Returns the offer annotated with the selected contract template.")
                    .Tags("contract", "offer"))
                .RunAsync(offer, cancellationToken);

        /// <summary>
        /// Prepares dispatch metadata for an offer through a linked subflow.
        /// </summary>
        /// <param name="offer">The generated offer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The offer enriched with dispatch metadata.</returns>
        public Task<CreditOffer> PrepareOfferDispatchAsync(
            CreditOffer offer,
            CancellationToken cancellationToken)
            => _spider
                .ComposeFlow<CreditOffer, CreditOffer>("Prepare offer dispatch")
                .Describe("Routes the generated offer into priority, relationship manager, or digital dispatch lanes.")
                .Tags("dispatch", "subflow")
                .UsingProfile("Background telemetry")
                .Then(ComplexCreditOfferSteps.BuildOfferDispatchPlan, step => step
                    .Named("Build dispatch plan")
                    .Describe("Creates the dispatch plan from the generated offer.")
                    .Tags("dispatch", "mapping"))
                .Branch<ResolvedOfferDispatchPlan>(branch => branch
                    .Named("Select dispatch lane")
                    .Describe("Chooses the best dispatch lane for the offer.")
                    .Tags("branch", "dispatch")
                    .When(ComplexCreditOfferSteps.RequiresPriorityDispatch, priority => priority
                        .Named("Priority dispatch")
                        .Describe("Sends high value offers through the priority desk.")
                        .Tags("priority", "dispatch")
                        .Then(ComplexCreditOfferSteps.SchedulePriorityDispatchAsync, step => step
                            .Named("Schedule priority dispatch")
                            .Tags("priority")))
                    .When(ComplexCreditOfferSteps.RequiresRelationshipManager, manager => manager
                        .Named("Relationship manager")
                        .Describe("Assigns conditional offers to a relationship manager.")
                        .Tags("manager", "dispatch")
                        .Then(ComplexCreditOfferSteps.AssignRelationshipManager, step => step
                            .Named("Assign relationship manager")
                            .Tags("manager")))
                    .Otherwise(digital => digital
                        .Named("Digital delivery")
                        .Describe("Uses the default digital delivery lane.")
                        .Tags("digital", "dispatch")
                        .Then(ComplexCreditOfferSteps.ScheduleDigitalDelivery, step => step
                            .Named("Schedule digital delivery")
                            .Tags("digital"))))
                .ThenWith<CreditOffer, ResolvedOfferDispatchPlan, CreditOffer>(ComplexCreditOfferSteps.MarkOfferReadyForDispatch, step => step
                    .Named("Mark offer ready")
                    .Describe("Returns the offer annotated with the selected dispatch lane.")
                    .Tags("dispatch", "offer"))
                .RunAsync(offer, cancellationToken);
    }
}
