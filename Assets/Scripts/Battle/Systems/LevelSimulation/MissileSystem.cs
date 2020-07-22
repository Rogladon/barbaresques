using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Jobs;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Burst;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(LevelSimulationSystemGroup)),UpdateBefore(typeof(UnitSystemGroup))]
	public class MissileSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
		private BuildPhysicsWorld _buildPhysicsWorld;
		private StepPhysicsWorld _stepPhysicsWorld;

		[BurstCompile]
		private struct HitJob : ITriggerEventsJob {
			[ReadOnly]
			public ComponentDataFromEntity<Missile> missileGroup;
			public ComponentDataFromEntity<Health> healthGroup;
			[ReadOnly]
			public ComponentDataFromEntity<Died> diedGroup;
			public EntityCommandBuffer ecb;

			public void Execute(TriggerEvent triggerEvent) {
				var a = triggerEvent.EntityA;
				var b = triggerEvent.EntityB;

				bool isMissileA = missileGroup.HasComponent(a);
				bool isMissileB = missileGroup.HasComponent(b);

				if (isMissileA && isMissileB) {
					ecb.DestroyEntity(a);
					ecb.DestroyEntity(b);
					return;
				}

				bool isHealthyA = healthGroup.HasComponent(a);
				bool isHealthyB = healthGroup.HasComponent(b);

				if (isHealthyA && isHealthyB)
					return;

				if ((isMissileA && !isHealthyB) ||
					(isMissileB && !isHealthyA))
					return;

				var missileEntity = isMissileA ? a : b;
				var unitEntity = isHealthyA ? a : b;

				// Мёртвые не тлеют, не горят
				if (diedGroup.HasComponent(unitEntity))
					return;

				// Наносим урон
				if (missileGroup.HasComponent(missileEntity)) {
					var missileMissile = missileGroup[missileEntity];
					var unitHealth = healthGroup[unitEntity];

					unitHealth.value -= missileMissile.damage;

					healthGroup[unitEntity] = unitHealth;
				}

				// Пуля исчезает
				ecb.DestroyEntity(missileEntity);
			}
		}

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			_buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
			_stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

			var delta = Time.DeltaTime;

			JobHandle flight = Entities.WithName(nameof(flight))
				.ForEach((ref Translation t, in Rotation r, in Missile m) => {
					t.Value += delta * m.speed * math.forward(r.Value);
				})
				.ScheduleParallel(Dependency);

			JobHandle cleanup = Entities.WithName(nameof(cleanup))
				.WithAll<Missile>()
				.ForEach((Entity e, in Translation t) => {
					if (math.length(t.Value) > 100.0f) {
						ecb.DestroyEntity(e);
					}
				})
				.Schedule(flight);

			JobHandle collisions = new HitJob() {
				missileGroup = GetComponentDataFromEntity<Missile>(true),
				healthGroup = GetComponentDataFromEntity<Health>(),
				diedGroup = GetComponentDataFromEntity<Died>(),
				ecb = ecb,
			}.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, cleanup);

			Dependency = collisions;

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}