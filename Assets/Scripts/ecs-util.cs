using System;
using Unity.Entities;

namespace Barbaresques {
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class AssociatedComponentAttribute : Attribute {
		public Type type { get; private set; }

		public AssociatedComponentAttribute(Type type) {
			this.type = type;
			if (!typeof(IComponentData).IsAssignableFrom(type)) {
				throw new ArgumentException($"{nameof(type)} expected to implement {nameof(IComponentData)}");
			}
		}
	}
}
