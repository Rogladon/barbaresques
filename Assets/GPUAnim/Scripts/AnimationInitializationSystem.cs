using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using AnimBakery.Cook;
using UnityEngine;
using AnimBakery.Cook.Model;
namespace AnimBakery {
	public class AnimInitSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		private Dictionary<AnimInitComponent, AnimationData> _animComponents = new Dictionary<AnimInitComponent, AnimationData>();

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			_animComponents = new Dictionary<AnimInitComponent, AnimationData>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

			List<AnimInitComponent> initComponents = new List<AnimInitComponent>();
			EntityManager.GetAllUniqueSharedComponentData(initComponents);

			AnimationData animData = new AnimationData();
			foreach (AnimInitComponent aic in initComponents) {
				Debug.Log($"{aic.GetHashCode()} {aic.bakery != null}");
				bool got = false;

				Entities.WithName("AnimationInit")
					.WithSharedComponentFilter(aic)
					.WithoutBurst()
					.ForEach((Entity e, in AnimInitComponent init) => {
						if (!got) {
							if (!_animComponents.TryGetValue(aic, out animData)) {
								BakedMeshData[] baked = new BakedMeshData[aic.bakery.Length];
								for (int i = 0; i < baked.Length; i++) {
									Material m = Material.Instantiate(aic.bakery[i].Material);
									baked[i] = BakedMeshData.Copy(aic.bakery[i], m);
								}

								animData = new AnimationData {
									baked = baked,
								};
								_animComponents[aic] = animData;
							}
							got = true;
						}
						ecb.AddSharedComponent(e, animData);
						ecb.RemoveComponent<AnimInitComponent>(e);
					})
					.Run();
			}

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
