using UnityEngine;

namespace bkTools
{
	/// <summary>
	/// 간단한 근접 무기: 트리거 콜라이더 + Damager로 히트 전달
	/// - 애니메이션 이벤트에서 MainAttackStart/Release를 호출해 데미지 윈도우 제어
	/// - Stats/Absorption 등 복잡 로직은 사용하지 않음(단순 수치 전달)
	/// </summary>
	[DisallowMultipleComponent]
	public class SimpleMeleeWeapon : MonoBehaviour, IWeapon
	{
		[SerializeField] private bool active = true;
		[SerializeField] private bool canAim = false;
		[SerializeField] private bool rightHanded = true;
		[SerializeField] private int weaponType = 1;
		[SerializeField] private float baseDamage = 25f;
		[SerializeField] private bool criticalHits = false;
		[SerializeField] private Collider hitCollider; // 무기 타격 콜라이더(트리거)
		[SerializeField] private Damager damager;     // 데미지 전달자

		private WeaponManager ownerManager;
		private bool initialized;

		public bool Active { get => active; set => active = value; }
		public bool CanAim => canAim;
		public bool IsRightHanded => rightHanded;
		public int WeaponType => weaponType;

		void Awake()
		{
			EnsureComponents();
			SetColliderEnabled(false);
		}

		public void Equip(WeaponManager manager)
		{
			ownerManager = manager;
			EnsureComponents();
			// Damager의 기본 파라미터를 무기 데이터로 업데이트
			if (damager != null)
			{
				var so = damager.GetType().GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (so != null) so.SetValue(damager, baseDamage);
				so = damager.GetType().GetField("critical", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (so != null) so.SetValue(damager, criticalHits);
			}
		}

		public void Unequip(WeaponManager manager)
		{
			SetColliderEnabled(false);
			ownerManager = null;
		}

		public void MainAttackStart(WeaponManager manager)
		{
			if (!Active) return;
			SetColliderEnabled(true);
		}

		public void MainAttackRelease(WeaponManager manager)
		{
			SetColliderEnabled(false);
		}

		public bool TryReload(WeaponManager manager)
		{
			return false;
		}

		public void Configure(float damage, bool isCritical, bool isRightHand)
		{
			baseDamage = damage;
			criticalHits = isCritical;
			rightHanded = isRightHand;
			if (damager != null)
			{
				var so = damager.GetType().GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (so != null) so.SetValue(damager, baseDamage);
				so = damager.GetType().GetField("critical", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (so != null) so.SetValue(damager, criticalHits);
			}
		}

		void EnsureComponents()
		{
			if (initialized) return;
			if (damager == null) damager = GetComponent<Damager>();
			if (damager == null) damager = gameObject.AddComponent<Damager>();
			if (hitCollider == null) hitCollider = GetComponent<Collider>();
			if (hitCollider == null)
			{
				var mf = GetComponentInChildren<MeshFilter>();
				if (mf != null) hitCollider = mf.gameObject.AddComponent<BoxCollider>();
			}
			if (hitCollider != null)
			{
				hitCollider.isTrigger = true;
			}
			initialized = true;
		}

		void SetColliderEnabled(bool enabled)
		{
			if (hitCollider != null) hitCollider.enabled = enabled;
		}
	}
}


