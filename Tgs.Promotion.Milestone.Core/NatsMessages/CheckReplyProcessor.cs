using Google.Protobuf;
using NATS.Client.Core;
using Tgs.Promotion.Clients.Protos;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Promotion.Milestone.Core.NatsMessages
{
    public class CheckReplyProcessor : IMessageReplyHandler<CheckPromotionRequest, ReadOnlyMemory<byte>>
    {
        public Task<ReadOnlyMemory<byte>> Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, CheckPromotionRequest? message,
            CancellationToken cancellationToken)
        {
            if (message?.PromotionId == null || message.PromotionId.ToGuid() == Guid.Empty)
                return Task.FromResult<ReadOnlyMemory<byte>>(new CheckPromotionResponse { Result = false }.ToByteArray());

            return Task.FromResult<ReadOnlyMemory<byte>>(new CheckPromotionResponse { Result = true }.ToByteArray());
        }
    }
}
