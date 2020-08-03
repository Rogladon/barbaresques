using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using AnimBakery.Cook;
using UnityEngine;
using AnimBakery.Draw;
using AnimBakery.Cook.Model;
namespace AnimBakery {
	public class AnimInitSystem : SystemBase {

		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

			var delta = Time.DeltaTime;
			int index = 0;
			Entities.WithName("AnimationInit")
				.WithoutBurst()
				.ForEach((Entity e, in AnimInitComponent init) => {
					AnimComponent anim = new AnimComponent {
						addAnimationDifference = init.anim.addAnimationDifference,
						animated = init.anim.animated,
						frameRate = init.anim.frameRate,
						normalizedTime = init.anim.normalizedTime,
						timeMultiplier = init.anim.timeMultiplier,
						animationId = init.anim.animationId,
					};
					BakedData[] bakery = new BakedData[init.bakery.Length];
					for (int i = 0; i < bakery.Length; i++) {
						Material m = Material.Instantiate(init.bakery[i].Material);
						bakery[i] = BakedData.Copy(init.bakery[i], m);
					}
					anim.drawer = new GPUAnimDrawer(bakery, anim, init.clips);
					anim.id = index;
					index++;
					ecb.AddSharedComponent(e, anim);
					ecb.RemoveComponent(e, typeof(AnimInitComponent));
				})
				.Run();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
