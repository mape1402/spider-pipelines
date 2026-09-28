namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Contains richer executable steps used to render complex flow graphs in the sample application.
    /// </summary>
    public static class ComplexCreditOfferSteps
    {
        /// <summary>
        /// Determines whether cached bureau data can be used by the complex graph sample.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <returns><see langword="true"/> when the cached path should be used; otherwise, <see langword="false"/>.</returns>
        public static bool CanUseCachedBureau(BureauRequest request)
            => request.Amount <= 1000m;

        /// <summary>
        /// Determines whether the bureau request should be reviewed manually.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <returns><see langword="true"/> when the manual bureau path should be used; otherwise, <see langword="false"/>.</returns>
        public static bool RequiresManualBureauReview(BureauRequest request)
            => request.Amount > 1000m && request.Amount <= 3000m;

        /// <summary>
        /// Creates a bureau decision from a manual review lane.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <returns>The manual bureau decision.</returns>
        public static BureauDecision CreateManualBureauDecision(BureauRequest request)
            => new(request.ApplicationId, "Manual review", request.Amount <= 2500m);

        /// <summary>
        /// Records that the manual bureau lane was selected.
        /// </summary>
        /// <param name="decision">The manual bureau decision.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task RecordManualBureauReviewAsync(
            BureauDecision decision,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        /// Determines whether a remote bureau decision needs escalation.
        /// </summary>
        /// <param name="decision">The remote bureau decision.</param>
        /// <returns><see langword="true"/> when the remote bureau result needs escalation; otherwise, <see langword="false"/>.</returns>
        public static bool RemoteBureauNeedsEscalation(BureauDecision decision)
            => !decision.IsApproved;

        /// <summary>
        /// Records an escalation for a remote bureau decision.
        /// </summary>
        /// <param name="decision">The remote bureau decision.</param>
        public static void RecordRemoteBureauEscalation(BureauDecision decision)
        {
        }

        /// <summary>
        /// Records that the remote bureau decision was cleared.
        /// </summary>
        /// <param name="decision">The remote bureau decision.</param>
        public static void RecordRemoteBureauClearance(BureauDecision decision)
        {
        }

        /// <summary>
        /// Builds the underwriting packet from the request and bureau result.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="bureau">The bureau decision.</param>
        /// <returns>The underwriting packet.</returns>
        public static UnderwritingPacket BuildUnderwritingPacket(
            CreditApplicationRequest request,
            BureauDecision bureau)
            => new(
                request.ApplicationId,
                request.CustomerId,
                request.RequestedAmount,
                bureau.Source,
                bureau.IsApproved,
                CalculateExposureTier(request.RequestedAmount));

        /// <summary>
        /// Captures independent underwriting signals that conceptually run in parallel.
        /// </summary>
        /// <param name="packet">The underwriting packet.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task CaptureParallelUnderwritingSignalsAsync(
            UnderwritingPacket packet,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        /// Inspects the supporting documents associated with the underwriting packet.
        /// </summary>
        /// <param name="packet">The underwriting packet.</param>
        public static void InspectSupportingDocuments(UnderwritingPacket packet)
        {
        }

        /// <summary>
        /// Prepares a batch of exposure checks for the underwriting packet.
        /// </summary>
        /// <param name="packet">The underwriting packet.</param>
        public static void PrepareExposureBatch(UnderwritingPacket packet)
        {
        }

        /// <summary>
        /// Determines whether fraud screening can use the fast-track route.
        /// </summary>
        /// <param name="packet">The underwriting packet.</param>
        /// <returns><see langword="true"/> when the fast-track fraud route should be used; otherwise, <see langword="false"/>.</returns>
        public static bool CanFastTrackFraud(UnderwritingPacket packet)
            => packet.BureauApproved && packet.ExposureTier == 1;

        /// <summary>
        /// Determines whether enhanced fraud review is required.
        /// </summary>
        /// <param name="packet">The underwriting packet.</param>
        /// <returns><see langword="true"/> when enhanced fraud review is required; otherwise, <see langword="false"/>.</returns>
        public static bool RequiresEnhancedFraudReview(UnderwritingPacket packet)
            => !packet.BureauApproved || packet.ExposureTier >= 3;

        /// <summary>
        /// Creates a fast-track fraud screening result.
        /// </summary>
        /// <param name="packet">The underwriting packet.</param>
        /// <returns>The fraud screening result.</returns>
        public static FraudScreeningResult FastTrackFraudScreening(UnderwritingPacket packet)
            => new(packet.ApplicationId, true, "Fast-track", "Known low-exposure applicant.");

        /// <summary>
        /// Runs an enhanced fraud screening lane.
        /// </summary>
        /// <param name="packet">The underwriting packet.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The fraud screening result.</returns>
        public static Task<FraudScreeningResult> RunEnhancedFraudScreeningAsync(
            UnderwritingPacket packet,
            CancellationToken cancellationToken)
            => Task.FromResult(new FraudScreeningResult(
                packet.ApplicationId,
                packet.BureauApproved,
                "Enhanced",
                packet.BureauApproved ? "Enhanced review cleared." : "Bureau rejection blocked the request."));

        /// <summary>
        /// Creates a standard fraud screening result.
        /// </summary>
        /// <param name="packet">The underwriting packet.</param>
        /// <returns>The fraud screening result.</returns>
        public static FraudScreeningResult RunStandardFraudScreening(UnderwritingPacket packet)
            => new(packet.ApplicationId, packet.BureauApproved, "Standard", "Standard rules evaluated.");

        /// <summary>
        /// Determines whether the flow can continue after fraud screening.
        /// </summary>
        /// <param name="result">The fraud screening result.</param>
        /// <returns><see langword="true"/> when fraud screening cleared the request; otherwise, <see langword="false"/>.</returns>
        public static bool FraudCleared(FraudScreeningResult result)
            => result.IsCleared;

        /// <summary>
        /// Builds the pricing input from the original request and fraud result.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="fraud">The fraud screening result.</param>
        /// <returns>The pricing input.</returns>
        public static PricingInput BuildPricingInput(
            CreditApplicationRequest request,
            FraudScreeningResult fraud)
            => new(
                request.ApplicationId,
                request.RequestedAmount,
                fraud.IsCleared,
                fraud.ReviewLevel,
                CalculateExposureTier(request.RequestedAmount));

        /// <summary>
        /// Determines whether promotional pricing should be used.
        /// </summary>
        /// <param name="input">The pricing input.</param>
        /// <returns><see langword="true"/> when the promotional pricing route should be used; otherwise, <see langword="false"/>.</returns>
        public static bool UsesPromotionalPricing(PricingInput input)
            => input.FraudCleared && input.RequestedAmount <= 1000m;

        /// <summary>
        /// Determines whether risk-adjusted pricing should be used.
        /// </summary>
        /// <param name="input">The pricing input.</param>
        /// <returns><see langword="true"/> when the risk-adjusted pricing route should be used; otherwise, <see langword="false"/>.</returns>
        public static bool UsesRiskAdjustedPricing(PricingInput input)
            => input.FraudCleared && input.ExposureTier >= 2;

        /// <summary>
        /// Builds a promotional credit offer.
        /// </summary>
        /// <param name="input">The pricing input.</param>
        /// <returns>The credit offer.</returns>
        public static CreditOffer BuildPromotionalOffer(PricingInput input)
            => new(input.ApplicationId, "Approved", input.RequestedAmount, 11.5m, "promo");

        /// <summary>
        /// Builds a risk-adjusted credit offer.
        /// </summary>
        /// <param name="input">The pricing input.</param>
        /// <returns>The credit offer.</returns>
        public static CreditOffer BuildRiskAdjustedOffer(PricingInput input)
            => new(input.ApplicationId, "Approved with conditions", input.RequestedAmount * 0.85m, 24.9m, "risk-adjusted");

        /// <summary>
        /// Builds a standard credit offer.
        /// </summary>
        /// <param name="input">The pricing input.</param>
        /// <returns>The credit offer.</returns>
        public static CreditOffer BuildStandardOffer(PricingInput input)
            => new(input.ApplicationId, "Approved", input.RequestedAmount, 17.9m, "standard");

        /// <summary>
        /// Determines whether a contract should be prepared for the offer.
        /// </summary>
        /// <param name="offer">The generated offer.</param>
        /// <returns><see langword="true"/> when a contract should be prepared; otherwise, <see langword="false"/>.</returns>
        public static bool RequiresContract(CreditOffer offer)
            => offer.ApprovedAmount >= 1000m;

        /// <summary>
        /// Marks an offer as not requiring a contract.
        /// </summary>
        /// <param name="offer">The generated offer.</param>
        /// <returns>The updated offer.</returns>
        public static CreditOffer SkipContract(CreditOffer offer)
            => offer with { Channel = $"{offer.Channel} / no-contract" };

        /// <summary>
        /// Builds the contract envelope for an offer.
        /// </summary>
        /// <param name="offer">The generated offer.</param>
        /// <returns>The contract envelope.</returns>
        public static ContractEnvelope BuildContractEnvelope(CreditOffer offer)
            => new(
                offer.ApplicationId,
                offer.AnnualRate >= 20m ? "conditional-offer" : "standard-offer",
                offer.AnnualRate >= 20m,
                "Base credit terms");

        /// <summary>
        /// Determines whether legal review is required for a contract.
        /// </summary>
        /// <param name="envelope">The contract envelope.</param>
        /// <returns><see langword="true"/> when legal review is required; otherwise, <see langword="false"/>.</returns>
        public static bool RequiresLegalReview(ContractEnvelope envelope)
            => envelope.RequiresLegalReview;

        /// <summary>
        /// Attaches legal review terms to a contract envelope.
        /// </summary>
        /// <param name="envelope">The contract envelope.</param>
        /// <returns>The reviewed contract envelope.</returns>
        public static ReviewedContractEnvelope AttachLegalReviewTerms(ContractEnvelope envelope)
            => new(envelope.ApplicationId, envelope.Template, "legal", $"{envelope.Terms}; legal review required");

        /// <summary>
        /// Attaches standard terms to a contract envelope.
        /// </summary>
        /// <param name="envelope">The contract envelope.</param>
        /// <returns>The reviewed contract envelope.</returns>
        public static ReviewedContractEnvelope AttachStandardTerms(ContractEnvelope envelope)
            => new(envelope.ApplicationId, envelope.Template, "standard", $"{envelope.Terms}; standard terms");

        /// <summary>
        /// Attaches contract metadata to an offer.
        /// </summary>
        /// <param name="offer">The generated offer.</param>
        /// <param name="envelope">The reviewed contract envelope.</param>
        /// <returns>The updated offer.</returns>
        public static CreditOffer AttachContractToOffer(
            CreditOffer offer,
            ReviewedContractEnvelope envelope)
            => offer with { Channel = $"{offer.Channel} / {envelope.Template}:{envelope.ReviewLane}" };

        /// <summary>
        /// Builds the dispatch plan for an offer.
        /// </summary>
        /// <param name="offer">The generated offer.</param>
        /// <returns>The dispatch plan.</returns>
        public static OfferDispatchPlan BuildOfferDispatchPlan(CreditOffer offer)
            => new(
                offer.ApplicationId,
                offer.ApprovedAmount >= 10000m ? "priority-desk" : "digital",
                offer.ApprovedAmount >= 10000m,
                offer.Status.Contains("conditions", StringComparison.OrdinalIgnoreCase) ? "relationship-manager" : "automation");

        /// <summary>
        /// Determines whether priority dispatch is required.
        /// </summary>
        /// <param name="plan">The dispatch plan.</param>
        /// <returns><see langword="true"/> when priority dispatch is required; otherwise, <see langword="false"/>.</returns>
        public static bool RequiresPriorityDispatch(OfferDispatchPlan plan)
            => plan.HighPriority;

        /// <summary>
        /// Determines whether a relationship manager should own dispatch.
        /// </summary>
        /// <param name="plan">The dispatch plan.</param>
        /// <returns><see langword="true"/> when a relationship manager should own dispatch; otherwise, <see langword="false"/>.</returns>
        public static bool RequiresRelationshipManager(OfferDispatchPlan plan)
            => plan.Owner == "relationship-manager";

        /// <summary>
        /// Schedules priority dispatch for the offer.
        /// </summary>
        /// <param name="plan">The dispatch plan.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The resolved dispatch plan.</returns>
        public static Task<ResolvedOfferDispatchPlan> SchedulePriorityDispatchAsync(
            OfferDispatchPlan plan,
            CancellationToken cancellationToken)
            => Task.FromResult(new ResolvedOfferDispatchPlan(plan.ApplicationId, "priority-desk", plan.Owner));

        /// <summary>
        /// Assigns a relationship manager to the offer.
        /// </summary>
        /// <param name="plan">The dispatch plan.</param>
        /// <returns>The resolved dispatch plan.</returns>
        public static ResolvedOfferDispatchPlan AssignRelationshipManager(OfferDispatchPlan plan)
            => new(plan.ApplicationId, "relationship-manager", plan.Owner);

        /// <summary>
        /// Schedules standard digital delivery for the offer.
        /// </summary>
        /// <param name="plan">The dispatch plan.</param>
        /// <returns>The resolved dispatch plan.</returns>
        public static ResolvedOfferDispatchPlan ScheduleDigitalDelivery(OfferDispatchPlan plan)
            => new(plan.ApplicationId, "digital", plan.Owner);

        /// <summary>
        /// Marks the offer as ready to dispatch.
        /// </summary>
        /// <param name="offer">The generated offer.</param>
        /// <param name="plan">The resolved dispatch plan.</param>
        /// <returns>The updated offer.</returns>
        public static CreditOffer MarkOfferReadyForDispatch(
            CreditOffer offer,
            ResolvedOfferDispatchPlan plan)
            => offer with { Channel = $"{offer.Channel} / {plan.Channel}" };

        private static int CalculateExposureTier(decimal requestedAmount)
        {
            if (requestedAmount >= 10000m)
                return 3;

            if (requestedAmount >= 3000m)
                return 2;

            return 1;
        }
    }
}
