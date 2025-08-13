using UnityEngine;

namespace bkTools
{
	/// <summary>
	/// 데미지 전달에 사용하는 간단한 정보 구조체
	/// </summary>
	public struct DamageInfo
	{
		/// <summary>가해지는 데미지 양(+는 체력 감소)</summary>
		public float amount;
		/// <summary>피격 방향(가해자 -> 피격자)</summary>
		public Vector3 direction;
		/// <summary>피격 지점 위치</summary>
		public Vector3 position;
		/// <summary>데미지를 준 오브젝트</summary>
		public GameObject source;
		/// <summary>치명타 여부</summary>
		public bool isCritical;

		public DamageInfo(float amount, Vector3 direction, Vector3 position, GameObject source, bool isCritical)
		{
			this.amount = amount;
			this.direction = direction;
			this.position = position;
			this.source = source;
			this.isCritical = isCritical;
		}
	}

	/// <summary>
	/// 피격 가능한 대상이 구현해야 하는 최소 인터페이스
	/// </summary>
	public interface IDamageable
	{
		void ReceiveDamage(DamageInfo info);
	}
}


