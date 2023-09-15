namespace FSerialization;

public abstract class Deserializer<T>
	: IDeserializer {
	public abstract T Deserialize(byte[] bytes);

	object? IDeserializer.Deserialize(byte[] bytes) {
		return Deserialize(bytes);
	}
}
