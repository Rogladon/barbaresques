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
	[UpdateInGroup(typeof(AiSystemGroup))]
	public class UnitAiManagementSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			// Закидываем состояния на новые юниты
			Entities.WithName("UnitAI_init")
				.WithAll<UnitAi>()
				.WithNone<UnitAiState>()
				.ForEach((Entity e, int entityInQueryIndex) => {
					ecb.AddComponent(entityInQueryIndex, e, new UnitAiState() {});
					ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitched() {
						// Т.к. совпадают, должна быть просто инициализация
						previousState = UnitAiStates.IDLE,
						newState = UnitAiStates.IDLE,
					});
				})
				.ScheduleParallel();

			// Переключаем состояния
			Entities.WithName("UnitAI_switch")
				.WithAll<UnitAi, UnitAiState>()
				.ForEach((Entity e, int entityInQueryIndex, in UnitAiStateSwitched switched) => {
					if (!switched.initialization) {
						foreach (AssociatedComponentAttribute aca in GetAssociatedComponentsOf(switched.previousState)) {
							ecb.RemoveComponent(entityInQueryIndex, e, aca.type);
						}
					}
					foreach (AssociatedComponentAttribute aca in GetAssociatedComponentsOf(switched.newState)) {
						ecb.AddComponent(entityInQueryIndex, e, aca.type);
					}
					ecb.RemoveComponent<UnitAiStateSwitched>(entityInQueryIndex, e);
				})
				.WithoutBurst()
				.ScheduleParallel();

			// Удаляем состояния с задестроенных юнитов
			Entities.WithName("UnitAI_deinit")
				.WithNone<UnitAi>()
				.ForEach((Entity e, int entityInQueryIndex, in UnitAiState state) => {
					// Удаление компонентов, связанных с состоянием машины состояния
					foreach (AssociatedComponentAttribute aca in GetAssociatedComponentsOf(state.state)) {
						ecb.RemoveComponent(entityInQueryIndex, e, aca.type);
					}
					ecb.RemoveComponent<UnitAiState>(entityInQueryIndex, e);
				})
				.WithoutBurst()
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}

		/// <summary>
		/// C Burst не работает, так что использовать выйдет только с <c>WithoutBurst()</c>
		/// </summary>
		private static AssociatedComponentAttribute[] GetAssociatedComponentsOf(UnitAiStates uis) {
			var attributes = uis.GetType()
				.GetMember(uis.ToString())[0]
				.GetCustomAttributes(typeof(AssociatedComponentAttribute), false);
			if (attributes.Length == 0) {
				Debug.LogError($"Enum value {uis} got no {nameof(AssociatedComponentAttribute)}");
			}
			return (AssociatedComponentAttribute[])attributes;
		}
	}
}
