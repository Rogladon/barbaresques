using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Barbaresques.GlobalMap {
	public class Scheduler : MonoBehaviour {
		public UnityEvent onNextRealm = new UnityEvent();
		public UnityEvent onNextTurn = new UnityEvent();

		public RealmSocket current => _realms[_currentRealm];

		private RealmSocket[] _realms;
		private int _currentRealm = 0;

		void Awake() {
			_realms = GetComponentsInChildren<RealmSocket>();
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