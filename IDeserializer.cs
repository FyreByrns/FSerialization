namespace FSerialization;

public interface IDeserializer {
	public object? Deserialize(byte[] bytes);
}
