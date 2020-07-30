using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Barbaresques.Battle {
	public struct AttackingSystemState : ISystemStateComponentData {
		public float colddown;
		public float sinceLast;
	}

	[UpdateInGroup(typeof(UnitSystemGroup))]
	public class AttackingSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			var delta = Time.DeltaTime;

			JobHandle init = Entities.WithName(nameof(init))
				.WithAll<Attacking>()
				.WithNone<AttackingSystemState>()
				.ForEach((int entityInQueryIndex, Entity e, in Weapon w) => {
					var ass = new AttackingSystemState() {
						colddown = w.shotsPerMinute > 0 ? 60.0f/(float)w.shotsPerMinute : 0.1f,
					};
					ass.sinceLast = ass.colddown * 100.0f;
					ecb.AddComponent(entityInQueryIndex, e, ass);
				})
				.ScheduleParallel(Dependency);

			JobHandle attack = Entities.WithName(nameof(attack))
				.WithAll<Unit>()
				.WithNone<Died>()
				.ForEach((int entityInQueryIndex, Entity e, ref Attacking a, ref AttackingSystemState ass, in Weapon w, in Translation t, in Rotation r, in OwnedByRealm ownership) => {
					if (a.burst > 0) {
						if (ass.colddown < ass.sinceLast) {
							ass.sinceLast = 0;
							var missile = ecb.Instantiate(entityInQueryIndex, w.missilePrefab);
							ecb.SetComponent(entityInQueryIndex, missile, new Translation() {
								Value = t.Value + math.forward(r.Value) * 0.75f + new float3(0, 1.75f, 0),
							});
							ecb.SetComponent(entityInQueryIndex, missile, r);
							ecb.AddComponent(entityInQueryIndex, missile, ownership);
							a.burst--;
						} else {
							ass.sinceLast += delta;
						}
					}
				})
				.ScheduleParallel(init);

			JobHandle cleanupDestroyed = Entities.WithName(nameof(cleanupDestroyed))
				.WithNone<Attacking>()
				.WithAll<AttackingSystemState>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.RemoveComponent<AttackingSystemState>(entityInQueryIndex, e);
				})
				.ScheduleParallel(attack);

			JobHandle cleanupCompleted = Entities.WithName(nameof(cleanupCompleted))
				.WithAll<AttackingSystemState>()
				.ForEach((int entityInQueryIndex, Entity e, in Attacking a) => {
					if (a.burst < 1) {
						ecb.RemoveComponent<Attacking>(entityInQueryIndex, e);
						ecb.RemoveComponent<AttackingSystemState>(entityInQueryIndex, e);
					}
				})
				.ScheduleParallel(cleanupDestroyed);

			Dependency = cleanupCompleted;

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}