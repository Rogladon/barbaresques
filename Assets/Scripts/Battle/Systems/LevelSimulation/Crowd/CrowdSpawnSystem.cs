using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(CrowdSystemGroup)), UpdateBefore(typeof(CrowdSystem))]
	public class CrowdSpawnSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
		private RandomSystem _randomSystem;

		private EntityArchetype _archetypeCrowd;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			_randomSystem = World.GetOrCreateSystem<RandomSystem>();

			_archetypeCrowd = World.EntityManager.CreateArchetype(new ComponentType[] {
				typeof(Crowd),
				typeof(OwnedByRealm),
			});
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
			var randoms = _randomSystem.randoms;
			EntityArchetype archetypeCrowd = _archetypeCrowd;

			Entities.WithName("SpawnCrowd")
				.WithoutBurst()
				.ForEach((int nativeThreadIndex, int entityInQueryIndex, Entity e, in Translation translation, in SpawnCrowd spawn) => {
					var random = randoms[nativeThreadIndex];

					var crowd = ecb.CreateEntity(entityInQueryIndex, archetypeCrowd);
					ecb.SetComponent(entityInQueryIndex, crowd, new OwnedByRealm() { owner = spawn.owner });
					// ecb.AddComponent(entityInQueryIndex, crowd, new CrowdTargetPosition() { value = translation.Value });

					// TODO: Переписать на NativeArray
					for (int i = 0; i < spawn.count; i++) {
						var member = ecb.Instantiate(entityInQueryIndex, spawn.crowdMemberPrefab);
						ecb.AddComponent(entityInQueryIndex, member, new CrowdMember() { crowd = crowd, policy = CrowdMemberPolicy.IDLE });
						ecb.SetComponent(entityInQueryIndex, member, new OwnedByRealm() { owner = spawn.owner });
						ecb.SetComponent(entityInQueryIndex, member, new Translation() {
							Value = translation.Value + random.NextFloat3(new float3(-16.0f, 0, -16.0f), new float3(16.0f, 0, 16.0f)),
						});
					}

					ecb.DestroyEntity(entityInQueryIndex, e);

					randoms[nativeThreadIndex] = random;
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}

