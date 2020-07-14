using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques {
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class RandomSystem : SystemBase {
		public NativeArray<Random> randoms { get; private set; }

		protected override void OnCreate() {
			base.OnCreate();
			
			_AssembleRandoms();
		}

		protected override void OnUpdate() {
			if (Unity.Jobs.LowLevel.Unsafe.JobsUtility.MaxJobThreadCount > randoms.Length ) {
				randoms.Dispose();
				_AssembleRandoms();
			}
		}

		protected override void OnDestroy() {
			randoms.Dispose();
		}

		private void _AssembleRandoms() {
			var randomsArray = new Random[Unity.Jobs.LowLevel.Unsafe.JobsUtility.MaxJobThreadCount];

			var seed = new System.Random();
			for (int i = 0; i < randomsArray.Length; i++) {
				randomsArray[i] = new Random((uint)seed.Next());
			}

			randoms = new NativeArray<Random>(randomsArray, Allocator.Persistent);
		}
	}
}
