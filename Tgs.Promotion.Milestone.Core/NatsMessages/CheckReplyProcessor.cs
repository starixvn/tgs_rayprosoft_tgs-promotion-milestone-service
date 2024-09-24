using Google.Protobuf;
using NATS.Client.Core;
using Tgs.Promotion.Clients.Protos;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Promotion.Milestone.Core.NatsMessages
{
    public class CheckReplyProcessor : IMessageReplyHandler<CheckPromotionRequest, ReadOnlyMemory<byte>>
    {
        private static readonly ReadOnlyMemory<byte> EmptyResponse = Array.Empty<byte>();
        public Task<ReadOnlyMemory<byte>> Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, CheckPromotionRequest? message,
        CancellationToken cancellationToken)
        {
            var action = subject.Split('.').Last();

            if (message?.PromotionId == null || message.PromotionId.ToGuid() == Guid.Empty)
                return Task.FromResult(EmptyResponse);

            return action switch
            {
                "check" => Task.FromResult<ReadOnlyMemory<byte>>(new CheckPromotionResponse { Result = true }
                    .ToByteArray()),
                "commit" or "rollback" or "tag" => Task.FromResult(EmptyResponse),
                _ => Task.FromResult(EmptyResponse)
            };
        }
    }
}
