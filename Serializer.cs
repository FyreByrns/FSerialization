namespace FSerialization;

public abstract class Serializer<T>
	: ISerializer {
	public abstract byte[] Serialize(T value);

	public byte[] Serialize(object value) {
		return Serialize((T)value);
	}
}
