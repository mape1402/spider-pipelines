using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;

[MemoryDiagnoser]
public class PipelineScenarioBenchmarks
{
    private IServiceBridge<BenchmarkService, string, int> _emptyPipeline;
    private IServiceBridge<BenchmarkService, string, int> _prePostPipeline;
    private IServiceBridge<BenchmarkService, string, int> _middlewareParallelPipeline;
    private IServiceBridge<BenchmarkService, string, int> _overridePipeline;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<BenchmarkService>();
        services.AddSpider();

        var provider = services.BuildServiceProvider();
        var spider = provider.GetRequiredService<ISpider>();

        _emptyPipeline = spider
            .InitBridge<BenchmarkService>()
            .Attach<string, int>(builder => { });

        _prePostPipeline = spider
            .InitBridge<BenchmarkService>()
            .Attach<string, int>(builder =>
            {
                builder
                    .PreProcess<string, int>((ctx, args) => Task.CompletedTask)
                    .OnSuccess<string, int>((ctx, args) => Task.CompletedTask);
            });

        _middlewareParallelPipeline = spider
            .InitBridge<BenchmarkService>()
            .Attach<string, int>(builder =>
            {
                builder
                    .UseMiddleware<string, int>(async (ctx, next) => await next())
                    .Parallel<string, int>((ctx, args) => Task.CompletedTask);
            });

        _overridePipeline = spider
            .InitBridge<BenchmarkService>()
            .Attach<string, int>(builder =>
            {
                builder.UseOverride<string, int>((request, token) => Task.FromResult(request.Length + 1));
            });
    }

    [Benchmark(Baseline = true)]
    public Task<int> EmptyPipeline()
        => _emptyPipeline.ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

    [Benchmark]
    public Task<int> PreAndPostPipeline()
        => _prePostPipeline.ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

    [Benchmark]
    public Task<int> MiddlewareAndParallelPipeline()
        => _middlewareParallelPipeline.ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

    [Benchmark]
    public Task<int> OverridePipeline()
        => _overridePipeline.ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");
}

public sealed class BenchmarkService
{
    public Task<int> HandleAsync(string request, CancellationToken cancellationToken)
        => Task.FromResult(request.Length);
}
