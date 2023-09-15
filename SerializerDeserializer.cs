namespace FSerialization;

public class SerializerDeserializer {
	public ISerializer Serializer { get; set; }
	public IDeserializer Deserializer { get; set; }
	public SerializerDeserializer(ISerializer serializer, IDeserializer deserializer) {
		Serializer = serializer;
		Deserializer = deserializer;
	}
}
public class SerializerDeserializer<T> : SerializerDeserializer {
	public SerializerDeserializer(Serializer<T> serializer, Deserializer<T> deserializer) : base(serializer, deserializer) { }
}
