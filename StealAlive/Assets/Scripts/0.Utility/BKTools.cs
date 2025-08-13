using System.Collections;
using UnityEngine;

namespace bkTools
{
	/// <summary>
	/// 가볍고 의존성이 없는 공용 유틸리티 함수 모음.
	/// 게임 플레이 및 에디터 작업에서 자주 쓰이는 기능을 간단히 제공합니다.
	/// </summary>
	public static class BKTools
	{
		/// <summary>
		/// (Time.time - startedAt) 값이 intervalSeconds 이상일 때 true를 반환합니다.
		/// </summary>
		public static bool Elapsed(float startedAt, float intervalSeconds)
		{
			return (Time.time - startedAt) >= intervalSeconds;
		}

		/// <summary>
		/// GameObject의 Layer가 주어진 LayerMask에 포함되어 있는지 확인합니다.
		/// </summary>
		public static bool InLayerMask(GameObject gameObject, LayerMask mask)
		{
			int layerMask = 1 << gameObject.layer;
			return (mask.value & layerMask) != 0;
		}

		/// <summary>
		/// 메인 카메라를 안전하게 찾습니다.
		/// Camera.main이 null이면, 활성화된 첫 번째 카메라를 반환합니다.
		/// </summary>
		public static Camera FindMainCamera()
		{
			if (Camera.main != null) return Camera.main;
			var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
			for (int i = 0; i < cameras.Length; i++)
			{
				if (cameras[i].isActiveAndEnabled) return cameras[i];
			}
			return cameras.Length > 0 ? cameras[0] : null;
		}

		/// <summary>
		/// 지정된 컴포넌트를 가져오거나, 없으면 새로 추가합니다.
		/// </summary>
		public static T GetOrAdd<T>(Component owner) where T : Component
		{
			if (owner == null) return null;
			var c = owner.GetComponent<T>();
			if (c == null) c = owner.gameObject.AddComponent<T>();
			return c;
		}

		/// <summary>
		/// Transform의 월드 위치를 duration 초 동안 선형 보간(Lerp)으로 이동시킵니다.
		/// 외부에서 제어할 수 있도록 시작된 Coroutine을 반환합니다.
		/// </summary>
		public static Coroutine LerpPosition(MonoBehaviour runner, Transform target, Vector3 toWorldPos, float duration)
		{
			if (runner == null || target == null) return null;
			return runner.StartCoroutine(LerpPositionRoutine(target, toWorldPos, duration));
		}

		/// <summary>
		/// Transform의 월드 회전을 duration 초 동안 구면 보간(Slerp)으로 회전시킵니다.
		/// 외부에서 제어할 수 있도록 시작된 Coroutine을 반환합니다.
		/// </summary>
		public static Coroutine LerpRotation(MonoBehaviour runner, Transform target, Quaternion toWorldRot, float duration)
		{
			if (runner == null || target == null) return null;
			return runner.StartCoroutine(LerpRotationRoutine(target, toWorldRot, duration));
		}

		static IEnumerator LerpPositionRoutine(Transform target, Vector3 toWorldPos, float duration)
		{
			Vector3 from = target.position;
			float t = 0f;
			duration = Mathf.Max(0f, duration);
			if (duration <= 0f)
			{
				target.position = toWorldPos;
				yield break;
			}
			while (t < duration)
			{
				t += Time.deltaTime;
				float p = Mathf.Clamp01(t / duration);
				target.position = Vector3.LerpUnclamped(from, toWorldPos, p);
				yield return null;
			}
			target.position = toWorldPos;
		}

		static IEnumerator LerpRotationRoutine(Transform target, Quaternion toWorldRot, float duration)
		{
			Quaternion from = target.rotation;
			float t = 0f;
			duration = Mathf.Max(0f, duration);
			if (duration <= 0f)
			{
				target.rotation = toWorldRot;
				yield break;
			}
			while (t < duration)
			{
				t += Time.deltaTime;
				float p = Mathf.Clamp01(t / duration);
				target.rotation = Quaternion.SlerpUnclamped(from, toWorldRot, p);
				yield return null;
			}
			target.rotation = toWorldRot;
		}

		/// <summary>
		/// 간단한 로그 출력 함수. context를 지정할 수 있습니다.
		/// </summary>
		public static void Log(object message, Object context = null)
		{
			if (context != null) Debug.Log(message, context); else Debug.Log(message);
		}

		public static void LogWarning(object message, Object context = null)
		{
			if (context != null) Debug.LogWarning(message, context); else Debug.LogWarning(message);
		}

		public static void LogError(object message, Object context = null)
		{
			if (context != null) Debug.LogError(message, context); else Debug.LogError(message);
		}

#if UNITY_EDITOR
		/// <summary>
		/// 에디터에서 객체를 Dirty 상태로 설정하여 변경 사항이 저장되도록 합니다.
		/// </summary>
		public static void SetDirty(Object obj)
		{
			if (obj == null) return;
			UnityEditor.EditorUtility.SetDirty(obj);
		}
#endif
	}
}
