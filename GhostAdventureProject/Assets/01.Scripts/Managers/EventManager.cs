using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private HashSet<string> completedEvents = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        { 
            Destroy(gameObject);
        }
    }

    public void MarkEventCompleted(string eventId)
    {
        if (completedEvents.Add(eventId))
        {
            SaveManager.SetEventCompleted(eventId, true);
            SaveManager.SaveGame();
        }
    }

    public bool IsEventCompleted(string eventId)
    {
        return completedEvents.Contains(eventId);
    }

    // SaveStateApplier에서 로드할 때 사용
    public void ApplyEventCompletedFromSave(string eventId, bool completed)
    {
        if (completed)
            completedEvents.Add(eventId);
    }
}

