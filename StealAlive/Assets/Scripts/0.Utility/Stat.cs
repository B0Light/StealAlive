using System;
using UnityEngine;
using UnityEngine.Events;

namespace bkTools
{
	/// <summary>
	/// Minimal stat container with bounds, optional regeneration and events.
	/// </summary>
	[Serializable]
	public class Stat
	{
		[SerializeField] private string id;
		[SerializeField] private float currentValue = 100f;
		[SerializeField] private float minValue = 0f;
		[SerializeField] private float maxValue = 100f;
		[SerializeField] private float regenPerSecond = 0f;
		[SerializeField] private bool clampToBounds = true;
		[SerializeField] private bool enableRegeneration = true;

		public UnityEvent<float> OnValueChanged = new();
		public UnityEvent OnMinReached = new();
		public UnityEvent OnMaxReached = new();

		public string Id => id;
		public float Current => currentValue;
		public float Min => minValue;
		public float Max => maxValue;
		public float RegenPerSecond
		{
			get => regenPerSecond;
			set => regenPerSecond = value;
		}
		public bool ClampToBounds
		{
			get => clampToBounds;
			set => clampToBounds = value;
		}
		public bool EnableRegeneration
		{
			get => enableRegeneration;
			set => enableRegeneration = value;
		}

		public float Normalized
		{
			get
			{
				float range = maxValue - minValue;
				if (Mathf.Approximately(range, 0f)) return 0f;
				return Mathf.Clamp01((currentValue - minValue) / range);
			}
		}

		public bool IsEmpty => currentValue <= minValue + Mathf.Epsilon;
		public bool IsFull => currentValue >= maxValue - Mathf.Epsilon;

		public void Initialize(string statId, float min, float max, float startValue, float regen = 0f, bool clamp = true)
		{
			id = statId;
			minValue = min;
			maxValue = max;
			regenPerSecond = regen;
			clampToBounds = clamp;
			EnsureOrderedBounds();
			Set(startValue);
		}

		public void SetBounds(float min, float max, bool keepCurrentWithinBounds = true)
		{
			minValue = min;
			maxValue = max;
			EnsureOrderedBounds();
			if (keepCurrentWithinBounds)
			{
				Set(currentValue);
			}
		}

		public void Set(float value)
		{
			float newValue = value;
			if (clampToBounds)
			{
				newValue = Mathf.Clamp(value, minValue, maxValue);
			}

			if (Mathf.Approximately(newValue, currentValue)) return;

			currentValue = newValue;
			OnValueChanged?.Invoke(currentValue);

			if (IsEmpty) OnMinReached?.Invoke();
			if (IsFull) OnMaxReached?.Invoke();
		}

		public float Add(float amount)
		{
			Set(currentValue + amount);
			return currentValue;
		}

		public void Fill() => Set(maxValue);
		public void Empty() => Set(minValue);

		public void Tick(float deltaTime)
		{
			if (!enableRegeneration) return;
			if (Mathf.Approximately(regenPerSecond, 0f)) return;
			if (IsFull && regenPerSecond > 0f) return;
			if (IsEmpty && regenPerSecond < 0f) return;
			Add(regenPerSecond * Mathf.Max(0f, deltaTime));
		}

		void EnsureOrderedBounds()
		{
			if (maxValue < minValue)
			{
				(maxValue, minValue) = (minValue, maxValue);
			}
		}
	}
}


