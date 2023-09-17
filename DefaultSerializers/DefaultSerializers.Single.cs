namespace FSerialization;

public static partial class DefaultSerializers {
	class Single : SerializerDeserializer<float> {
		class Ser : Serializer<float> {
			public override byte[] Serialize(float value) {
				return BitConverter.GetBytes(value);
			}
		}
		class Der : Deserializer<float> {
			public override float Deserialize(byte[] bytes) {
				return BitConverter.ToSingle(bytes);
			}
		}
		public Single() : base(new Ser(), new Der()) { }
	}
}
