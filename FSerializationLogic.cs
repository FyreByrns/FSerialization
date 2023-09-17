using System.Reflection;

namespace FSerialization;

public static partial class FSerializationLogic {
	static FSerializationLogic() {
		DefaultSerializers.Init();
	}

	/// <summary>
	/// Attempt to serialize an entire class, according to annotation rules.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="objectToSerialize"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool TrySerialize<T>(T objectToSerialize, out byte[] result) {
		if (objectToSerialize is null) {
			result = Array.Empty<byte>();
			return false;
		}

		List<byte> workingBytes = new();

		// iterate over fields to serialize those
		Type classType = typeof(T);
		List<(FieldInfo field, SerAttribute serData)> fieldsToSerialize = new();

		foreach (var field in classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
			foreach (var attr in field.GetCustomAttributes<SerAttribute>().OrderByDescending(x => x.FieldOrder)) {
				fieldsToSerialize.Add((field, attr));
			}
		}

		// analyze fields based on attributes
		int maxLength = 0;
		bool allowStatic = true;

		foreach ((var field, var serData) in fieldsToSerialize) {
			// if the field is of static location and size ..
			if (serData is SerializeAndDeserializeAttribute staticSerData) {
				if (!allowStatic) {
					result = Array.Empty<byte>();
					return false;
				}

				// .. handle as normal
				if (staticSerData.DataStreamEnd > maxLength) {
					maxLength = staticSerData.DataStreamEnd;
				}
			}

			// if the attribute is dynamically sized .. 
			if (serData is SerializeAndDeserializeDynamicAttribute dynamicSerData) {
				// .. disallow any static fields from now on
				allowStatic = false;

				// get length of field
				int length = dynamicSerData.GetLength(field.GetValue(objectToSerialize));
				dynamicSerData.DataStreamStart = maxLength;
				dynamicSerData.DataStreamEnd = dynamicSerData.DataStreamStart + length + sizeof(int);

				maxLength += dynamicSerData.DataLength;
			}
		}

		// create result buffer according to max length
		result = new byte[maxLength];

		// fill buffer
		foreach ((var field, var serData) in fieldsToSerialize) {
			// get instance field
			object oValue = field.GetValue(objectToSerialize)!;
			if (oValue is null) {
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
		List<(FieldInfo field, SerAttribute serData)> fieldsToDeserialize = new();

		int currentLength = 0;
		foreach (var field in classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
			foreach (var attr in field.GetCustomAttributes<SerAttribute>().OrderByDescending(x => x.FieldOrder)) {
				// if the field is dynamic, figure out length and end
				if (attr is SerializeAndDeserializeDynamicAttribute dyn) {
					attr.DataStreamStart = currentLength;
					TryDeserializeValue(bytes[attr.DataStreamStart..(attr.DataStreamStart + sizeof(int))], out int len);
					attr.DataStreamEnd = attr.DataStreamStart + len * dyn.DataSize + sizeof(int);// + dyn.DataSize;
				}

				currentLength += attr.DataLength;
				fieldsToDeserialize.Add((field, attr));
			}
		}

		// deserialize
		foreach ((var field, var serData) in fieldsToDeserialize) {
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
