namespace FSerialization;

public static partial class DefaultSerializers {
	class Int32 : SerializerDeserializer<int> {
		class Ser : Serializer<int> {
			public override byte[] Serialize(int value) {
				return BitConverter.GetBytes(value);
			}
		}
		class Der : Deserializer<int> {
			public override int Deserialize(byte[] bytes) {
				return BitConverter.ToInt32(bytes);
			}
		}
		public Int32() : base(new Ser(), new Der()) { }
	}
}
