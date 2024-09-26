using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using TGS.Core.Infrastructures.Nats;
using Tgs.Promotion.Milestone.Domain.Entities;

namespace Tgs.Promotion.Milestone.IntegrationTest;

extern alias Milestone;

internal class TestApplication(Action<IWebHostBuilder>? webHostConfigure = null)
    : WebApplicationFactory<Milestone::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder
            .ConfigureAppConfiguration(builder => builder.Sources.Clear())
            .ConfigureLogging(logging => logging.ClearProviders().AddConsole());

        _ = builder.ConfigureServices(services =>
            {
                services
                    .AddFakeNatsMessage()
                    .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
                        sp => new ConfigureNamedOptions<OtlpExporterOptions>(
                            Options.DefaultName,
                            exporterOptions =>
                            {
                                exporterOptions.TimeoutMilliseconds = 0;
                                exporterOptions.ExportProcessorType = ExportProcessorType.Batch;
                                exporterOptions.Endpoint = new Uri("http://otel.rpgp.cc:4317");
                            }))
                    .Replace(ServiceDescriptor.Singleton(sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<tgsContext>();
                        dbContextOptionsBuilder.UseNpgsql();
                        return new tgsContext(dbContextOptionsBuilder.Options);
                    }));
            }
        );

        webHostConfigure?.Invoke(builder);
    }
}