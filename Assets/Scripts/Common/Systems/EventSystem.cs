using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Events;
using Unity.Collections;
using System.Collections.Generic;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class EventSystem : SystemBase {
		private Dictionary<System.Type, List<object>> _eventHandlers = new Dictionary<Type, List<object>>();

		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		}
		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

			// EntityManager.GetComponentData<T>(Entity)
			var miGetComponentData = typeof(EntityManager).GetMethod(nameof(EntityManager.GetComponentData), new Type[] { typeof(Entity) });

			Entities.WithName("events")
				.WithAll<Event>()
				.ForEach((Entity eventEntity) => {
					foreach (var h in _eventHandlers) {
						if (EntityManager.HasComponent(eventEntity, h.Key)) {
							var componentData = miGetComponentData.MakeGenericMethod(h.Key).Invoke(EntityManager, new object[] { eventEntity });

							var miHandlerInvoke = typeof(UnityAction<>).MakeGenericType(h.Key).GetMethod(nameof(UnityAction.Invoke));

							foreach (var ua in h.Value) {
								miHandlerInvoke.Invoke(ua, new object[] { componentData });
							}
						}
					}
					ecb.DestroyEntity(eventEntity);
				})
				.WithoutBurst()
				.Run();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}

		public void AddEventHandler<T>(UnityAction<T> a) where T : struct, IComponentData, IEventData {
			if (_eventHandlers.ContainsKey(typeof(T))) {
				_eventHandlers[typeof(T)].Add(a);
			} else {
				_eventHandlers[typeof(T)] = new List<object>() { a };
			}
		}
	}
}
