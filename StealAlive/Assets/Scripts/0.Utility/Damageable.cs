using UnityEngine;
using UnityEngine.Events;

namespace bkTools
{
	/// <summary>
	/// 매우 단순한 피격 컴포넌트: 스탯의 Health를 사용하여 데미지 수신 + 이벤트
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("bkTools/Damage/Damageable")] 
	public class Damageable : MonoBehaviour, IDamageable
	{
		[Header("스탯 연결")]
		[SerializeField] private Stats stats; // Health를 보관하는 Stats 컴포넌트
		[SerializeField] private string healthStatId = "Health";    // Health 스탯 ID 이름
		[SerializeField] private bool createIfMissing = true;        // 없으면 생성할지 여부
		[SerializeField] private float defaultMax = 100f;            // 새로 생성 시 최대값
		[SerializeField] private float defaultStart = 100f;          // 새로 생성 시 시작값

		[Header("넉백 설정(선택)")]
		[SerializeField] private bool applyKnockback = false; // 데미지 시 넉백 적용 여부
		[SerializeField] private float knockbackForce = 5f;   // 넉백 힘
		[SerializeField] private ForceMode knockbackForceMode = ForceMode.Impulse; // 넉백 포스 모드

		[Header("이벤트")]
		public UnityEvent<float> OnHealthChanged = new();   // 현재 체력 값 브로드캐스트
		public UnityEvent<float> OnDamaged = new();         // 이번에 받은 데미지 양
		public UnityEvent OnDeath = new();                  // 사망 시

		private Stat cachedHealth;    // 연결된 Health 스탯 캐시

		public float MaxHealth => cachedHealth != null ? cachedHealth.Max : 0f;
		public float CurrentHealth => cachedHealth != null ? cachedHealth.Current : 0f;
		public bool IsDead => cachedHealth != null && cachedHealth.IsEmpty;

		void Awake()
		{
			if (stats == null) TryGetComponent(out stats);
			SetupHealthRef();
			if (cachedHealth != null)
			{
				OnHealthChanged.Invoke(cachedHealth.Current);
			}
		}

		void OnDestroy()
		{
			if (cachedHealth != null)
			{
				cachedHealth.OnValueChanged.RemoveListener(HandleStatValueChanged);
			}
		}

		/// <summary>
		/// 데미지 수신. amount는 양수일 때 체력이 감소합니다.
		/// </summary>
		public void ReceiveDamage(DamageInfo info)
		{
			if (cachedHealth == null)
			{
				SetupHealthRef();
				if (cachedHealth == null) return;
			}
			if (IsDead) return;

			float damage = Mathf.Max(0f, info.amount);
			if (info.isCritical) damage *= 1.5f; // 간단한 크리티컬 배율(필요시 조정)

			// Health 스탯에서 감소 처리
			cachedHealth.Add(-damage);
			OnDamaged.Invoke(damage);

			// 선택: 리지드바디가 있으면 넉백 적용
			if (applyKnockback && damage > 0f)
			{
				var rb = GetComponent<Rigidbody>();
				if (rb != null)
				{
					Vector3 dir = info.direction.sqrMagnitude > 0.0001f ? info.direction.normalized : -transform.forward;
					rb.AddForce(dir * knockbackForce, knockbackForceMode);
				}
			}

			if (IsDead)
			{
				OnDeath.Invoke();
			}
		}

		/// <summary>
		/// 회복(양수), 고정 피해(음수) 등으로 직접 체력을 변경합니다.
		/// </summary>
		public void AddHealth(float amount)
		{
			if (cachedHealth == null)
			{
				SetupHealthRef();
				if (cachedHealth == null) return;
			}
			cachedHealth.Add(amount);
		}

		// 내부 유틸: Health 스탯을 찾아 캐시하고 변경 이벤트를 포워딩합니다.
		void SetupHealthRef()
		{
			if (stats == null) return;
			if (!stats.TryGet(healthStatId, out var s))
			{
				if (createIfMissing)
				{
					s = stats.GetOrCreate(healthStatId, 0f, defaultMax, defaultStart);
				}
				else
				{
					return;
				}
			}
			if (cachedHealth == s) return;
			if (cachedHealth != null)
			{
				cachedHealth.OnValueChanged.RemoveListener(HandleStatValueChanged);
			}
			cachedHealth = s;
			cachedHealth.OnValueChanged.AddListener(HandleStatValueChanged);
		}

		void HandleStatValueChanged(float value)
		{
			OnHealthChanged.Invoke(value);
			if (IsDead) OnDeath.Invoke();
		}
	}
}


