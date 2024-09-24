using NATS.Client.Core;
using Tgs.Promotion.Clients.Protos;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Promotion.Milestone.Core.NatsMessages
{
    public class RollBackMessageHandler : IMessageHandler<CheckPromotionRequest>
    {
        public Task Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, CheckPromotionRequest? message,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
