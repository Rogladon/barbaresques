using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques {
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class RandomSystem : SystemBase {
		public NativeArray<Random> randoms { get; private set; }

		protected override void OnCreate() {
			base.OnCreate();
			
			var randomsArray = new Random[Unity.Jobs.LowLevel.Unsafe.JobsUtility.MaxJobThreadCount];

			var seed = new System.Random();
			for (int i = 0; i < randomsArray.Length; i++) {
				randomsArray[i] = new Random((uint)seed.Next());
			}

			randoms = new NativeArray<Random>(randomsArray, Allocator.Persistent);
		}

		protected override void OnUpdate() {}

		protected override void OnDestroy() {
			randoms.Dispose();
		}
	}
}
