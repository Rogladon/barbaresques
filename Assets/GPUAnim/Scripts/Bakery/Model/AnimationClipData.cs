using UnityEngine;

namespace AnimBakery.Cook.Model {
	public struct AnimationClipData {
		private AnimationClip _clip;
		private string _name;
		private int _framesCount;
		private int _start;
		private int _end;

		public static AnimationClipData Create(
			AnimationClip clip,
			string name,
			int start,
			int end,
			int frameCount) {
			return new AnimationClipData {
				_clip = clip,
				_start = start,
				_end = end,
				_framesCount = frameCount,
				_name = name
			};
		}

		public string Name => _name;

		public float ClipLength => _clip.length;
		public AnimationClip Clip => _clip;
		public int Start => _start;
		public int End => _end;
		public int FramesCount => _framesCount;
	}
}