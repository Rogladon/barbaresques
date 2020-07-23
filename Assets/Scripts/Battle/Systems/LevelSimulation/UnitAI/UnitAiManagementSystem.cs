using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Debug = UnityEngine.Debug;

namespace Barbaresques.Battle {
	/// <summary>
	/// Инициализация, деинициализация, переключение состояний ИИ юнитов.
	/// <br/>
	/// Должен исполняться первым в группе!
	/// </summary>
	[UpdateInGroup(typeof(UnitAiSystemGroup), OrderFirst = true)]
	public class UnitAiManagementSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			// Закидываем состояния на новые юниты
			Entities.WithName("init")
				.WithAll<UnitAi>()
				.WithNone<UnitAiState>()
				.ForEach((Entity e, int entityInQueryIndex) => {
					ecb.AddComponent(entityInQueryIndex, e, new UnitAiState() { });
					ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitch() {
						// Т.к. совпадают, должна быть просто инициализация
						previousState = UnitAiStates.IDLE,
						newState = UnitAiStates.IDLE,
					});
				})
				.ScheduleParallel();

			// Переключаем состояния
			Entities.WithName("switch")
				.WithAll<UnitAi>()
				.ForEach((Entity e, int entityInQueryIndex, ref UnitAiState aiState, in UnitAiStateSwitch switched) => {
					if (!switched.initialization) {
						foreach (AssociatedComponentAttribute aca in AssociatedComponentAttribute.OfEnum(switched.previousState)) {
							ecb.RemoveComponent(entityInQueryIndex, e, aca.type);
						}
					}
					foreach (AssociatedComponentAttribute aca in AssociatedComponentAttribute.OfEnum(switched.newState)) {
						ecb.AddComponent(entityInQueryIndex, e, aca.type);
					}
					ecb.RemoveComponent<UnitAiStateSwitch>(entityInQueryIndex, e);
					aiState.state = switched.newState;
				})
				.WithoutBurst()
				.ScheduleParallel();

			// Удаляем состояния с задестроенных юнитов
			Entities.WithName("cleanup_states")
				.WithNone<UnitAi>()
				.ForEach((Entity e, int entityInQueryIndex, in UnitAiState state) => {
					// Удаление компонентов, связанных с состоянием машины состояния
					foreach (AssociatedComponentAttribute aca in AssociatedComponentAttribute.OfEnum(state.state)) {
						ecb.RemoveComponent(entityInQueryIndex, e, aca.type);
					}
					ecb.RemoveComponent<UnitAiState>(entityInQueryIndex, e);
				})
				.WithoutBurst()
				.ScheduleParallel();

			// Удаляем переключатели состояний с задестроенных юнитов
			// (по идее не должны оставаться, но мало ли)
			Entities.WithName($"cleanup_{nameof(UnitAiStateSwitch)}")
				.WithNone<UnitAi>()
				.WithAll<UnitAiStateSwitch>()
				.ForEach((Entity e, int entityInQueryIndex) => ecb.RemoveComponent<UnitAiStateSwitch>(entityInQueryIndex, e))
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
