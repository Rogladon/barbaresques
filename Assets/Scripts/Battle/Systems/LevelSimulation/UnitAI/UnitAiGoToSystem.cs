using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup)), UpdateAfter(typeof(UnitAiManagementSystem))]
	public class UnitAiGoToSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			float targetRadius = 10.0f;

			JobHandle setTarget = Entities.WithName(nameof(setTarget))
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateGoTo>()
				.WithNone<Walking>()
				.ForEach((int entityInQueryIndex, Entity e, in CrowdMember crowdMember) => {
					// TODO: чек на расстояние до цели
					ecb.AddComponent(entityInQueryIndex, e, new Walking() {
						target = crowdMember.targetLocation,
						speedFactor = 1,
						targetRadius = targetRadius,
						stopAfterSecsInRadius = 3.0f,
					});
				})
				.ScheduleParallel(Dependency);

			JobHandle updateTarget = Entities.WithName(nameof(updateTarget))
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateGoTo>()
				.ForEach((int entityInQueryIndex, Entity e, ref Walking walking, in CrowdMember crowdMember) => {
					// TODO: чек на расстояние до цели
					walking = new Walking() {
						target = crowdMember.targetLocation,
						speedFactor = 1,
						targetRadius = targetRadius,
						stopAfterSecsInRadius = 3.0f,
					};
				})
				.ScheduleParallel(Dependency);
			
			Dependency = JobHandle.CombineDependencies(setTarget, updateTarget);

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}

