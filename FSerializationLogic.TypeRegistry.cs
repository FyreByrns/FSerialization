namespace FSerialization;

public static partial class FSerializationLogic {
	public static class TypeRegistry {
		public static Dictionary<string, SerializerDeserializer> RegisteredSerializerDeserializers
			= new();

		public static bool SerializerDeserializerRegistered<T>() {
			return RegisteredSerializerDeserializers.ContainsKey(typeof(T).ToString());
		}

		public static void RegisterSerializerDeserializer<T>(SerializerDeserializer<T> serializerDeserializer) {
			if (!SerializerDeserializerRegistered<T>()) {
				RegisteredSerializerDeserializers[typeof(T).ToString()] = serializerDeserializer;
			}
		}

		public static Serializer<T>? SerializerFor<T>() {
			if (SerializerDeserializerRegistered<T>()) {
				if (RegisteredSerializerDeserializers[typeof(T).ToString()] is SerializerDeserializer<T> serder
				 && serder.Serializer is Serializer<T> ser) {
					return ser;
				}
			}

			return null;
		}
		public static Deserializer<T>? DeserializerFor<T>() {
			if (SerializerDeserializerRegistered<T>()) {
				if (RegisteredSerializerDeserializers[typeof(T).ToString()] is SerializerDeserializer<T> serder
				 && serder.Deserializer is Deserializer<T> der) {
					return der;
				}
			}

			return null;
		}
	}
}
