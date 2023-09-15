namespace FSerialization;

public interface ISerializer {
	public byte[] Serialize(object value);
}
