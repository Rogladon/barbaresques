using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	public class CrowdBarController : MonoBehaviour {
		private static World World => World.DefaultGameObjectInjectionWorld;
		private static EntityManager entityManager => World.EntityManager;

		#region Prefabs & Components
#pragma warning disable 649
		[Header("Components")]
		[SerializeField]
		private Button _btnCancelTarget;

		[SerializeField]
		private Slider _moralSlider;
#pragma warning restore 649
		#endregion

		public Entity crowdEntity = Entity.Null;
		private Entity lastCrowdEntity = Entity.Null;

		void Update() {
			if (lastCrowdEntity != crowdEntity) {
				Refresh();
			}
		}

		public void Refresh() {
			var maxMoral = entityManager.GetComponentData<MaxMoral>(crowdEntity);
			var moral = entityManager.GetComponentData<MaxMoral>(crowdEntity);

			_btnCancelTarget.interactable = entityManager.HasComponent<CrowdTargetPosition>(crowdEntity);

			_moralSlider.maxValue = maxMoral.value;
			_moralSlider.value = moral.value;
		}

		public void CancelTarget() {
			if (crowdEntity == Entity.Null) return;

			if (entityManager.HasComponent<CrowdTargetPosition>(crowdEntity)) {
				if (!entityManager.RemoveComponent<CrowdTargetPosition>(crowdEntity)) {
					Debug.Log("Failed to remove component");
				}
				Refresh();
			}
		}
	}
}