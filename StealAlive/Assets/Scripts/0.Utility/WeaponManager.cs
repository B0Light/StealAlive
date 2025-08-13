using UnityEngine;
using UnityEngine.Events;

namespace bkTools
{
	/// <summary>
	/// 단순 무기 매니저: 장착/해제/조준/공격/재장전만 관리
	/// - Animator/IK/Combo/홀스터 없음
	/// - 왼손/오른손 장착 포인트만 지원
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("bkTools/Weapons/WeaponManager")] 
	public class WeaponManager : MonoBehaviour
	{
		[Header("장착 포인트")]
		[SerializeField] Transform leftHand;
		[SerializeField] Transform rightHand;
		[SerializeField] bool parentOnEquip = true; // 장착 시 무기를 손에 붙일지 여부

		[Header("현재 상태")]
		[SerializeField] bool canAim = true; // 조준 가능 여부
		public bool Aim { get; private set; }
		public bool IsReloading { get; private set; }
		public IWeapon Weapon { get; private set; }

		[Header("이벤트")]
		public UnityEvent<GameObject> OnEquipWeapon = new();
		public UnityEvent<GameObject> OnUnequipWeapon = new();
		public UnityEvent<bool> OnAim = new();
		public UnityEvent OnAttackStart = new();
		public UnityEvent OnAttackReleased = new();

		public bool CanAim
		{
			get => canAim;
			set
			{
				if (canAim == value) return;
				canAim = value;
				if (!canAim && Aim) SetAim(false);
			}
		}

		public void Equip(IWeapon weapon)
		{
			if (weapon == null) return;
			if (Weapon != null) Unequip();

			Weapon = weapon;
			Weapon.Active = true;
			Weapon.Equip(this);

			if (parentOnEquip)
			{
				var asComponent = (weapon as Component);
				if (asComponent != null)
				{
					var parent = weapon.IsRightHanded ? rightHand : leftHand;
					if (parent != null)
					{
						asComponent.transform.SetParent(parent, worldPositionStays: false);
						asComponent.transform.localPosition = Vector3.zero;
						asComponent.transform.localRotation = Quaternion.identity;
					}
				}
			}

			OnEquipWeapon.Invoke((Weapon as Component)?.gameObject);
		}

		public void Unequip()
		{
			if (Weapon == null) return;
			Weapon.Unequip(this);
			OnUnequipWeapon.Invoke((Weapon as Component)?.gameObject);
			Weapon = null;
			SetAim(false);
		}

		public void SetAim(bool value)
		{
			if (Weapon == null) { Aim = false; return; }
			if (!Weapon.CanAim) { value = false; }
			if (!CanAim) { value = false; }

			if (Aim == value) return;
			Aim = value;
			OnAim.Invoke(Aim);
		}

		public void MainAttack(bool pressed)
		{
			if (Weapon == null) return;
			if (pressed)
			{
				OnAttackStart.Invoke();
				Weapon.MainAttackStart(this);
			}
			else
			{
				Weapon.MainAttackRelease(this);
				OnAttackReleased.Invoke();
			}
		}

		public void TryReload()
		{
			if (Weapon == null) return;
			if (IsReloading) return;
			if (Weapon.TryReload(this)) IsReloading = true;
		}

		// 재장전 완료 시 외부(무기)에서 호출
		public void NotifyReloadFinished()
		{
			IsReloading = false;
		}
	}
}


