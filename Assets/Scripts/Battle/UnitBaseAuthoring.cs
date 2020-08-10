using Barbaresques;
using Barbaresques.Battle;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AnimBakery {
	public class UnitBaseAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
		public int health;
		public int maxHealth;
		public float speed;

		public void Convert(Entity unit, EntityManager em, GameObjectConversionSystem conversionSystem) {
			em.AddComponent<Unit>(unit);
			em.AddComponentData(unit, new Health { value = health });
			em.AddComponentData(unit, new MaxHealth { value = maxHealth });
			em.AddComponentData(unit, new Speed() { value = speed });
			em.AddComponent<OwnedByRealm>(unit);
			em.AddComponentData(unit, new RotationConstraint { axes = new bool3(true, false, true) });
		}
	}
}
