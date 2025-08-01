using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTEUI_MusicBox : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowContainer;
    [SerializeField] private Sprite leftArrow, rightArrow, upArrow, downArrow;
    [SerializeField] private Image highlightImage;
    [SerializeField] private int arrowCount = 5; // 유니티에서 조절 가능
    [SerializeField] private float timeLimit = 6f; // 제한 시간
    private List<KeyCode> targetSequence = new List<KeyCode>(); // 방향키 순서를 정해두고 사용자 키 입력과 맞는지 확인용
    KeyCode[] possibleKeys = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };

    private int currentIndex = 0;
    private float timer;
    private bool isRunning;

    void Start()
    {
        // gameObject.SetActive(false);
        isRunning = false;
        // GenerateRandomSequence(arrowCount);
        timer = timeLimit;    
        StartQTE();
    }

    public void StartQTE()
    {
        GenerateRandomSequence(arrowCount);
        isRunning = true;
    }

    void Update()
    {
        if(!isRunning) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Debug.Log("실패: 시간 초과");
            return;
        }

        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(targetSequence[currentIndex]))
            {
                Debug.Log("성공 입력");
                currentIndex++;
                UpdateHighlight();

                if (currentIndex >= targetSequence.Count)
                {
                    Debug.Log("QTE 성공!");
                }
            }
            else
            {
                Debug.Log("실패: 틀린 키");
            }
        }
    }

    private KeyCode GetRandomArrowKey()
    {
        return possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Length)];
    }

    private void GenerateRandomSequence(int count)
    {
        targetSequence.Clear();

        for (int i = 0; i < count; i++)
        {
            KeyCode randomKey = GetRandomArrowKey();
            targetSequence.Add(randomKey);

            GameObject arrow = Instantiate(arrowPrefab, arrowContainer);
            Image img = arrow.GetComponent<Image>();
            img.sprite = GetSpriteForKey(randomKey);
        }

        UpdateHighlight();
    }

    Sprite GetSpriteForKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftArrow: return leftArrow;
            case KeyCode.RightArrow: return rightArrow;
            case KeyCode.UpArrow: return upArrow;
            case KeyCode.DownArrow: return downArrow;
            default: return null;
        }
    }

    void UpdateHighlight()
    {
        if (currentIndex < arrowContainer.childCount)
        {
            Transform target = arrowContainer.GetChild(currentIndex);
            highlightImage.transform.SetParent(target);
            highlightImage.transform.localPosition = Vector3.zero;
        }
    }
}
