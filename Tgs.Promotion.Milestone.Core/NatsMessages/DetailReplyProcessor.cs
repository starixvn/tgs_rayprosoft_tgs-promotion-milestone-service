using System.Text.Json;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using TGS.Core.Infrastructures.Nats.Interface;
using Tgs.Promotion.Milestone.Domain.Interfaces;

namespace Tgs.Promotion.Milestone.Core.NatsMessages;

public class DetailReplyProcessor(ILogger<DetailReplyProcessor> logger, IMilestoneRepository repository)
    : IMessageReplyHandler<DistributedDetailRequest, DistributedDetailResponse>
{
    public async Task<DistributedDetailResponse> Handle(string subject, string? queueGroup,
        NatsHeaders? natsHeaders, DistributedDetailRequest? message,
        CancellationToken cancellationToken)
    {
        var milestone = await repository.GetRequirementsFulfilledByIdAsync(message.PromotionId,
            message.ReferenceId,
            cancellationToken);

        var response = new DistributedDetailResponse
        {
            Detail = milestone != null
                ? JsonSerializer.Serialize(
                    new
                    {
                        milestone.MilestoneLevel
                    },
                    PromotionUtility.SerializerOptions)
                : null
        };

        return response;
    }
}