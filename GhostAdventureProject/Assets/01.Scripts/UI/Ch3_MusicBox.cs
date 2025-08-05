using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Ch3_MusicBox : BaseInteractable
{
    [Header("화살표 생성")]
     [SerializeField] private GameObject arrowPrefab; //생성될 프리팹
    [SerializeField] private Transform arrowContainer; //프리팹 생성 위치
    [SerializeField] private Sprite leftArrow, rightArrow, upArrow, downArrow; //화살표 방향(생성된 프리팹 Sprite 바꿈)
    [SerializeField] private Image highlightImage; // 하이라이트 이미지
    [SerializeField] private Transform highlightTransform; // 하이라이트 오브젝트가 있을 위치
    [SerializeField] private int arrowCount = 5; // 생성될 프리팹 개수
    [SerializeField] private float timeLimit = 6f; // 제한 시간
    [SerializeField] private Image timeBar; // 제한시간 타임바
    private float timer; // 타이머
    private int currentIndex = 0;

    [Header("오르골 QTE")]
    private bool playAble; // 오르골과 상호작용할 수 있는 영역에 있는지
    private bool isRunning; // QTE가 실행되고 있는지
    private bool isQTESuccess = false; // QTE를 성공했는지
    private List<KeyCode> targetSequence = new List<KeyCode>(); // 방향키 순서를 정해두고 사용자 키 입력과 맞는지 확인용
    KeyCode[] possibleKeys = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow }; //입력 가능한 키
    public CryEnemy linkedEnemy; // CryEnemy에서 정보 넣어줌(인스펙터연결x)
    [SerializeField] private GameObject QTEUI_MusicBox; // QTE UI Canvas


        //     if (linkedEnemy != null)
        // {
        //     linkedEnemy.OnMusicBoxFail();
        // }


    void Start()
    {
        playAble = false;
        isRunning = false;
        timer = timeLimit;
    }

    private void Update()
    {
        if(!playAble || isQTESuccess) return;

        if(!isRunning)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                StartQTE();
                isRunning = true;
            }
        }

        if(!isRunning) return;
        
        // isRunning일 때 실행
        timer -= Time.deltaTime;
        timeBar.fillAmount = timer / timeLimit;

        if (timer <= 0f)
        {
            Debug.Log("실패: 시간 초과"); // 울보에게 정보 전달
            FailQTE();
            return;
        }

        if (Input.anyKeyDown)
        {
            if(Input.GetKeyDown(KeyCode.E)) return; // 임시

            if (Input.GetKeyDown(targetSequence[currentIndex]))
            {
                Debug.Log("성공 입력");
                currentIndex++;
                UpdateHighlight();

                if (currentIndex >= targetSequence.Count)
                {
                    Debug.Log("QTE 성공!");
                    isQTESuccess = true;
                    SuccessQTE();
                }
            }
            else
            {
                Debug.Log("실패: 틀린 키");
                currentIndex++;
                Plus_FailCount();
                UpdateHighlight();
            }
        }
    }

    
    void StartQTE()
    {
        currentIndex = 0;
        GameObject highlight = GameObject.Find("ArrowHighlight");
        PossessionSystem.Instance.CanMove = false;
        QTEUI_MusicBox.SetActive(true);
        GenerateRandomSequence(arrowCount);
    }

    void StopQTE()
    {
        QTEUI_MusicBox.SetActive(false);
        // isRunning()
        // musicBox_FailCount ++;
        
    }
    void SuccessQTE()
    {
        QTEUI_MusicBox.SetActive(false);
        isRunning = false;
        PossessionSystem.Instance.CanMove = true;
        linkedEnemy.OnMusicBoxSuccess();
    }

    void Plus_FailCount()
    {
        if (linkedEnemy != null) linkedEnemy.OnMusicBoxFail();
    }
    void FailQTE()
    {
        QTEUI_MusicBox.SetActive(false);
        isRunning = false;
        if (linkedEnemy != null) linkedEnemy.OnMusicBoxFail();
        PossessionSystem.Instance.CanMove = true;
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
    private KeyCode GetRandomArrowKey()
    {
        return possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Length)];
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

    // 화살표(arrow)를 하이라이트 이미지가 따라다니도록 만듬
    void UpdateHighlight()
    {
        if (currentIndex >= targetSequence.Count)
            return;
            
        if (currentIndex < arrowContainer.childCount)
        {
            Transform target = arrowContainer.GetChild(currentIndex);
            highlightImage.transform.SetParent(target);
            highlightImage.transform.localPosition = Vector3.zero;
        }
        if(currentIndex >= arrowContainer.childCount)
        {
            highlightImage.transform.SetParent(highlightTransform);
            // currentIndex = 0;
        }
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isQTESuccess)
        {
            //SetHighlight(true);
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
            playAble = true;
        }
    }
    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isQTESuccess)
        {
            SetHighlight(false);
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
            playAble = false;
        }
    }

        // public void Interact()
    // {
    //     if (isQTESuccess) return;

    //     QTEManager.Instance.StartQTE(() => {
    //         isQTESuccess = true;
    //         linkedEnemy.OnMusicBoxSuccess(this);
    //     });
    // }


}


