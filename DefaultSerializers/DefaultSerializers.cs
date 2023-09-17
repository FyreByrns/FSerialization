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
}
