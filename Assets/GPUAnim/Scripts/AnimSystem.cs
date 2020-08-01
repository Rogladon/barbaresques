using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
namespace AnimBakery {
	public class AnimSystem : SystemBase {

		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

			var delta = Time.DeltaTime;

			Entities.WithName("Animation")
				.WithoutBurst()
				.ForEach((Entity e, in Translation translation, in Rotation rotation, in AnimComponent anim) => {
					anim.drawer.Draw(delta, translation.Value, rotation.Value);
				})
				.Run();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
