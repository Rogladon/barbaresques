﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Barbarian.Animations.Cook {
	public class AnimationBakery : BaseBakery {
		private readonly Animation _animationComponent;
		private readonly SkinnedMeshRenderer _originalRenderer;

		public AnimationBakery(SkinnedMeshRenderer originalRenderer, Animation animationComponent) {
			if (animationComponent == null) {
				throw new ArgumentException("Animation couldn't be null ");
			}

			if (originalRenderer == null) {
				throw new ArgumentException("SkinnedMeshRenderer couldn't be null ");
			}
			if (originalRenderer.sharedMesh == null) {
				throw new ArgumentException("SkinnedMeshRenderer.Mesh couldn't be null ");
			}
			if (originalRenderer.bones == null) {
				throw new ArgumentException("SkinnedMeshRenderer.Bones couldn't be null ");
			}

			this._animationComponent = animationComponent;
			this._originalRenderer = originalRenderer;
		}

		protected override List<Matrix4x4[,]> SampleAnimationClips(float frameRate, List<Clip> clips, out int numberOfKeyFrames, out int numberOfBones) {
			foreach (var i in clips) {
				_animationComponent.AddClip(i.clip, i.name);
			}
			List<AnimationClip> animationClips = _animationComponent.GetAllAnimationClips();
			foreach (var clip in animationClips) {
				_animationComponent[clip.name].enabled = false;
				_animationComponent[clip.name].weight = 0f;
			}

			numberOfKeyFrames = 0;
			var sampledBoneMatrices = new List<Matrix4x4[,]>();
			foreach (var animationClip in animationClips) {
				var sampledMatrix = SampleAnimationClip(animationClip, _originalRenderer, _animationComponent, frameRate);
				sampledBoneMatrices.Add(sampledMatrix);

				numberOfKeyFrames += sampledMatrix.GetLength(0);
			}

			numberOfBones = sampledBoneMatrices[0].GetLength(1);

			return sampledBoneMatrices;
		}

		protected override Mesh CreateMesh() {
			return CreateMesh(_originalRenderer.sharedMesh);
		}

		protected override Material CreateMaterial() {
			return new Material(_originalRenderer.sharedMaterial);
		}

		private static Matrix4x4[,] SampleAnimationClip(AnimationClip clip, SkinnedMeshRenderer renderer, Animation animation, float frameRate) {
			var boneMatrices = new Matrix4x4[Mathf.CeilToInt(frameRate * clip.length), renderer.bones.Length];
			var bakingState = animation[clip.name];

			bakingState.enabled = true;
			bakingState.weight = 1f;
			for (var frameIndex = 0; frameIndex < boneMatrices.GetLength(0); frameIndex++) {
				var t = (float)frameIndex / (boneMatrices.GetLength(0));

				bakingState.normalizedTime = t;
				animation.Sample();

				for (var boneIndex = 0; boneIndex < renderer.bones.Length; boneIndex++) {
					boneMatrices[frameIndex, boneIndex] = renderer.bones[boneIndex].localToWorldMatrix * renderer.sharedMesh.bindposes[boneIndex];
				}
			}

			bakingState.enabled = false;
			bakingState.weight = 0f;

			return boneMatrices;
		}
	}
}