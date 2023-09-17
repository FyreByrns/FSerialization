namespace FSerialization;

public abstract class SerAttribute : Attribute {
	public int FieldOrder;

	public int DataStreamStart, DataStreamEnd;
	public int DataLength => DataStreamEnd - DataStreamStart;

	protected SerAttribute(int fieldOrder) {
		FieldOrder = fieldOrder;
	}
}
