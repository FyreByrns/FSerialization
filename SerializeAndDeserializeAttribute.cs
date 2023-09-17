namespace FSerialization;

public class SerializeAndDeserializeAttribute : SerAttribute {
	public string FieldName;

	public SerializeAndDeserializeAttribute(int fieldOrder, int dataStreamStart, int dataStreamEnd, string fieldName)
		: base(fieldOrder) {
		DataStreamStart = dataStreamStart;
		DataStreamEnd = dataStreamEnd;
		FieldName = fieldName;
	}
	public SerializeAndDeserializeAttribute(int fieldOrder, int dataStreamStart, int dataStreamEnd)
		: this(fieldOrder, dataStreamStart, dataStreamEnd, "unnamed") { }
}
