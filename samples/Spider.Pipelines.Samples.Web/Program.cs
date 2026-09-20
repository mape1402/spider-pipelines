namespace Spider.Pipelines.Samples.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Spider.Pipelines.Generated;
    using Spider.Pipelines.Web;

    /// <summary>
    /// Hosts the Spider architecture documentation sample.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Starts the sample web application.
        /// </summary>
        /// <param name="args">The application arguments.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<ISpiderArchitectureWebRenderer, SpiderArchitectureWebRenderer>();

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                if (IsDocumentationPath(context.Request.Path))
                {
                    var manifest = SpiderGeneratedArchitecture.BuildManifest();
                    var renderer = context.RequestServices.GetRequiredService<ISpiderArchitectureWebRenderer>();
                    var html = renderer.Render(manifest, new SpiderArchitectureWebOptions
                    {
                        Title = "Spider Sample Architecture"
                    });

                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(html, context.RequestAborted);
                    return;
                }

                await next();
            });

            await app.RunAsync();
        }

        /// <summary>
        /// Determines whether the requested path should render the documentation UI.
        /// </summary>
        /// <param name="path">The requested path.</param>
        /// <returns><see langword="true"/> when the path renders Spider documentation; otherwise, <see langword="false"/>.</returns>
        private static bool IsDocumentationPath(PathString path)
            => path == "/" || path == "/_spider";
    }
}
