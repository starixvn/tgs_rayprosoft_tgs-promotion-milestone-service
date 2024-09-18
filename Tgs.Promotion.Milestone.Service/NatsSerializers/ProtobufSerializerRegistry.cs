using NATS.Client.Core;

namespace Tgs.Promotion.Milestone.Service.NatsSerializers;

public class ProtobufSerializerRegistry : INatsSerializerRegistry
{
	public static readonly ProtobufSerializerRegistry Default = new();

	public INatsSerialize<T> GetSerializer<T>() => ProtobufSerializer<T>.Default;

	public INatsDeserialize<T> GetDeserializer<T>() => ProtobufSerializer<T>.Default;
}
