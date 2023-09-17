namespace FSerialization;

public static partial class DefaultSerializers {
	class Int64 : SerializerDeserializer<long> {
		class Ser : Serializer<long> {
			public override byte[] Serialize(long value) {
				return BitConverter.GetBytes(value);
			}
		}
		class Der : Deserializer<long> {
			public override long Deserialize(byte[] bytes) {
				return BitConverter.ToInt64(bytes);
			}
		}
		public Int64() : base(new Ser(), new Der()) { }
	}
}
