namespace FSerialization;

public class SerializeAndDeserializeDynamicAttribute : SerAttribute {
	public ILengthHandler LengthHandler;
	public int DataSize;

	public int GetLength(object instance) {
		return LengthHandler.GetLength(instance);
	}

	public SerializeAndDeserializeDynamicAttribute(int fieldOrder, Type lengthHandlerType, int dataSize)
		: base(fieldOrder) {
		LengthHandler = (ILengthHandler?)Activator.CreateInstance(lengthHandlerType)!;
		DataSize = dataSize;
	}
}