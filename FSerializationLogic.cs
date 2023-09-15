using System.Reflection;

namespace FSerialization;

public static class FSerializationLogic {
	static FSerializationLogic() {
		DefaultSerializers.Init();
	}

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

	/// <summary>
	/// Attempt to serialize an entire class, according to annotation rules.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="objectToSerialize"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool TrySerialize<T>(T objectToSerialize, out byte[]? result) {
		if (objectToSerialize is null) {
			result = Array.Empty<byte>();
			return false;
		}

		List<byte> workingBytes = new();


		// iterate over fields to serialize those
		Type classType = typeof(T);
		List<(FieldInfo field, SerializeAttribute serData)> fieldsToSerialize = new();

		foreach (var field in classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
			foreach (var attr in field.GetCustomAttributes<SerializeAttribute>()) {
				if (attr is SerializeAttribute a && a is not DeserializeOnlyAttribute) {
					fieldsToSerialize.Add((field, a));
				}
			}
		}

		// get max data stream index
		int maxLength = 0;
		foreach ((var field, var serData) in fieldsToSerialize) {
			if (serData.DataStreamEnd > maxLength) {
				maxLength = serData.DataStreamEnd;
			}
		}

		// create result buffer according to max length
		result = new byte[maxLength];

		// fill buffer
		foreach ((var field, var serData) in fieldsToSerialize) {
			// get instance field
			object oValue = field.GetValue(objectToSerialize)!;
			if(oValue is null) {
				continue;
			}

			// create specific generic TrySerialize for the field
			MethodInfo generic_TrySerializeValue = typeof(FSerializationLogic).GetMethod(nameof(TrySerializeValue))!;
			MethodInfo specific_TrySerializeValue = generic_TrySerializeValue.MakeGenericMethod(oValue.GetType());

			// invoke specific method
			byte[] valueSerialized = Array.Empty<byte>();
			object[] parameters = { oValue, valueSerialized };
			if (specific_TrySerializeValue.Invoke(null, parameters) is bool success && success) {
				// copy successful serialization into result array
				valueSerialized = (byte[])parameters[1];
				Array.Copy(valueSerialized, 0, result, serData.DataStreamStart, Math.Min(valueSerialized.Length, serData.DataLength));
			}
		}

		return true;
	}
	/// <summary>
	/// Attempt to deserialize an entire class, according to annotation rules.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="bytes"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool TryDeserialize<T>(byte[] bytes, ref T? result) {
		// iterate over fields to deserialize
		Type classType = typeof(T);
		List<(FieldInfo field, SerializeAttribute serData)> fieldsToDeserialize = new();

		foreach (var field in classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
			foreach (var attr in field.GetCustomAttributes<SerializeAttribute>()) {
				if (attr is DeserializeOnlyAttribute or SerializeAttribute) {
					SerializeAttribute a = attr; ;
					fieldsToDeserialize.Add((field, a));
				}
			}
		}

		// deserialize
		foreach ((var field, var serData) in fieldsToDeserialize) {
			// create specific generic TrySerialize for the field
			MethodInfo generic_TryDeserializeValue = typeof(FSerializationLogic).GetMethod(nameof(TryDeserializeValue))!;
			MethodInfo specific_TryDeserializeValue = generic_TryDeserializeValue.MakeGenericMethod(field.FieldType);

			// invoke specific method
			byte[] valueSerialized = bytes[serData.DataStreamStart..serData.DataStreamEnd];
			object valueDeserialized = null;

			object[] parameters = { valueSerialized, valueDeserialized };
			if (specific_TryDeserializeValue.Invoke(null, parameters) is bool success && success) {
				// copy successful serialization into result array
				valueDeserialized = parameters[1];
				field.SetValue(result, valueDeserialized);
			}
		}

		return true;
	}

	/// <summary>
	/// Attempt to serialize a single value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool TrySerializeValue<T>(T value, out byte[]? result) {
		Serializer<T>? ser = TypeRegistry.SerializerFor<T>();

		if (ser is not null) {
			result = ser.Serialize(value);
			return true;
		}

		result = null;
		return false;
	}
	/// <summary>
	/// Attempt to deserialize a single value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="bytes"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool TryDeserializeValue<T>(byte[] bytes, out T? result) {
		Deserializer<T>? des = TypeRegistry.DeserializerFor<T>();

		if (des is not null) {
			result = (T)des.Deserialize(bytes);
			return true;
		}

		result = default;
		return false;
	}
}
