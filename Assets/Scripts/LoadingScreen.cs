using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Barbaresques {
	public class LoadingScreen : MonoBehaviour {
		public static LoadingScreen instance { get; private set; } = null;

		[Header("Components")]
		[SerializeField]
#pragma warning disable 649
		private Slider _progressBarSlider;
#pragma warning restore 649

		private bool _isLoading = false;
		private AsyncOperation _currentLoadingOperation = null;

		void Awake() {
			if (instance != null) {
				Destroy(gameObject);
				return;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);

			_progressBarSlider.maxValue = 1.0f;
		}

		void Update() {
			if (_isLoading) {
				SetProgress(_currentLoadingOperation.progress);

				if (_currentLoadingOperation.isDone) {
					Hide();
				}
			}
		}

		void SetProgress(float progress) {
			_progressBarSlider.value = progress;
		}

		public void Show(AsyncOperation asyncOperation) {
			gameObject.SetActive(true);

			_currentLoadingOperation = asyncOperation;
			_isLoading = true;

			SetProgress(0.0f);
		}

		public void Hide() {
			gameObject.SetActive(false);
			_currentLoadingOperation = null;
			_isLoading = false;
		}
	}
}
