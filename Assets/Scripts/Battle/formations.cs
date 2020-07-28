using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Barbaresques.Battle {
	public interface ICrowdFormationPositionsDistributor {
		NativeArray<float3> Distribute(Transform root, int unitsCount, float intervalBetweenUnits);
	}

	public class SquareCrowdFormation : ICrowdFormationPositionsDistributor {
		public NativeArray<float3> Distribute(Transform transform, int unitsCount, float intervalBetweenUnits) {
			NativeArray<float3> na = new NativeArray<float3>(unitsCount, Allocator.Temp);
			int width = (int)math.round(math.sqrt(unitsCount) + 0.49f);
			int x = 0;
			int y = -(int)math.round(width / 2 - 0.49f);

			for (int i = 0; i < na.Length; i++) {
				na[i] = transform.position + (transform.right * y - transform.forward * x) * intervalBetweenUnits;
				y++;
				if (y >= math.round(width/2+0.5f)) {
					x++;
					y = -(int)math.round(width/2-0.49f);
				}
			}
			return na;
		}
	}
	public class TriangleCrowdFormation : ICrowdFormationPositionsDistributor {
		public NativeArray<float3> Distribute(Transform transform, int unitsCount, float intervalBetweenUnits) {
			NativeArray<float3> na = new NativeArray<float3>(unitsCount, Allocator.Temp);
			int width = 1;
			int x = 0;
			int y = 0;

			for (int i = 0; i < na.Length; i++) {
				na[i] = transform.position + (transform.right * y - transform.forward * x) * intervalBetweenUnits;
				y++;
				if (y > (width - 1) / 2) {
					x++;
					width += 2;
					y = -(width - 1) / 2;

				}
			}
			return na;;
		}
	}
}
