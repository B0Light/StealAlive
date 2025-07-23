using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public enum MissionObjectType
{
    Door,           // 문 열기/닫기
    Light,          // 라이트 켜기/끄기
    Elevator,       // 엘리베이터 이동
    Platform,       // 플랫폼 이동
    Bridge,         // 다리 펼치기/접기
    Barrier         // 장벽 활성화/비활성화
}

public enum DoorAnimationType
{
    RotateY,        // Y축 회전 (일반 문)
    RotateX,        // X축 회전 (위아래 열리는 문)
    RotateZ,        // Z축 회전 (시계방향 회전)
    SlideHorizontal,// 좌우 슬라이딩
    SlideVertical   // 상하 슬라이딩
}

public class MissionObject : MonoBehaviour
{
    [Header("미션 오브젝트 설정")]
    [SerializeField] private MissionObjectType objectType = MissionObjectType.Door;
    [SerializeField] private bool isActivated = false;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("문 설정 (Door Type Only)")]
    [SerializeField] private DoorAnimationType doorAnimation = DoorAnimationType.RotateY;
    [SerializeField] private Transform leftDoor;              // 왼쪽 문 또는 단일 문
    [SerializeField] private Transform rightDoor;             // 오른쪽 문 (양쪽 문일 때만)
    [SerializeField] private float rotationAngle = 90.0f;     // 회전 각도
    [SerializeField] private float slideDistance = 2.0f;      // 슬라이딩 거리
    
    [Header("라이트 설정 (Light Type Only)")]
    [SerializeField] private Light[] lights;
    [SerializeField] private Material[] emissiveMaterials;    // 발광 재질들
    [SerializeField] private Color activatedColor = Color.white;
    [SerializeField] private Color deactivatedColor = Color.black;
    
    [Header("이동 오브젝트 설정 (Elevator/Platform/Bridge)")]
    [SerializeField] private Transform movableObject;
    [SerializeField] private Vector3 targetPosition;          // 목표 위치 (로컬)
    [SerializeField] private Vector3 targetRotation;          // 목표 회전
    
    [Header("이벤트")]
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onDeactivated;
    
    // 초기 상태 저장
    private Vector3 _leftDoorInitialPosition;
    private Vector3 _rightDoorInitialPosition;
    private Quaternion _leftDoorInitialRotation;
    private Quaternion _rightDoorInitialRotation;
    private Vector3 _movableObjectInitialPosition;
    private Quaternion _movableObjectInitialRotation;
    
    private bool _isAnimating = false;
    
    private void Awake()
    {
        SaveInitialStates();
    }
    
    private void SaveInitialStates()
    {
        // 문 초기 상태 저장
        if (leftDoor != null)
        {
            _leftDoorInitialPosition = leftDoor.localPosition;
            _leftDoorInitialRotation = leftDoor.localRotation;
        }
        
        if (rightDoor != null)
        {
            _rightDoorInitialPosition = rightDoor.localPosition;
            _rightDoorInitialRotation = rightDoor.localRotation;
        }
        
        // 이동 오브젝트 초기 상태 저장
        if (movableObject != null)
        {
            _movableObjectInitialPosition = movableObject.localPosition;
            _movableObjectInitialRotation = movableObject.localRotation;
        }
    }
    
    // 외부에서 호출할 수 있는 공개 메서드들
    public void ActivateObject()
    {
        if (_isAnimating || isActivated) return;
        
        isActivated = true;
        ExecuteObjectAction(true);
        onActivated?.Invoke();
    }
    
    public void DeactivateObject()
    {
        if (_isAnimating || !isActivated) return;
        
        isActivated = false;
        ExecuteObjectAction(false);
        onDeactivated?.Invoke();
    }
    
    public void ToggleObject()
    {
        if (isActivated)
            DeactivateObject();
        else
            ActivateObject();
    }
    
    private void ExecuteObjectAction(bool activate)
    {
        switch (objectType)
        {
            case MissionObjectType.Door:
                StartCoroutine(AnimateDoor(activate));
                break;
            case MissionObjectType.Light:
                ControlLights(activate);
                break;
            case MissionObjectType.Elevator:
            case MissionObjectType.Platform:
            case MissionObjectType.Bridge:
                StartCoroutine(AnimateMovableObject(activate));
                break;
            case MissionObjectType.Barrier:
                ControlBarrier(activate);
                break;
        }
    }
    
    #region Door Animation
    private IEnumerator AnimateDoor(bool opening)
    {
        _isAnimating = true;
        
        switch (doorAnimation)
        {
            case DoorAnimationType.RotateY:
                yield return StartCoroutine(AnimateDoorRotation(opening, Vector3.up));
                break;
            case DoorAnimationType.RotateX:
                yield return StartCoroutine(AnimateDoorRotation(opening, Vector3.right));
                break;
            case DoorAnimationType.RotateZ:
                yield return StartCoroutine(AnimateDoorRotation(opening, Vector3.forward));
                break;
            case DoorAnimationType.SlideHorizontal:
                yield return StartCoroutine(AnimateDoorSliding(opening, true));
                break;
            case DoorAnimationType.SlideVertical:
                yield return StartCoroutine(AnimateDoorSliding(opening, false));
                break;
        }
        
        _isAnimating = false;
    }
    
    private IEnumerator AnimateDoorRotation(bool opening, Vector3 rotationAxis)
    {
        Quaternion leftStartRotation = Quaternion.identity;
        Quaternion rightStartRotation = Quaternion.identity;
        Quaternion leftTargetRotation = Quaternion.identity;
        Quaternion rightTargetRotation = Quaternion.identity;
        
        // 왼쪽 문 설정
        if (leftDoor != null)
        {
            leftStartRotation = leftDoor.localRotation;
            Vector3 rotationVector = rotationAxis * -rotationAngle;
            leftTargetRotation = opening ? 
                _leftDoorInitialRotation * Quaternion.Euler(rotationVector) : 
                _leftDoorInitialRotation;
        }
        
        // 오른쪽 문 설정
        if (rightDoor != null)
        {
            rightStartRotation = rightDoor.localRotation;
            Vector3 rotationVector = rotationAxis * rotationAngle;
            rightTargetRotation = opening ? 
                _rightDoorInitialRotation * Quaternion.Euler(rotationVector) : 
                _rightDoorInitialRotation;
        }
        
        float elapsedTime = 0;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsedTime / animationDuration);
            
            if (leftDoor != null)
                leftDoor.localRotation = Quaternion.Lerp(leftStartRotation, leftTargetRotation, t);
            
            if (rightDoor != null)
                rightDoor.localRotation = Quaternion.Lerp(rightStartRotation, rightTargetRotation, t);
            
            yield return null;
        }
        
        // 최종 상태 설정
        if (leftDoor != null)
            leftDoor.localRotation = leftTargetRotation;
        
        if (rightDoor != null)
            rightDoor.localRotation = rightTargetRotation;
    }
    
    private IEnumerator AnimateDoorSliding(bool opening, bool horizontal)
    {
        Vector3 leftStartPos = Vector3.zero;
        Vector3 rightStartPos = Vector3.zero;
        Vector3 leftTargetPos = Vector3.zero;
        Vector3 rightTargetPos = Vector3.zero;
        
        if (leftDoor != null)
        {
            leftStartPos = leftDoor.localPosition;
            Vector3 direction = horizontal ? Vector3.left : Vector3.up;
            leftTargetPos = opening ? 
                _leftDoorInitialPosition + direction * slideDistance : 
                _leftDoorInitialPosition;
        }
        
        if (rightDoor != null)
        {
            rightStartPos = rightDoor.localPosition;
            Vector3 direction = horizontal ? Vector3.right : Vector3.down;
            rightTargetPos = opening ? 
                _rightDoorInitialPosition + direction * slideDistance : 
                _rightDoorInitialPosition;
        }
        
        float elapsedTime = 0;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsedTime / animationDuration);
            
            if (leftDoor != null)
                leftDoor.localPosition = Vector3.Lerp(leftStartPos, leftTargetPos, t);
            
            if (rightDoor != null)
                rightDoor.localPosition = Vector3.Lerp(rightStartPos, rightTargetPos, t);
            
            yield return null;
        }
        
        if (leftDoor != null)
            leftDoor.localPosition = leftTargetPos;
        
        if (rightDoor != null)
            rightDoor.localPosition = rightTargetPos;
    }
    #endregion
    
    #region Light Control
    private void ControlLights(bool activate)
    {
        // 라이트 켜기/끄기
        if (lights != null)
        {
            foreach (var light in lights)
            {
                if (light != null)
                    light.enabled = activate;
            }
        }
        
        // 발광 재질 제어
        if (emissiveMaterials != null)
        {
            Color targetColor = activate ? activatedColor : deactivatedColor;
            foreach (var material in emissiveMaterials)
            {
                if (material != null)
                {
                    material.SetColor("_EmissionColor", targetColor);
                    if (activate)
                        material.EnableKeyword("_EMISSION");
                    else
                        material.DisableKeyword("_EMISSION");
                }
            }
        }
    }
    #endregion
    
    #region Movable Object Animation
    private IEnumerator AnimateMovableObject(bool activate)
    {
        if (movableObject == null) yield break;
        
        _isAnimating = true;
        
        Vector3 startPosition = movableObject.localPosition;
        Quaternion startRotation = movableObject.localRotation;
        
        Vector3 endPosition = activate ? targetPosition : _movableObjectInitialPosition;
        Quaternion endRotation = activate ? 
            Quaternion.Euler(targetRotation) : 
            _movableObjectInitialRotation;
        
        float elapsedTime = 0;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsedTime / animationDuration);
            
            movableObject.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            movableObject.localRotation = Quaternion.Lerp(startRotation, endRotation, t);
            
            yield return null;
        }
        
        movableObject.localPosition = endPosition;
        movableObject.localRotation = endRotation;
        
        _isAnimating = false;
    }
    #endregion
    
    #region Barrier Control
    private void ControlBarrier(bool activate)
    {
        // 간단한 활성화/비활성화
        if (movableObject != null)
        {
            movableObject.gameObject.SetActive(activate);
        }
    }
    #endregion
    
    // 상태 확인 메서드들
    public bool IsActivated() => isActivated;
    public bool IsAnimating() => _isAnimating;
    public MissionObjectType GetObjectType() => objectType;
} 