using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShelterManager : MonoBehaviour
{
    [SerializeField] private GameObject parkGoerPrefab;
    [SerializeField] private Transform parkEntrance;
    [SerializeField] private Button openShelterButton;

    public int visitorCapacity = 30;
    public float visitCycle = 3f;
    private object _lockObject = new object();
    private List<ShelterVisitor> _shelterVisitorList = new List<ShelterVisitor>();
    
    public void RemoveVisitor(ShelterVisitor visitor)
    {
        lock (_lockObject)
        {
            _shelterVisitorList.Remove(visitor);
        }
            
    }

    private Variable<bool> _isBusinessOpenToday = new Variable<bool>(false);

    private void OnEnable()
    {
        WorldTimeManager.Instance.day.OnValueChanged += ResetDailyOperation;
        _isBusinessOpenToday.OnValueChanged += SetOpenShelterButton;
    }
    
    private void OnDisable()
    {
        _isBusinessOpenToday.OnValueChanged -= SetOpenShelterButton;
    }

    private void ResetDailyOperation(int newValue)
    {
        _isBusinessOpenToday.Value = false;
    }

    public void OpenShelter()
    {
        if(_isBusinessOpenToday.Value) return;
        
        _isBusinessOpenToday.Value = true;
        StartCoroutine(VisitEnumerator());
    }

    private IEnumerator VisitEnumerator()
    {
        float duration = visitorCapacity * visitCycle;
        float elapsedTime = 0f;
    
        while (elapsedTime < duration)
        {
            yield return new WaitForSecondsRealtime(visitCycle);
            HandleVisitorEntry();
            elapsedTime += visitCycle;
        }
    }
    
    [ContextMenu("VisitorEntry")]
    public void HandleVisitorEntry()
    {
        GameObject visitor = Instantiate(parkGoerPrefab, parkEntrance);
        ShelterVisitor shelterVisitor = visitor.GetComponent<ShelterVisitor>();
        if(shelterVisitor) shelterVisitor.SpawnVisitor(this);
        lock (_lockObject)
        {
            _shelterVisitorList.Add(shelterVisitor);
        }
    }

    private void SetOpenShelterButton(bool value)
    {
        openShelterButton.interactable = !value;
    }

    public bool IsVisitorInShelter()
    {
        return _shelterVisitorList.Count > 0;
    }

    public bool CheckVisit()
    {
        return _isBusinessOpenToday.Value;
    }
} 
