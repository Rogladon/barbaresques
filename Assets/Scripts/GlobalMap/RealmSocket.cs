using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public interface IMoneyAgent {
		int MoneyAgent();
	}

	public class RealmSocket : MonoBehaviour, INextTurnEventHandler {
		public Realm realmComponent;
		public int money;

		public void OnNextTurn(Scheduler scheduler) {
			foreach (var a in GetComponentsInChildren<IMoneyAgent>()) {
				money += a.MoneyAgent();
			}
		}
	}
}