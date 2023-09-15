namespace FSerialization;

public class SerializeAttribute : Attribute {
	public int DataStreamStart, DataStreamEnd;
	public string FieldName;

	public int DataLength => DataStreamEnd - DataStreamStart;

	protected SerializeAttribute() { }
	public SerializeAttribute(int dataStreamStart, int dataStreamEnd, string fieldName) {
		DataStreamStart = dataStreamStart;
		DataStreamEnd = dataStreamEnd;
		FieldName = fieldName;
	}
	public SerializeAttribute(int dataStreamStart, int dataStreamEnd)
		: this(dataStreamStart, dataStreamEnd, "unnamed") { }
}

public class DeserializeOnlyAttribute : SerializeAttribute {
	public DeserializeOnlyAttribute(int dataStreamStart, int dataStreamEnd, string fieldName)
		: base(dataStreamStart, dataStreamEnd, fieldName) { }
	public DeserializeOnlyAttribute(int dataStreamStart, int dataStreamEnd)
		: this(dataStreamStart, dataStreamEnd, "unnamed") { }
}