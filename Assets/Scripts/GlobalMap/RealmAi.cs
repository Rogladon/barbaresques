using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class RealmAi : MonoBehaviour, INextRealmEventHandler {
		private Scheduler _scheduler;
		public void OnNextRealm(Scheduler scheduler, RealmSocket current) {
			_scheduler = scheduler;

			if (current.gameObject == gameObject) {
				StartCoroutine(_MakeTurn());
			}
		}

		private IEnumerator _MakeTurn() {
			// TODO: ии
			yield return new WaitForSeconds(1.5f);
			_scheduler.NextTurn();
		}
	}
}
