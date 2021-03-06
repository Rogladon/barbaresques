using Barbarian.Animations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Barbaresques.Battle {
	[MaterialProperty("_RealmColor", MaterialPropertyFormat.Float4)]
	public struct UnitTint : ISystemStateComponentData {
		public float4 Value;
	}

	[UpdateInGroup(typeof(AppearanceSystemGroup))]
	public class UnitTintSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
		private RandomSystem _randomSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			_randomSystem = World.GetOrCreateSystem<RandomSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			Entities.WithName("update")
				.WithAll<UnitAppearance>()
				.WithNone<Died>()
				.ForEach((int entityInQueryIndex, Entity e, ref UnitTint mc, in Parent parent) => {
					if (!HasComponent<OwnedByRealm>(parent.Value))
						return;
					var obr = GetComponent<OwnedByRealm>(parent.Value);
					if (obr.owner == Entity.Null)
						return;
					if (!HasComponent<Realm>(obr.owner))
						return;
					var realm = GetComponent<Realm>(obr.owner);
					mc.Value = new float4(realm.color.r, realm.color.g, realm.color.b, realm.color.a);
					if (HasComponent<Died>(parent.Value)) {
						mc.Value *= 0.25f;
					}
				})
				.ScheduleParallel();

			Entities.WithName("tint")
				.WithAll<UnitAppearance>()
				.WithNone<UnitTint, Died>()
				.ForEach((int entityInQueryIndex, Entity e, in Parent parent) => {
					if (!HasComponent<OwnedByRealm>(parent.Value))
						return;
					var obr = GetComponent<OwnedByRealm>(parent.Value);
					if (obr.owner == Entity.Null)
						return;
					if (!HasComponent<Realm>(obr.owner))
						return;
					var realm = GetComponent<Realm>(obr.owner);
					ecb.AddComponent(entityInQueryIndex, e, new UnitTint() { Value = new float4(realm.color.r, realm.color.g, realm.color.b, realm.color.a) });
				})
				.ScheduleParallel();

			Entities
				.WithAll<UnitAppearance>()
				.WithNone<UnitTint, Died>()
				.ForEach((int entityInQueryIndex, Entity e, in OwnedByRealm obr) => {
					if (obr.owner == Entity.Null)
						return;
					if (!HasComponent<Realm>(obr.owner))
						return;
					var realm = GetComponent<Realm>(obr.owner);
					ecb.AddComponent(entityInQueryIndex, e, new UnitTint() { Value = new float4(realm.color.r, realm.color.g, realm.color.b, realm.color.a) });
				})
				.ScheduleParallel();

			Entities
				.WithAll<UnitAppearance>()
				.WithNone<Died>()
				.ForEach((int entityInQueryIndex, Entity e, ref UnitTint mc, in OwnedByRealm obr) => {
					if (obr.owner == Entity.Null)
						return;
					if (!HasComponent<Realm>(obr.owner))
						return;
					var realm = GetComponent<Realm>(obr.owner);
					mc.Value = new float4(realm.color.r, realm.color.g, realm.color.b, realm.color.a);
					if (HasComponent<Died>(e)) {
						mc.Value *= 0.25f;
					}
				})
				.ScheduleParallel();

			Entities.WithName("cleanup")
				.WithNone<Parent>()
				.WithAll<UnitTint>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.RemoveComponent<UnitTint>(entityInQueryIndex, e);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

			Entities.WithAll<UnitAppearance>().ForEach((ref AnimationConfig config, in UnitTint ut) => { config.tint = ut.Value; }).ScheduleParallel();
		}
	}
}
