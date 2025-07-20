using System.Collections;
using TMPro;
using UnityEngine;

public class HUDGridBuildingCount : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI itemCount;

    private void OnEnable()
    {
        StartCoroutine(WaitForGridBuildingSystem());
    }
    
    private IEnumerator WaitForGridBuildingSystem()
    {
        // GridBuildingSystem.Instance가 null이 아닌지 확인
        while (GridBuildingSystem.Instance == null)
        {
            yield return null; // 매 프레임 기다림
        }

        // GridBuildingSystem.Instance가 설정되었을 때 이벤트 등록
        GridBuildingSystem.Instance.OnSelectedChanged += Instance_OnSelectedChanged;
        GridBuildingSystem.Instance.OnObjectPlaced += Instance_OnSelectedChanged;
    }

    private void OnDisable()
    {
        GridBuildingSystem.Instance.OnSelectedChanged -= Instance_OnSelectedChanged;
        GridBuildingSystem.Instance.OnObjectPlaced -= Instance_OnSelectedChanged;
    }

    private void Instance_OnSelectedChanged(object sender, System.EventArgs e) 
    {
        DisplayBuildingCount();
    }

    private void DisplayBuildingCount()
    {
        int count = GridBuildingSystem.Instance.GetPlacedObjectCount();
        if (count == 0)
        {
            itemCount.text = GridBuildingSystem.Instance.CanBuildObject() ? "O" : "X";
        }
        else
        {
            itemCount.text = GridBuildingSystem.Instance.GetPlacedObjectCount().ToString();
        }
    }
}
