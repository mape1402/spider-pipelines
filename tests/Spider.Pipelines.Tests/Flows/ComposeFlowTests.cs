using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Core;
using Spider.Pipelines.Flows;

namespace Spider.Pipelines.Tests.Flows
{
    public class ComposeFlowTests
    {
        [Fact]
        public async Task RunAsync_WhenFlowProducesResponse_ShouldReturnResponse()
        {
            var spider = CreateSpider();
            var saved = false;

            var response = await spider
                .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
                .Then(ValidateAsync)
                .Then(MapAsync)
                .ThenWith<CreateCustomerRequest, Customer>(async (request, customer, token) =>
                {
                    await SaveAsync(request, customer, token);
                    saved = true;
                })
                .Then(ReturnResponse)
                .RunAsync(new CreateCustomerRequest("ada@example.com"), CancellationToken.None);

            Assert.True(saved);
            Assert.Equal("ada@example.com", response.Email);
        }

        [Fact]
        public async Task RunAsync_WhenFlowHasDescriptiveMetadata_ShouldKeepExecutionBehavior()
        {
            var spider = CreateSpider();

            var response = await spider
                .ComposeFlow<RiskRequest, RiskResponse>("Evaluate risk")
                .Describe("Evaluates the risk score and produces a decision.")
                .Tags("risk", "decision")
                .Metadata("owner", "credit")
                .Then(BuildRiskProfile, step => step
                    .Named("Build profile")
                    .Describe("Creates the risk profile used by branch conditions.")
                    .Tags("mapping"))
                .Branch<RiskDecision>(branch => branch
                    .Named("Risk decision")
                    .Describe("Chooses the correct decision path.")
                    .Tags("branch")
                    .When(IsLowRisk, low => low
                        .Named("Low risk route")
                        .Tags("automatic")
                        .Then(AutoApprove, step => step.Named("Approve automatically")))
                    .When(IsHighRisk, high => high
                        .Named("High risk route")
                        .Then(RequireManualReview, step => step.Tags("manual-review")))
                    .Otherwise(normal => normal
                        .Named("Standard risk route")
                        .Then(CalculateStandardDecision)))
                .Then(ReturnRiskResponse, step => step.Tags("response"))
                .RunAsync(new RiskRequest(100), CancellationToken.None);

            Assert.Equal("approved", response.Decision);
        }

        [Fact]
        public async Task RunAsync_WhenFlowDoesNotDeclareResponse_ShouldComplete()
        {
            var spider = CreateSpider();
            var saved = false;

            await spider
                .ComposeFlow<CreateCustomerRequest>("Create customer")
                .Then(ValidateAsync)
                .Then(MapAsync)
                .ThenWith<CreateCustomerRequest, Customer>(async (request, customer, token) =>
                {
                    await SaveAsync(request, customer, token);
                    saved = true;
                })
                .RunAsync(new CreateCustomerRequest("ada@example.com"), CancellationToken.None);

            Assert.True(saved);
        }

        [Fact]
        public async Task RunAsync_WhenContinueIfReturnsEarly_ShouldSkipRemainingSteps()
        {
            var spider = CreateSpider();
            var saved = false;

            var response = await spider
                .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
                .ContinueIf(
                    HasValidEmail,
                    Flow.Return<CreateCustomerRequest, CustomerResponse>(BuildInvalidEmailResponse))
                .Then(MapAsync)
                .ThenWith<CreateCustomerRequest, Customer>(async (request, customer, token) =>
                {
                    await SaveAsync(request, customer, token);
                    saved = true;
                })
                .Then(ReturnResponse)
                .RunAsync(new CreateCustomerRequest(""), CancellationToken.None);

            Assert.False(saved);
            Assert.False(response.Created);
            Assert.Equal("Email is required.", response.Message);
        }

        [Fact]
        public async Task RunAsync_WhenContinueIfStopsVoidFlow_ShouldSkipRemainingSteps()
        {
            var spider = CreateSpider();
            var sent = false;

            await spider
                .ComposeFlow<NotificationCommand>("Send notification")
                .ContinueIf(ShouldSendNotification, Flow.Stop())
                .Then(command => sent = true)
                .RunAsync(new NotificationCommand(false), CancellationToken.None);

            Assert.False(sent);
        }

        [Fact]
        public async Task RunAsync_WhenContinueIfThrows_ShouldThrowConfiguredException()
        {
            var spider = CreateSpider();

            await Assert.ThrowsAsync<InvalidOperationException>(() => spider
                .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
                .ContinueIf(
                    HasValidEmail,
                    Flow.Throw(() => new InvalidOperationException("Invalid email.")))
                .Then(MapAsync)
                .Then(ReturnResponse)
                .RunAsync(new CreateCustomerRequest(""), CancellationToken.None));
        }

        [Theory]
        [InlineData(100, "approved")]
        [InlineData(900, "manual-review")]
        [InlineData(500, "standard")]
        public async Task RunAsync_WhenBranchConverges_ShouldContinueWithBranchResult(int score, string expectedDecision)
        {
            var spider = CreateSpider();

            var response = await spider
                .ComposeFlow<RiskRequest, RiskResponse>("Evaluate risk")
                .Then(BuildRiskProfile)
                .Branch<RiskDecision>(branch => branch
                    .When(IsLowRisk, low => low.Then(AutoApprove))
                    .When(IsHighRisk, high => high.Then(RequireManualReview))
                    .Otherwise(normal => normal.Then(CalculateStandardDecision)))
                .Then(ReturnRiskResponse)
                .RunAsync(new RiskRequest(score), CancellationToken.None);

            Assert.Equal(expectedDecision, response.Decision);
        }

        [Fact]
        public async Task RunAsync_WhenBranchRouteContainsNestedBranch_ShouldExecuteNestedRoute()
        {
            var spider = CreateSpider();
            var selectedNestedRoute = string.Empty;

            var response = await spider
                .ComposeFlow<RiskRequest, RiskResponse>("Evaluate nested risk")
                .Then(BuildRiskProfile)
                .Branch<RiskDecision>(branch => branch
                    .When(IsHighRisk, high => high
                        .Then(RequireManualReview)
                        .Branch<RiskDecision>(nested => nested
                            .When(
                                decision => decision.Value == "manual-review",
                                senior => senior.Then(_ => { selectedNestedRoute = "senior-review"; }))
                            .Otherwise(standard => standard.Then(_ => { selectedNestedRoute = "standard-review"; }))))
                    .Otherwise(normal => normal.Then(CalculateStandardDecision)))
                .Then(ReturnRiskResponse)
                .RunAsync(new RiskRequest(900), CancellationToken.None);

            Assert.Equal("manual-review", response.Decision);
            Assert.Equal("senior-review", selectedNestedRoute);
        }

        [Fact]
        public async Task RunAsync_WhenResponseFlowEndsWithWrongActiveType_ShouldThrow()
        {
            var spider = CreateSpider();

            await Assert.ThrowsAsync<InvalidOperationException>(() => spider
                .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
                .Then(MapAsync)
                .RunAsync(new CreateCustomerRequest("ada@example.com"), CancellationToken.None));
        }

        [Fact]
        public void Branch_WhenOtherwiseIsMissing_ShouldThrow()
        {
            var spider = CreateSpider();

            Assert.Throws<InvalidOperationException>(() => spider
                .ComposeFlow<RiskRequest, RiskResponse>("Evaluate risk")
                .Then(BuildRiskProfile)
                .Branch<RiskDecision>(branch => branch
                    .When(IsLowRisk, low => low.Then(AutoApprove))));
        }

        private static ISpider CreateSpider()
        {
            var services = new ServiceCollection();
            services.AddSpider(builder =>
            {
                builder.AddFlowProfile("Business", profile =>
                {
                    profile.TelemetryEnabled = true;
                    profile.MetricsEnabled = true;
                });
            });

            return services.BuildServiceProvider().GetRequiredService<ISpider>();
        }

        private static Task ValidateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return Task.CompletedTask;
        }

        private static Task<Customer> MapAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new Customer(request.Email));

        private static Task SaveAsync(CreateCustomerRequest request, Customer customer, CancellationToken cancellationToken)
            => Task.CompletedTask;

        private static CustomerResponse ReturnResponse(Customer customer)
            => new(true, customer.Email, "Customer created.");

        private static bool HasValidEmail(CreateCustomerRequest request)
            => !string.IsNullOrWhiteSpace(request.Email);

        private static CustomerResponse BuildInvalidEmailResponse(CreateCustomerRequest request)
            => new(false, request.Email, "Email is required.");

        private static bool ShouldSendNotification(NotificationCommand command)
            => command.ShouldSend;

        private static RiskProfile BuildRiskProfile(RiskRequest request)
            => new(request.Score);

        private static bool IsLowRisk(RiskProfile profile)
            => profile.Score < 300;

        private static bool IsHighRisk(RiskProfile profile)
            => profile.Score > 700;

        private static RiskDecision AutoApprove(RiskProfile profile)
            => new("approved");

        private static RiskDecision RequireManualReview(RiskProfile profile)
            => new("manual-review");

        private static RiskDecision CalculateStandardDecision(RiskProfile profile)
            => new("standard");

        private static RiskResponse ReturnRiskResponse(RiskDecision decision)
            => new(decision.Value);

        private sealed record CreateCustomerRequest(string Email);

        private sealed record Customer(string Email);

        private sealed record CustomerResponse(bool Created, string Email, string Message);

        private sealed record NotificationCommand(bool ShouldSend);

        private sealed record RiskRequest(int Score);

        private sealed record RiskProfile(int Score);

        private sealed record RiskDecision(string Value);

        private sealed record RiskResponse(string Decision);
    }
}
