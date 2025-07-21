using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class PhysicalProjectile : BaseProjectile
{
    private IObjectPool<PhysicalProjectile> _projectilePool;
    private IObjectPool<ParticleSystem> _particlePool;
    private ParticleSystem _currentParticleSystem;
    private Vector3 _velocity;
    private float _traveledDistance;
    private int _pierceCount;
    private readonly int _maxDetectionCount = 50;
    
    [Header("Static Effect Settings")]
    [SerializeField] private bool isStaticEffect = false;
    [SerializeField] private float staticEffectDuration = 2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitEffectDuration = 2f;
    
    [Header("Advanced Physics")]
    [SerializeField] private bool useGravity = false;
    [SerializeField] private float gravityMultiplier = 1f;
    [SerializeField] private bool useRicochet = false;
    [SerializeField] private int maxRicochets = 2;
    [SerializeField] private float ricochetAngleThreshold = 45f;
    
    // 추가 상태 변수
    private int _ricochetCount = 0;
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        // AudioSource 컴포넌트 추가 (없으면)
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1f; // 3D 사운드
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSource.maxDistance = 30f;
        }
    }

    public void SetPools(IObjectPool<PhysicalProjectile> projectilePool, IObjectPool<ParticleSystem> particlePool)
    {
        _projectilePool = projectilePool;
        _particlePool = particlePool;
    }

    public override void Fire(Vector3 position, Vector3 direction, Transform firePoint)
    {
        // 초기화
        charactersDamaged.Clear();
        _isActive = true;
        _ricochetCount = 0;
        
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction);
        
        _velocity = direction * _config.projectileSpeed;
        _traveledDistance = 0f;
        _pierceCount = 0;

        // 파티클 시스템 설정
        SetupParticleSystem(position);

        // 발사 모드에 따른 처리
        if (_config.projectileSpeed <= 0f || isStaticEffect)
        {
            StartCoroutine(StaticEffectHandler());
        }
        else
        {
            StartCoroutine(ProjectileMovement());
        }
    }

    private void SetupParticleSystem(Vector3 position)
    {
        if (_particlePool != null && _config.particleSystemPrefab != null)
        {
            _currentParticleSystem = _particlePool.Get();
            if (_currentParticleSystem != null)
            {
                _currentParticleSystem.transform.position = position;
                _currentParticleSystem.transform.rotation = transform.rotation;
                _currentParticleSystem.Play();
            }
        }
    }

    private IEnumerator StaticEffectHandler()
    {
        // 즉시 범위 공격 실행
        StartCoroutine(PerformProgressiveAreaDamage());
        
        // 이펙트 지속 시간 계산
        float effectDuration = CalculateEffectDuration();
        
        // 이펙트 지속 시간만큼 대기
        yield return new WaitForSeconds(effectDuration);
        
        ReturnToPool();
    }

    private float CalculateEffectDuration()
    {
        float duration = staticEffectDuration;
        
        if (_currentParticleSystem != null)
        {
            var main = _currentParticleSystem.main;
            if (main.duration > 0)
            {
                duration = Mathf.Max(duration, main.duration + main.startLifetime.constantMax);
            }
        }
        
        return duration;
    }
    
    private IEnumerator PerformProgressiveAreaDamage()
    {
        float baseRadius = _config.areaOfEffect > 0 ? _config.areaOfEffect : 2f;
        float delayBetweenStages = 0.1f;
    
        // 단계별 범위 설정
        float[] stageRadii = {
            baseRadius * 0.3f,
            baseRadius * 0.6f,
            baseRadius
        };
    
        HashSet<Collider> alreadyHitTargets = new HashSet<Collider>();
        Collider[] colliderBuffer = new Collider[_maxDetectionCount];
    
        for (int stage = 0; stage < stageRadii.Length; stage++)
        {
            yield return StartCoroutine(ProcessStageAreaDamage(stageRadii[stage], colliderBuffer, alreadyHitTargets));
            
            if (stage < stageRadii.Length - 1)
            {
                yield return new WaitForSeconds(delayBetweenStages);
            }
        }
    }

    private IEnumerator ProcessStageAreaDamage(float radius, Collider[] buffer, HashSet<Collider> alreadyHit)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            radius,
            buffer,
            _config.collisionMask);
    
        for (int i = 0; i < hitCount; i++)
        {
            var target = buffer[i];
            if (target && !alreadyHit.Contains(target))
            {
                ProcessStaticHit(target, target.ClosestPoint(transform.position));
                alreadyHit.Add(target);
                
                // 약간의 딜레이로 자연스러운 연쇄 효과
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        // 버퍼 정리
        Array.Clear(buffer, 0, buffer.Length);
    }
    
    private void ProcessStaticHit(Collider hitCollider, Vector3 hitPoint)
    {
        CharacterManager targetCharacter = GetTargetCharacter(hitCollider);

        if (targetCharacter != null && !targetCharacter.isDead.Value)
        {
            if (ShouldDamageTarget(targetCharacter))
            {
                ProcessCharacterHit(targetCharacter, hitPoint);
            }
        }
        else
        {
            ProcessEnvironmentHit(hitCollider, hitPoint);
        }
    }

    private IEnumerator ProjectileMovement()
    {
        while (_isActive && _traveledDistance < _config.maxRange)
        {
            float deltaTime = Time.deltaTime;
            
            // 중력 적용
            if (useGravity)
            {
                _velocity += Vector3.down * (Physics.gravity.magnitude * gravityMultiplier * deltaTime);
            }
            
            Vector3 movement = _velocity * deltaTime;
            
            // 레이캐스트로 충돌 검사
            if (Physics.Raycast(transform.position, _velocity.normalized, out RaycastHit hit, 
                               movement.magnitude, _config.collisionMask))
            {
                // 충돌 지점으로 이동
                transform.position = hit.point;
                UpdateParticlePosition(hit.point);

                // 충돌 처리
                bool shouldContinue = HandleProjectileCollision(hit);
                if (!shouldContinue) break;
            }
            else
            {
                // 정상 이동
                transform.position += movement;
                _traveledDistance += movement.magnitude;
                UpdateParticlePosition(transform.position);
                
                // 방향 업데이트 (중력 적용 시)
                if (useGravity && _velocity.magnitude > 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation(_velocity.normalized);
                }
            }

            yield return null;
        }

        ReturnToPool();
    }

    private bool HandleProjectileCollision(RaycastHit hit)
    {
        HandleCollision(hit);

        // 도탄 처리
        if (useRicochet && _ricochetCount < maxRicochets)
        {
            float angle = Vector3.Angle(-_velocity.normalized, hit.normal);
            if (angle > ricochetAngleThreshold)
            {
                return HandleRicochet(hit);
            }
        }

        // 관통 처리
        if (_config.piercing && _pierceCount < _config.maxPierceCount)
        {
            return HandlePiercing();
        }

        return false; // 발사체 종료
    }

    private bool HandleRicochet(RaycastHit hit)
    {
        _ricochetCount++;
        
        // 도탄 방향 계산
        Vector3 ricochetDirection = Vector3.Reflect(_velocity.normalized, hit.normal);
        _velocity = ricochetDirection * _velocity.magnitude * 0.8f; // 속도 감소
        
        // 위치 약간 오프셋
        transform.position = hit.point + hit.normal * 0.1f;
        transform.rotation = Quaternion.LookRotation(ricochetDirection);
        
        // 도탄 이펙트
        CreateHitEffect(hit.point, hit.normal, true);
        
        return true; // 계속 진행
    }

    private bool HandlePiercing()
    {
        _pierceCount++;
        transform.position += _velocity.normalized * 0.1f;
        return true; // 계속 진행
    }

    private void UpdateParticlePosition(Vector3 position)
    {
        if (_currentParticleSystem != null)
        {
            _currentParticleSystem.transform.position = position;
        }
    }

    private CharacterManager GetTargetCharacter(Collider hitCollider)
    {
        CharacterManager target = hitCollider.GetComponent<CharacterManager>();
        if (target == null)
        {
            target = hitCollider.GetComponentInParent<CharacterManager>();
        }
        return target;
    }

    private void ProcessCharacterHit(CharacterManager targetCharacter, Vector3 hitPoint)
    {
        contactPoint = hitPoint;
        SetBlockingDotValues(targetCharacter);
        
        if (CheckForParried(targetCharacter)) return;
        
        bool isBlocked = CheckForBlock(targetCharacter);
        DamageTarget(targetCharacter, isBlocked);
        
        if (_config.applyStatusEffect && _config.statusEffectType != StatusEffectType.None)
        {
            ApplyStatusEffect(targetCharacter);
        }
        
        // 히트 이펙트 생성
        CreateHitEffect(hitPoint, (hitPoint - transform.position).normalized, false);
    }

    private void ProcessEnvironmentHit(Collider hitCollider, Vector3 hitPoint)
    {
        Debug.Log($"Projectile hit environment: {hitCollider.name} at {hitPoint}");
        CreateHitEffect(hitPoint, (hitPoint - transform.position).normalized, false);
    }

    private void CreateHitEffect(Vector3 hitPoint, Vector3 normal, bool isRicochet = false)
    {
        // 파티클 이펙트
        if (_particlePool != null && hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(normal));
            Destroy(effect, hitEffectDuration);
        }
        
        // 사운드 이펙트
        if (hitSound != null && _audioSource != null)
        {
            _audioSource.pitch = isRicochet ? 1.2f : 1f; // 도탄 시 높은 피치
            _audioSource.PlayOneShot(hitSound);
        }
    }

    public override void ReturnToPool()
    {
        _isActive = false;
        StopAllCoroutines();

        // 파티클 정리
        if (_currentParticleSystem != null)
        {
            _currentParticleSystem.Stop();
            _particlePool?.Release(_currentParticleSystem);
            _currentParticleSystem = null;
        }

        // 풀로 반환
        _projectilePool?.Release(this);
    }
    
    // 런타임 설정 메서드들
    public void SetStaticEffectMode(bool isStatic, float duration = 2f)
    {
        isStaticEffect = isStatic;
        staticEffectDuration = duration;
    }
    
    public void SetPhysicsMode(bool gravity, float gravityMult = 1f)
    {
        useGravity = gravity;
        gravityMultiplier = gravityMult;
    }
    
    public void SetRicochetMode(bool ricochet, int maxRico = 2, float angleThreshold = 45f)
    {
        useRicochet = ricochet;
        maxRicochets = maxRico;
        ricochetAngleThreshold = angleThreshold;
    }
}