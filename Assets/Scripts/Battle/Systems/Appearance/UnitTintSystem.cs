using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(AppearanceSystemGroup))]
	public class UnitTintGroup : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
		private RandomSystem _randomSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			_randomSystem = World.GetOrCreateSystem<RandomSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			Entities.WithName("UnitAppearance_update")
				.WithAll<UnitAppearance>()
				.ForEach((int entityInQueryIndex, Entity e, ref MaterialColor mc, in Parent parent) => {
					if (!HasComponent<OwnedByRealm>(parent.Value))
						return;
					var obr = GetComponent<OwnedByRealm>(parent.Value);
					if (obr.owner == Entity.Null)
						return;
					if (!HasComponent<Realm>(obr.owner))
						return;
					var realm = GetComponent<Realm>(obr.owner);
					mc.Value = new float4(realm.color.r, realm.color.g, realm.color.b, realm.color.a);
				})
				.ScheduleParallel();

			Entities.WithName("UnitAppearance_tint")
				.WithAll<UnitAppearance>()
				.WithNone<MaterialColor>()
				.ForEach((int entityInQueryIndex, Entity e, in Parent parent) => {
					if (!HasComponent<OwnedByRealm>(parent.Value))
						return;
					var obr = GetComponent<OwnedByRealm>(parent.Value);
					if (obr.owner == Entity.Null)
						return;
					if (!HasComponent<Realm>(obr.owner))
						return;
					var realm = GetComponent<Realm>(obr.owner);
					ecb.AddComponent(entityInQueryIndex, e, new MaterialColor() { Value = new float4(realm.color.r, realm.color.g, realm.color.b, realm.color.a) });
				})
				.ScheduleParallel();

			Entities.WithName("UnitAppearance_cleanup")
				.WithNone<Parent>()
				.WithAll<MaterialColor>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.RemoveComponent<MaterialColor>(entityInQueryIndex, e);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
