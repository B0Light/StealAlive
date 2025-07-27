using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeAfterDelay : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [SerializeField] private float delayTime = 5.0f;
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private int targetSceneIndex = -1; // -1이면 씬 이름 사용
    
    [Header("Optional Settings")]
    [SerializeField] private bool useWorldSceneManager = true; // WorldSceneChangeManager 사용 여부
    [SerializeField] private bool showCountdown = false; // 카운트다운 표시 여부
    [SerializeField] private TextMeshProUGUI countdownText; // 카운트다운 텍스트 (옵션)
    
    [Header("Skip Settings")]
    [SerializeField] private bool allowSkip = true; // 스킵 허용 여부
    [SerializeField] private KeyCode skipKey = KeyCode.Space; // 스킵 키
    [SerializeField] private string skipButtonName = "Submit"; // Input Manager의 버튼 이름
    
    private bool isTransitioning = false;
    private Coroutine transitionCoroutine;
    
    void Start()
    {
        // 씬 전환 시작
        StartSceneTransition();
    }
    
    void Update()
    {
        // 스킵 입력 확인
        if (allowSkip && !isTransitioning)
        {
            bool skipPressed = Input.GetKeyDown(skipKey) || 
                              Input.GetButtonDown(skipButtonName) ||
                              Input.anyKeyDown; // 아무 키나 누르면 스킵
            
            if (skipPressed)
            {
                SkipToNextScene();
            }
        }
    }
    
    /// <summary>
    /// 씬 전환 시작
    /// </summary>
    public void StartSceneTransition()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        transitionCoroutine = StartCoroutine(DelayedSceneTransition());
    }
    
    /// <summary>
    /// 지연된 씬 전환 코루틴
    /// </summary>
    private IEnumerator DelayedSceneTransition()
    {
        float remainingTime = delayTime;
        
        while (remainingTime > 0)
        {
            // 카운트다운 표시
            if (showCountdown && countdownText != null)
            {
                countdownText.text = $"Next Scene in: {remainingTime:F1}s";
            }
            
            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }
        
        // 카운트다운 완료 후 씬 전환
        TransitionToTargetScene();
    }
    
    /// <summary>
    /// 즉시 다음 씬으로 스킵
    /// </summary>
    public void SkipToNextScene()
    {
        if (isTransitioning) return;
        
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        if (showCountdown && countdownText != null)
        {
            countdownText.text = "Loading...";
        }
        
        TransitionToTargetScene();
    }
    
    /// <summary>
    /// 대상 씬으로 전환
    /// </summary>
    private void TransitionToTargetScene()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        
        // 유효한 씬 설정 확인
        if (!IsValidSceneSettings())
        {
            Debug.LogError("SceneChangeAfterDelay: 유효한 씬이 설정되지 않았습니다!");
            return;
        }
        
        // WorldSceneChangeManager 사용 여부에 따라 분기
        if (useWorldSceneManager && WorldSceneChangeManager.Instance != null)
        {
            UseWorldSceneManager();
        }
        else
        {
            UseUnitySceneManager();
        }
    }
    
    /// <summary>
    /// WorldSceneChangeManager를 사용한 씬 전환
    /// </summary>
    private void UseWorldSceneManager()
    {
        try
        {
            if (targetSceneIndex >= 0)
            {
                WorldSceneChangeManager.Instance.LoadSceneAsync(targetSceneIndex);
            }
            else if (!string.IsNullOrEmpty(targetSceneName))
            {
                WorldSceneChangeManager.Instance.LoadSceneAsync(targetSceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WorldSceneChangeManager를 사용한 씬 전환 실패: {e.Message}");
            UseUnitySceneManager(); // 실패 시 기본 방법으로 대체
        }
    }
    
    /// <summary>
    /// Unity 기본 SceneManager를 사용한 씬 전환
    /// </summary>
    private void UseUnitySceneManager()
    {
        if (targetSceneIndex >= 0)
        {
            SceneManager.LoadScene(targetSceneIndex);
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
    
    /// <summary>
    /// 유효한 씬 설정인지 확인
    /// </summary>
    private bool IsValidSceneSettings()
    {
        return targetSceneIndex >= 0 || !string.IsNullOrEmpty(targetSceneName);
    }
    
    /// <summary>
    /// 지연 시간 변경
    /// </summary>
    public void SetDelayTime(float newDelayTime)
    {
        delayTime = Mathf.Max(0.1f, newDelayTime);
        
        // 이미 진행 중이면 재시작
        if (transitionCoroutine != null && !isTransitioning)
        {
            StartSceneTransition();
        }
    }
    
    /// <summary>
    /// 대상 씬 설정 (씬 이름)
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        targetSceneIndex = -1;
    }
    
    /// <summary>
    /// 대상 씬 설정 (씬 인덱스)
    /// </summary>
    public void SetTargetScene(int sceneIndex)
    {
        targetSceneIndex = sceneIndex;
        targetSceneName = "";
    }
    
    /// <summary>
    /// 전환 취소
    /// </summary>
    public void CancelTransition()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
        
        if (showCountdown && countdownText != null)
        {
            countdownText.text = "Transition Cancelled";
        }
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// 스킵 허용 여부 설정
    /// </summary>
    public void SetAllowSkip(bool allow)
    {
        allowSkip = allow;
    }
    
    void OnDestroy()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
    }
}