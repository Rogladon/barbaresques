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

		private Dictionary<AnimInitComponent, AnimComponent> _animComponents = new Dictionary<AnimInitComponent, AnimComponent>();

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			_animComponents = new Dictionary<AnimInitComponent, AnimComponent>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

			List<AnimInitComponent> initComponents = new List<AnimInitComponent>();
			EntityManager.GetAllUniqueSharedComponentData(initComponents);

			int animIndex = 0;
			AnimComponent anim = new AnimComponent();
			foreach (AnimInitComponent aic in initComponents) {
				bool got = false;

				Entities.WithName("AnimationInit")
					.WithSharedComponentFilter(aic)
					.WithoutBurst()
					.ForEach((Entity e, in AnimInitComponent init) => {
						if (!got) {
							if (!_animComponents.TryGetValue(aic, out anim)) {
								anim = new AnimComponent {
									addAnimationDifference = aic.anim.addAnimationDifference,
									animated = aic.anim.animated,
									frameRate = aic.anim.frameRate,
									normalizedTime = aic.anim.normalizedTime,
									timeMultiplier = aic.anim.timeMultiplier,
									animationId = aic.anim.animationId,
								};
								// Debug.Log($"{aic},{aic.bakery != null}");
								BakedData[] bakery = new BakedData[aic.bakery.Length];
								for (int i = 0; i < bakery.Length; i++) {
									Material m = Material.Instantiate(aic.bakery[i].Material);
									bakery[i] = BakedData.Copy(aic.bakery[i], m);
								}
								anim.drawer = new GPUAnimDrawer(bakery, anim, aic.clips);
								anim.id = animIndex;
								animIndex++;
								_animComponents[aic] = anim;
							}
							got = true;
						}
						ecb.AddSharedComponent(e, anim);
						ecb.RemoveComponent<AnimInitComponent>(e);
					})
					.Run();
			}

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
