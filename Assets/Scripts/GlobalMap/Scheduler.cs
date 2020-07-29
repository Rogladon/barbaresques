using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Barbaresques.GlobalMap {
	public interface INextTurnEventHandler {
		void OnNextTurn(Scheduler scheduler);
	}

	public class Scheduler : MonoBehaviour {
		public UnityEvent onNextRealm = new UnityEvent();
		public UnityEvent onNextTurn = new UnityEvent();

		public RealmSocket current => _realms[_currentRealm];

		private RealmSocket[] _realms;
		private int _currentRealm = 0;

		public RealmSocket[] realms => _realms;

		void Awake() {
			_realms = GetComponentsInChildren<RealmSocket>();

			onNextTurn.AddListener(() => {
				foreach (var h in GetComponentsInChildren<INextTurnEventHandler>()) {
					h.OnNextTurn(this);
				}
			});
		}

		public void NextTurn() {
			_currentRealm++;
			if (_currentRealm >= _realms.Length) {
				_currentRealm = 0;
				onNextTurn.Invoke();
			}
			onNextRealm.Invoke();
		}
	}
}