using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup)), UpdateAfter(typeof(UnitAiManagementSystem))]
	public class UnitAiFollowCrowdSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			Entities.WithName("UnitAi_followCrowd_setTask")
				.WithNone<UnitAiStateSwitched>()
				.WithAll<UnitAiStateFollowCrowd>()
				.WithNone<Walking>()
				.ForEach((int entityInQueryIndex, Entity e, in CrowdMember crowdMember) => {
					ecb.AddComponent(entityInQueryIndex, e, new Walking() { target = GetComponent<Crowd>(crowdMember.crowd).targetLocation, speedFactor = 1 });
				})
				.ScheduleParallel();

			Entities.WithName("UnitAi_followCrowd_updateTask")
				.WithNone<UnitAiStateSwitched>()
				.WithAll<UnitAiStateFollowCrowd>()
				.ForEach((int entityInQueryIndex, Entity e, ref Walking walking, in CrowdMember crowdMember) => {
					walking.target = GetComponent<Crowd>(crowdMember.crowd).targetLocation;
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}

