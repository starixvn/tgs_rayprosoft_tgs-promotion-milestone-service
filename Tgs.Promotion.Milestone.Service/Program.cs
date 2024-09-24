using Microsoft.EntityFrameworkCore;
using NATS.Client.Core;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Promotion.Clients.Protos;
using TGS.Core.Infrastructures.Nats;
using Tgs.Promotion.Milestone.Core.DependencyInjection;
using Tgs.Promotion.Milestone.Repository.DependencyInjection;
using Tgs.Promotion.Milestone.Core.NatsMessages;
using OpenTelemetry.Exporter;
using Tgs.Promotion;
using Tgs.Promotion.Milestone.Service.NatsSerializers;
using Tgs.Promotion.Milestone.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddNatsMessage(builder.Configuration.GetConnectionString("Nats")!, ProtobufSerializerRegistry.Default)
    .RequestReplyRegister<CampaignDetailRequest, CampaignDetailResponse>(typeof(RuleReplyProcessor),
        "tgs.promotion.milestone.setting", "tgs.promotion.milestone.setting")
    .RequestReplyRegister<CheckPromotionRequest, ReadOnlyMemory<byte>>(
        typeof(CheckReplyProcessor),
        string.Format(PromotionCheckConsts.CheckSubjectTemplate, Const.Milestone.ToLowerInvariant()),
        string.Format(PromotionCheckConsts.CheckSubjectTemplate, Const.Milestone.ToLowerInvariant()))
    .RequestReplyRegister<CheckPromotionRequest, ReadOnlyMemory<byte>>(
        typeof(CheckReplyProcessor),
        string.Format(PromotionCheckConsts.TagSubjectTemplate, Const.Milestone.ToLowerInvariant()),
        string.Format(PromotionCheckConsts.TagSubjectTemplate, Const.Milestone.ToLowerInvariant()))
    .RequestReplyRegister<CheckPromotionRequest, ReadOnlyMemory<byte>>(
        typeof(CheckReplyProcessor),
        string.Format(PromotionCheckConsts.RollbackSubjectTemplate, Const.Milestone.ToLowerInvariant()),
        string.Format(PromotionCheckConsts.RollbackSubjectTemplate, Const.Milestone.ToLowerInvariant()))
    .RequestReplyRegister<CheckPromotionRequest, ReadOnlyMemory<byte>>(
        typeof(CheckReplyProcessor),
        string.Format(PromotionCheckConsts.CommitSubjectTemplate, Const.Milestone.ToLowerInvariant()),
        string.Format(PromotionCheckConsts.CommitSubjectTemplate, Const.Milestone.ToLowerInvariant()))
    .SubscribeRegister<string>(typeof(MilestoneMessageHandler), "tgs.promotion.milestone.distribute",
        "tgs.promotion.milestone.distribute");

builder.Services
    .AddPromotionMilestoneCore()
    .AddSerialNumberGenerator()
    .AddMilestoneRepositories(
        dbBuilder => dbBuilder
            .UseNpgsql("name=Tgs"))
    .AddPromotionClientServices((opts, _) => opts.Url =
    builder.Configuration.GetValue<Uri>("Milestone:PromotionServiceBaseUrl")!);

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp!;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddOptions<OtlpExporterOptions>()
    .Configure<IConfiguration>((options, conifg) =>
    {
        var nodeIp = conifg.GetValue<string>("NODE_IP");

        if (!string.IsNullOrWhiteSpace(nodeIp)) options.Endpoint = new Uri($"http://{nodeIp}:4317");

        conifg.GetSection("Opentelemetry:Tracing").Bind(options);
    });

builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .Services
    .Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue ||
                       (!httpContext.Request.Path.StartsWithSegments("/swagger") &&
                        !httpContext.Request.Path.StartsWithSegments("/healthz") &&
                        !httpContext.Request.Path.StartsWithSegments("/profiler/") &&
                        !new[] { "html", "ico", "png", "jpg" }.Any(
                            ext => httpContext.Request.Path.Value.EndsWith($".{ext}"))));
builder.Services.AddProfileViewer();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerUI();
app.MapControllers();
app.MapSwagger();
app.MapHealthChecks("/healthz");

app.Run();
