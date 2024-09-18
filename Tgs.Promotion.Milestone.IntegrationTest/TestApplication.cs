using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Promotion.Milestone.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using TGS.Core.Infrastructures.Nats;
using TGS.Core.Infrastructures.Nats.Interface;
using TGS.Core.Infrastructures.Nats.Worker;

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