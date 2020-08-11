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

			Entities.WithName("Animation")
				.WithoutBurst()
				.ForEach((Entity e, in Translation translation, in Rotation rotation, in AnimComponent anim) => {
					anim.drawer.Draw(delta, translation.Value, rotation.Value);
				})
				.Run();
		}
	}
}
