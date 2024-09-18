using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using System.Text.Json;
using Tgs.Promotion.Clients.Protos;
using TGS.Core.Infrastructures.Nats.Interface;
using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.Core.NatsMessages
{
    public class RuleReplyProcessor(ILogger<RuleReplyProcessor> logger) : IMessageReplyHandler<CampaignDetailRequest, CampaignDetailResponse>
    {
        public Task<CampaignDetailResponse> Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, CampaignDetailRequest? message,
            CancellationToken cancellationToken)
        {
            var settings = JsonSerializer.Deserialize<MilestoneSetting>(
                               message!.Settings,
                               PromotionUtility.SerializerOptions) ??
                           throw new NullReferenceException("Deserialize MilestoneSetting is null");

            logger.LogDebug("Milestone setting : {settings}", settings);

            settings = settings with
            {
                BucketTypes = settings.BucketTypes?.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray()
            };

            var response = new CampaignDetailResponse
            {
                CampaignDetail = JsonSerializer.Serialize(
                    settings,
                    PromotionUtility.SerializerOptions)
            };

            return Task.FromResult(response);
        }
    }
}
