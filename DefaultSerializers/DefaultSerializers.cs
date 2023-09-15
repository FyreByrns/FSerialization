using System.Reflection;

using static FSerialization.FSerializationLogic;

namespace FSerialization;

public static partial class DefaultSerializers {
	public static void Init() { }
	static DefaultSerializers() {
		// iterate over all default ser/ders in this class and register them

		var defaultSerializers = typeof(DefaultSerializers).GetNestedTypes((BindingFlags)~0);
		MethodInfo generic_Register = typeof(TypeRegistry).GetMethod(nameof(TypeRegistry.RegisterSerializerDeserializer))!;

		foreach (var type in defaultSerializers) {
			MethodInfo specific_Register = generic_Register.MakeGenericMethod(type.BaseType!.GetGenericArguments()[0]);
			specific_Register.Invoke(null, new[] { Activator.CreateInstance(type) });
		}
	}

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
