using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ExpandingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("버튼 설정")]
    [SerializeField] private RectTransform buttonRectTransform;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private string normalText = "";
    [SerializeField] private string expandedText = "Expanded Text";
    
    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float expandedWidthPadding = 20f; // 확장 시 추가할 패딩
    [SerializeField] private bool useMinimumExpandedWidth = true; // 최소 확장 크기 사용 여부
    [SerializeField] private float minimumExpandedWidthRatio = 1.5f; // 기본 크기의 몇 배까지 최소로 확장할지
    
    private float _normalWidth;
    private float _targetWidth;
    private Coroutine _animationCoroutine;
    
    private void Awake()
    {
        // 컴포넌트가 할당되지 않았다면 자동으로 찾기
        if (buttonRectTransform == null)
            buttonRectTransform = GetComponent<RectTransform>();
            
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            
        // 현재 버튼의 크기를 기본 크기로 저장
        _normalWidth = buttonRectTransform.sizeDelta.x;
        
        // 초기 상태 설정
        buttonText.text = normalText;
        buttonRectTransform.sizeDelta = new Vector2(_normalWidth, buttonRectTransform.sizeDelta.y);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 확장된 텍스트로 변경
        buttonText.text = expandedText;
        
        // 텍스트 크기 계산을 위해 Canvas 업데이트 강제 실행
        Canvas.ForceUpdateCanvases();
        
        // 텍스트 크기에 맞게 너비 계산
        float textWidth = buttonText.preferredWidth;
        _targetWidth = (textWidth * 1.3f) + expandedWidthPadding;
        
        // 최소 확장 크기 적용 (옵션)
        if (useMinimumExpandedWidth)
        {
            float minimumWidth = _normalWidth * minimumExpandedWidthRatio;
            _targetWidth = Mathf.Max(_targetWidth, minimumWidth);
        }
        
        // 이전 애니메이션 중단
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        
        // 새 애니메이션 시작
        _animationCoroutine = StartCoroutine(AnimateButton(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 기본 텍스트로 변경
        buttonText.text = normalText;
        
        // 이전 애니메이션 중단
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        
        // 새 애니메이션 시작
        _targetWidth = _normalWidth;
        _animationCoroutine = StartCoroutine(AnimateButton(false));
    }

    private IEnumerator AnimateButton(bool expand)
    {
        float startWidth = buttonRectTransform.sizeDelta.x;
        float endWidth = expand ? _targetWidth : _normalWidth;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            t = EaseInOutQuad(t);
            
            float currentWidth = Mathf.Lerp(startWidth, endWidth, t);
            buttonRectTransform.sizeDelta = new Vector2(currentWidth, buttonRectTransform.sizeDelta.y);
            
            yield return null; // 다음 프레임까지 대기
        }
        
        // 정확한 최종 크기 설정
        buttonRectTransform.sizeDelta = new Vector2(endWidth, buttonRectTransform.sizeDelta.y);
    }
    
    // 부드러운 애니메이션을 위한 이징 함수
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
    
    // 런타임에서 기본 크기를 재설정하고 싶을 때 사용
    public void SetNormalWidth(float newNormalWidth)
    {
        _normalWidth = newNormalWidth;
        if (buttonText.text == normalText)
        {
            buttonRectTransform.sizeDelta = new Vector2(_normalWidth, buttonRectTransform.sizeDelta.y);
        }
    }
    
    // 현재 기본 크기 반환
    public float GetNormalWidth()
    {
        return _normalWidth;
    }
}