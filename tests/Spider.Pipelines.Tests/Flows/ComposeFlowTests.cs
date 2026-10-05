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
        public async Task RunAsync_WhenNoResponseFlowUsesDslOverloads_ShouldComplete()
        {
            var spider = CreateSpider();
            var log = new List<string>();

            await spider
                .ComposeFlow<NumberRequest>("Calculate")
                .UsingProfile("Business")
                .Describe("Exercises the no-response builder overloads.")
                .Tags(" math ", "", "no-response")
                .Metadata("owner", "tests")
                .Then(request => new NumberStage(request.Value + 1), step => step.Named("Map request"))
                .Then((NumberStage stage, CancellationToken token) => Task.FromResult(new NumberDecision(stage.Value + 1)), step => step.Tags("async-map"))
                .Then(decision => log.Add($"active:{decision.Value}"))
                .Then((NumberDecision decision, CancellationToken token) => log.Add($"token:{token.CanBeCanceled}"))
                .Then((NumberDecision decision, CancellationToken token) =>
                {
                    log.Add($"task:{decision.Value}");
                    return Task.CompletedTask;
                }, step => step.Describe("Records the active decision."))
                .ThenWith<NumberRequest, NumberHistoryStage>(request => new NumberHistoryStage(request.Value + 10), step => step.Named("Resolve original request"))
                .ThenWith<NumberRequest>((request, token) =>
                {
                    log.Add($"history:{request.Value}");
                    return Task.CompletedTask;
                }, step => step.Tags("history"))
                .ThenWith<NumberRequest, NumberStage, NumberDecision>((request, stage) => new NumberDecision(request.Value + stage.Value), step => step.Metadata("uses", "two-values"))
                .ThenWith<NumberRequest, NumberStage>((request, stage, token) =>
                {
                    log.Add($"pair:{request.Value + stage.Value}");
                    return Task.CompletedTask;
                }, step => step.Named("Pair effect"))
                .ContinueIf(decision => decision.Value > 0, Flow.Stop(), step => step.Named("Continue positive"))
                .ContinueIf(decision => decision.Value > 0, Flow.Throw(() => new InvalidOperationException("negative")), step => step.Named("Throw guard"))
                .Branch<NumberResult>(branch => branch
                    .Named("Decision route")
                    .Describe("Chooses a positive route.")
                    .Tags("branch")
                    .Metadata("kind", "number")
                    .When(
                        decision => decision.Value > 0,
                        route => route
                            .Named("Positive")
                            .Describe("Positive path")
                            .Tags("positive")
                            .Metadata("route", "positive")
                            .Then(decision => new NumberResult(decision.Value), step => step.Named("Create result"))
                            .Then(result => log.Add($"route:{result.Value}")))
                    .Otherwise(route => route
                        .Then(decision => new NumberResult(-1))))
                .Then(result => log.Add($"result:{result.Value}"))
                .RunAsync(new NumberRequest(2), CancellationToken.None);

            Assert.Contains("active:4", log);
            Assert.Contains("history:2", log);
            Assert.Contains("pair:5", log);
            Assert.Contains("route:5", log);
            Assert.Contains("result:5", log);
        }

        [Fact]
        public async Task RunAsync_WhenResponseFlowUsesDslOverloads_ShouldReturnResponse()
        {
            var spider = CreateSpider();
            var log = new List<string>();

            var response = await spider
                .ComposeFlow<NumberRequest, NumberResult>("Calculate result")
                .UsingProfile("Business")
                .Describe("Exercises the response builder overloads.")
                .Tags("math", "response")
                .Metadata("owner", "tests")
                .ContinueIf(
                    request => request.Value > 0,
                    Flow.Return<NumberRequest, NumberResult>((request, token) => Task.FromResult(new NumberResult(-1))),
                    step => step.Named("Positive input"))
                .ContinueIf(
                    request => request.Value > 0,
                    Flow.Throw(() => new InvalidOperationException("invalid")),
                    step => step.Named("Throwing input guard"))
                .Then(request => new NumberStage(request.Value + 1), step => step.Named("Map"))
                .Then((NumberStage stage, CancellationToken token) => Task.FromResult(new NumberDecision(stage.Value + 1)), step => step.Named("Async map"))
                .Then(decision => log.Add($"decision:{decision.Value}"))
                .Then((NumberDecision decision, CancellationToken token) => log.Add($"token:{token.CanBeCanceled}"), step => step.Named("Token effect"))
                .Then((NumberDecision decision, CancellationToken token) =>
                {
                    log.Add($"task:{decision.Value}");
                    return Task.CompletedTask;
                })
                .ThenWith<NumberRequest, NumberHistoryStage>(request => new NumberHistoryStage(request.Value + 10), step => step.Named("History map"))
                .ThenWith<NumberRequest>((request, token) =>
                {
                    log.Add($"history:{request.Value}");
                    return Task.CompletedTask;
                }, step => step.Named("History effect"))
                .ThenWith<NumberRequest, NumberStage, NumberDecision>((request, stage) => new NumberDecision(request.Value + stage.Value), step => step.Named("Two-value map"))
                .ThenWith<NumberRequest, NumberStage>((request, stage, token) =>
                {
                    log.Add($"pair:{request.Value + stage.Value}");
                    return Task.CompletedTask;
                }, step => step.Named("Two-value effect"))
                .Branch<NumberResult>(branch => branch
                    .When(
                        decision => decision.Value > 0,
                        route => route
                            .Then(decision => new NumberResult(decision.Value), step => step.Named("Result"))
                            .ThenWith<NumberRequest, NumberResult>(request => new NumberResult(request.Value + 100), step => step.Named("Route history map"))
                            .ThenWith<NumberRequest, NumberStage>((request, stage, token) =>
                            {
                                log.Add($"route-history:{request.Value + stage.Value}");
                                return Task.CompletedTask;
                            }, step => step.Named("Route pair effect")))
                    .Otherwise(route => route.Then(decision => new NumberResult(-1))))
                .RunAsync(new NumberRequest(3), CancellationToken.None);

            Assert.Equal(103, response.Value);
            Assert.Contains("decision:5", log);
            Assert.Contains("history:3", log);
            Assert.Contains("pair:7", log);
            Assert.Contains("route-history:7", log);
        }

        [Fact]
        public async Task RunAsync_WhenResponseFlowReturnsNullReferenceResponse_ShouldReturnDefault()
        {
            var spider = CreateSpider();

            var response = await spider
                .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
                .ContinueIf(
                    request => false,
                    Flow.Return<CreateCustomerRequest, CustomerResponse>((request, token) => Task.FromResult<CustomerResponse>(null)))
                .Then(request => throw new InvalidOperationException("Should not run."))
                .RunAsync(new CreateCustomerRequest("ada@example.com"), CancellationToken.None);

            Assert.Null(response);
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

        [Fact]
        public void BuilderMethods_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var spider = CreateSpider();

            Assert.Throws<ArgumentNullException>(() => spider.ComposeFlow<RiskRequest>("Risk").Tags(null));
            Assert.Throws<ArgumentNullException>(() => spider.ComposeFlow<RiskRequest, RiskResponse>("Risk").Tags(null));
            Assert.Throws<ArgumentNullException>(() => spider.ComposeFlow<RiskRequest>("Risk").Then(request => request, null));
            Assert.Throws<ArgumentNullException>(() => spider.ComposeFlow<RiskRequest, RiskResponse>("Risk").Then(request => request, null));
            Assert.Throws<ArgumentNullException>(() => spider.ComposeFlow<RiskRequest>("Risk").ContinueIf(request => true, (FlowStop)null));
            Assert.Throws<ArgumentNullException>(() => spider.ComposeFlow<RiskRequest>("Risk").Branch<RiskDecision>(null));
            Assert.Throws<ArgumentNullException>(() => spider.ComposeFlow<RiskRequest, RiskResponse>("Risk").Branch<RiskDecision>(null));
            Assert.Throws<ArgumentNullException>(() => spider
                .ComposeFlow<RiskRequest, RiskResponse>("Risk")
                .Then(BuildRiskProfile)
                .Branch<RiskDecision>(branch => branch.When(null, route => route.Then(AutoApprove))));
            Assert.Throws<ArgumentNullException>(() => spider
                .ComposeFlow<RiskRequest, RiskResponse>("Risk")
                .Then(BuildRiskProfile)
                .Branch<RiskDecision>(branch => branch.When(IsLowRisk, null)));
            Assert.Throws<ArgumentNullException>(() => spider
                .ComposeFlow<RiskRequest, RiskResponse>("Risk")
                .Then(BuildRiskProfile)
                .Branch<RiskDecision>(branch => branch.Otherwise(null)));
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

        private sealed record NumberRequest(int Value);

        private sealed record NumberStage(int Value);

        private sealed record NumberHistoryStage(int Value);

        private sealed record NumberDecision(int Value);

        private sealed record NumberResult(int Value);
    }
}
