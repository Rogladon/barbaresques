using UnityEngine;

namespace AnimBakery.Cook.Model {
	public struct AnimationClipData {
		private AnimationClip clip;
		private string name;
		private int framesCount;
		private int start;
		private int end;

		public static AnimationClipData Create(
			AnimationClip clip,
			string name,
			int start,
			int end,
			int frameCount) {
			return new AnimationClipData {
				clip = clip,
				start = start,
				end = end,
				framesCount = frameCount,
				name = name
			};
		}

		public string Name => name;

		public float ClipLength => clip.length;
		public AnimationClip Clip => clip;
		public int Start => start;
		public int End => end;
		public int FramesCount => framesCount;
	}
}