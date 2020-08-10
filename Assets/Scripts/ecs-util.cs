using System;
using Unity.Entities;

namespace Barbaresques {
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TagComponentAttribute : Attribute {}

	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class FlagComponentAttribute : Attribute {}

	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class StateComponentAttribute : Attribute {}

	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ParameterComponentAttribute : Attribute {}

	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class AssociatedComponentAttribute : Attribute {
		public Type type { get; private set; }

		public AssociatedComponentAttribute(Type type) {
			this.type = type;
			if (!typeof(IComponentData).IsAssignableFrom(type)) {
				throw new ArgumentException($"{nameof(type)} expected to implement {nameof(IComponentData)}");
			}
		}

		public static AssociatedComponentAttribute[] OfEnum<E>(E enumValue) where E : System.Enum {
			var attributes = enumValue.GetType()
				.GetMember(enumValue.ToString())[0]
				.GetCustomAttributes(typeof(AssociatedComponentAttribute), false);
			if (attributes.Length == 0) {
				UnityEngine.Debug.LogError($"Enum value {enumValue} got no {nameof(AssociatedComponentAttribute)}");
			}
			return (AssociatedComponentAttribute[])attributes;
		}
	}
}
