using UnityEngine;

namespace bkTools
{
	/// <summary>
	/// 무기가 구현해야 하는 최소 인터페이스
	/// </summary>
	public interface IWeapon
	{
		/// <summary>무기 활성/비활성 (비활성 시 장착/공격 불가)</summary>
		bool Active { get; set; }
		/// <summary>조준 지원 여부</summary>
		bool CanAim { get; }
		/// <summary>오른손 무기 여부(true면 오른손, false면 왼손)</summary>
		bool IsRightHanded { get; }
		/// <summary>무기 타입 식별용 정수값(애니메이션 등에서 사용 가능)</summary>
		int WeaponType { get; }

		/// <summary>무기 장착 시 호출</summary>
		void Equip(WeaponManager manager);
		/// <summary>무기 해제 시 호출</summary>
		void Unequip(WeaponManager manager);
		/// <summary>메인 공격 시작 (버튼 다운 시)</summary>
		void MainAttackStart(WeaponManager manager);
		/// <summary>메인 공격 해제 (버튼 업 시)</summary>
		void MainAttackRelease(WeaponManager manager);
		/// <summary>재장전 시도. 성공적으로 시작되면 true 반환</summary>
		bool TryReload(WeaponManager manager);
	}
}


