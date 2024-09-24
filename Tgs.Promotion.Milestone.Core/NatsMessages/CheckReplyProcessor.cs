using NATS.Client.Core;
using Tgs.Promotion.Clients.Protos;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Promotion.Milestone.Core.NatsMessages
{
    public class CheckReplyProcessor : IMessageReplyHandler<CheckPromotionRequest, bool>
    {
        public Task<bool> Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, CheckPromotionRequest? message,
            CancellationToken cancellationToken)
        {
            if (message?.PromotionId == null || message.PromotionId.ToGuid() == Guid.Empty)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
    }
}
