using UnityEngine;

namespace bkTools
{
	/// <summary>
	/// Minimal ScriptableObject-based ID. Uses name hash for the numeric id.
	/// </summary>
	public abstract class Identification : ScriptableObject
	{
		[SerializeField] protected string displayName;
		[SerializeField] protected int id;

		public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
		public int Id => id;

		public static implicit operator int(Identification reference)
		{
			return reference != null ? reference.id : 0;
		}

		#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			if (string.IsNullOrEmpty(displayName)) displayName = name;
			// Simple rule: derive numeric id from asset name to avoid project-wide scans
			id = Animator.StringToHash(name);
		}

		[ContextMenu("Rehash ID From Name")]
		protected void RehashFromName()
		{
			id = Animator.StringToHash(name);
			displayName = name;
			UnityEditor.EditorUtility.SetDirty(this);
		}
		#endif
	}
}


