using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace AnimBakery {
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class AnimSystem : SystemBase {
		protected override void OnUpdate() {
			var delta = Time.DeltaTime;

			List<AnimComponent> animComponents = new List<AnimComponent>();
			EntityManager.GetAllUniqueSharedComponentData(animComponents);

			foreach (AnimComponent anim in animComponents) {
				bool prepared = false;

				Entities.WithName("Animation")
					.WithSharedComponentFilter(anim)
					.WithoutBurst()
					.ForEach((Entity e, in Translation translation, in Rotation rotation) => {
						if (!prepared) {
							anim.drawer.PrepareFrame(delta);
							prepared = true;
						}
						anim.drawer.PushInstance(translation.Value, rotation.Value);
					})
					.Run();
				if (prepared) {
					anim.drawer.Draw();
				}
			}
		}
	}
}
