using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour, IDamageable
{
    [Header("Status")]
    public Variable<bool> isDead = new Variable<bool>(false);
    public Variable<bool> isGroggy = new Variable<bool>(false);

    public bool IsAlive => isDead.Value;

    // 컴포넌트 레퍼런스
    [HideInInspector] public Animator animator;
    [HideInInspector] public Collider characterCollider;
    [HideInInspector] public CharacterVariableManager characterVariableManager;
    [HideInInspector] public CharacterEffectsManager characterEffectsManager;
    [HideInInspector] public CharacterAnimatorManager characterAnimatorManager;
    [HideInInspector] public CharacterCombatManager characterCombatManager;
    [HideInInspector] public CharacterStatsManager characterStatsManager;
    [HideInInspector] public CharacterSoundFXManager characterSoundFXManager;
    [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
    [HideInInspector] public CharacterEquipmentMangaer characterEquipmentManager;
    [HideInInspector] public CharacterUIManager characterUIManager;
    [HideInInspector] public MeshTrail meshTrail;

    private Rigidbody _rigidbody;
	
	// bkTools 브릿지용 필드
	private bkTools.Stats _bkStats;
	private bkTools.Damageable _bkDamageable;
	private bkTools.Stat _bkHealthStat;
	private bool _syncingFromBkStat;
	private bool _syncingFromVariable;
    
    [Header(("CharacterGroup"))] 
    public CharacterGroup characterGroup;

    [HideInInspector] public bool isPerformingAction = false;
     
    #region Unity Lifecycle Methods
    
    protected virtual void Awake()
    {
        Debug.Log("Character Manager Awake");
        InitializeComponents();
    }

    protected virtual void Start()
    {
        IgnoreMyOwnColliders();
    }
    
    protected virtual void Update()
    {
        if(isDead.Value) return; 
        UpdateCharacterState();
    }
    
    protected virtual void OnEnable()
    {
        characterVariableManager.InitVariable();
        SubscribeToEvents();

		// bkTools Health 스탯과 기존 체력 변수 동기화 세팅
		SetupBkToolsBridge();
    }
    
    protected virtual void OnDisable()
    {
        UnsubscribeFromEvents();

		// bkTools 이벤트 해제
		if (_bkHealthStat != null)
		{
			_bkHealthStat.OnValueChanged.RemoveListener(HandleBkHealthChanged);
		}
		if (characterVariableManager != null && characterVariableManager.health != null)
		{
			characterVariableManager.health.OnValueChanged -= HandleVariableHealthChanged;
			characterVariableManager.health.OnSetMaxValue -= HandleVariableMaxHealthChanged;
		}
		if (_bkDamageable != null)
		{
			_bkDamageable.OnDeath.RemoveListener(HandleBkDeath);
		}
    }
    
    #endregion

	#region bkTools Bridge (0.Utility)
	private void SetupBkToolsBridge()
	{
		if (_bkStats == null) return;

		// Health 스탯 생성/획득 및 현재 값/최대값 동기화
		int curMax = characterVariableManager?.health?.MaxValue ?? 100;
		int curVal = characterVariableManager?.health?.Value ?? curMax;
		_bkHealthStat = _bkStats.GetOrCreate("Health", 0f, curMax, curVal);

		// 이벤트 연결(양방향 동기화)
		_bkHealthStat.OnValueChanged.RemoveListener(HandleBkHealthChanged);
		_bkHealthStat.OnValueChanged.AddListener(HandleBkHealthChanged);

		if (characterVariableManager != null && characterVariableManager.health != null)
		{
			characterVariableManager.health.OnValueChanged -= HandleVariableHealthChanged;
			characterVariableManager.health.OnValueChanged += HandleVariableHealthChanged;
			characterVariableManager.health.OnSetMaxValue -= HandleVariableMaxHealthChanged;
			characterVariableManager.health.OnSetMaxValue += HandleVariableMaxHealthChanged;
		}

		// Damageable 이벤트 연결: 사망/피해
		if (_bkDamageable != null)
		{
			_bkDamageable.OnDeath.RemoveListener(HandleBkDeath);
			_bkDamageable.OnDeath.AddListener(HandleBkDeath);
			_bkDamageable.OnDamaged.RemoveAllListeners();
			_bkDamageable.OnDamaged.AddListener(HandleBkDamaged);
		}
	}

	private void HandleBkHealthChanged(float newValue)
	{
		if (characterVariableManager?.health == null) return;
		if (_syncingFromVariable) return;
		_syncingFromBkStat = true;
		characterVariableManager.health.Value = Mathf.RoundToInt(newValue);
		_syncingFromBkStat = false;
	}

	private void HandleVariableHealthChanged(int newValue)
	{
		if (_bkHealthStat == null) return;
		if (_syncingFromBkStat) return;
		_syncingFromVariable = true;
		_bkHealthStat.Set(newValue);
		_syncingFromVariable = false;
	}

	private void HandleVariableMaxHealthChanged(int newMax)
	{
		if (_bkHealthStat == null) return;
		_bkHealthStat.SetBounds(0f, newMax, true);
	}

	private void HandleBkDeath()
	{
		if (!isDead.Value)
		{
			characterVariableManager.DeathProcess(0);
		}
	}

	private void HandleBkDamaged(float amount)
	{
		// 간단한 피격 연출: VFX/SFX만 트리거
		characterEffectsManager?.PlayBloodSplatterVFX(transform.position);
		characterSoundFXManager?.PlayDamageGruntSoundFX();
	}
	#endregion
    #region Initialization
    
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("NO ANIMATOR");
        }

        characterVariableManager = GetComponent<CharacterVariableManager>();
        characterEffectsManager = GetComponent<CharacterEffectsManager>();
        characterAnimatorManager = GetComponent<CharacterAnimatorManager>();
        characterCombatManager = GetComponent<CharacterCombatManager>();
        characterStatsManager = GetComponent<CharacterStatsManager>();
        characterSoundFXManager = GetComponent<CharacterSoundFXManager>();
        characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
        characterEquipmentManager = GetComponent<CharacterEquipmentMangaer>();
        characterUIManager = GetComponent<CharacterUIManager>();
        
        characterCollider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        meshTrail = GetComponent<MeshTrail>();
        
        characterAnimatorManager?.Spawn();

		// 0.Utility(bkTools) 연결: Stats/Damageable 자동 부착
		_bkStats = GetComponent<bkTools.Stats>();
		if (_bkStats == null) _bkStats = gameObject.AddComponent<bkTools.Stats>();
		_bkDamageable = GetComponent<bkTools.Damageable>();
		if (_bkDamageable == null) _bkDamageable = gameObject.AddComponent<bkTools.Damageable>();
    }
    
    private void SubscribeToEvents()
    {
        isDead.OnValueChanged += OnCharacterDeath;
        characterVariableManager.health.OnDepleted += characterVariableManager.DeathProcess;
        characterVariableManager.groggy.OnDepleted += characterVariableManager.OnGroggy;
        characterVariableManager.isBlock.OnValueChanged += characterVariableManager.OnBlocking;
        characterVariableManager.isCharging.OnValueChanged += characterVariableManager.OnIsChargingAttack;
        characterVariableManager.isTrailActive.OnValueChanged += meshTrail.OnTrailActiveChanged;
    }
    
    private void UnsubscribeFromEvents()
    {
        isDead.OnValueChanged -= OnCharacterDeath;
        characterVariableManager.health.OnDepleted -= characterVariableManager.DeathProcess;
        characterVariableManager.groggy.OnDepleted -= characterVariableManager.OnGroggy;
        characterVariableManager.isBlock.OnValueChanged -= characterVariableManager.OnBlocking;
        characterVariableManager.isCharging.OnValueChanged -= characterVariableManager.OnIsChargingAttack;
        characterVariableManager.isTrailActive.OnValueChanged -= meshTrail.OnTrailActiveChanged;
    }
    
    #endregion

    #region Character State Management
    
    private void UpdateCharacterState()
    {
        ActivateTrail();
        characterVariableManager.position.Value = transform.position;
        characterVariableManager.rotation.Value = transform.rotation;
    }
    
    private void OnCharacterDeath(bool value)
    {
        if (isDead.Value)
        {
            StartCoroutine(ProcessDeathEvent());
        }
    }
    
    protected virtual IEnumerator ProcessDeathEvent()
    {
        yield return new WaitForFixedUpdate();
        characterVariableManager.health.Value = 0;
        characterCombatManager.SetTarget(null);
        characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true, canMove: false, canRotate: false);
    }
    
    protected virtual void ActivateTrail()
    {
        characterVariableManager.isTrailActive.Value =
            _rigidbody.linearVelocity.magnitude >= characterVariableManager.CLVM.sprintSpeed;
    }
    
    #endregion

    #region Combat and Damage
    
    public void TakeDamage(DamageData data)
    {
		if (characterVariableManager.isInvulnerable.Value || isDead.Value) return;
		// bkTools 기반 단순 데미지 처리로 라우팅
		if (_bkDamageable == null) _bkDamageable = GetComponent<bkTools.Damageable>();
		if (_bkDamageable == null) return;

		float amount = Mathf.Max(0f, data.physicalDamage + data.magicalDamage + data.extraDamage);
		Vector3 dir = (data.attacker ? (transform.position - data.attacker.transform.position) : -transform.forward);
		var info = new bkTools.DamageInfo(
			amount,
			dir,
			data.contactPoint,
			data.attacker ? data.attacker.gameObject : gameObject,
			false
		);
		_bkDamageable.ReceiveDamage(info);
    }

    #endregion

    #region Collision Management
    
    private void IgnoreMyOwnColliders()
    {
        Collider[] damageableCharacterColliders = GetComponentsInChildren<Collider>();
        List<Collider> ignoreColliders = new List<Collider>(damageableCharacterColliders);
        ignoreColliders.Add(characterCollider);

        foreach (var mainCollider in ignoreColliders)
        {
            foreach (var otherCollider in ignoreColliders)
            {
                Physics.IgnoreCollision(mainCollider, otherCollider, true);
            }
        }
    }
    
    #endregion
}