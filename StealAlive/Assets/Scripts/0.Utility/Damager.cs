using UnityEngine;

namespace bkTools
{
	/// <summary>
	/// 매우 단순한 공격자: 충돌 시 대상의 BkDamageable에 데미지를 전달합니다.
	/// </summary>
	[AddComponentMenu("bkTools/Damage/Damager")] 
	public class Damager : MonoBehaviour
	{
		[Header("데미지 설정")]
		[SerializeField] private float damage = 10f;             // 기본 데미지 양
		[SerializeField] private bool critical = false;           // 치명타 여부

		[Header("히트 필터")]
		[SerializeField] private LayerMask hitMask = ~0;          // 타격 대상 레이어
		[SerializeField] private bool useTrigger = true;          // 트리거 이벤트 사용 여부

		/// <summary>
		/// 외부에서 한 번 공격을 트리거할 때 사용(예: 원거리 명중 시점)
		/// </summary>
		public void HitTarget(IDamageable target)
		{
			if (target == null) return;
			target.ReceiveDamage(new DamageInfo(
				amount: damage,
				direction: transform.forward,
				position: transform.position,
				source: gameObject,
				isCritical: critical));
		}

		void OnTriggerEnter(Collider other)
		{
			if (!useTrigger) return;
			ProcessHit(other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject, other.ClosestPoint(transform.position));
		}

		void OnCollisionEnter(Collision collision)
		{
			if (useTrigger) return;
			var contact = collision.GetContact(0);
			ProcessHit(collision.collider.attachedRigidbody ? collision.collider.attachedRigidbody.gameObject : collision.collider.gameObject, contact.point);
		}

		void ProcessHit(GameObject other, Vector3 hitPoint)
		{
			if (!IsInLayerMask(other.layer, hitMask)) return;
			var damageable = other.GetComponentInParent<Damageable>();
			if (damageable == null) return;

			Vector3 dir = (damageable.transform.position - transform.position);
			var info = new DamageInfo(damage, dir, hitPoint, gameObject, critical);
			damageable.ReceiveDamage(info);
		}

		static bool IsInLayerMask(int layer, LayerMask mask)
		{
			int layerMask = 1 << layer;
			return (mask.value & layerMask) != 0;
		}
	}
}


