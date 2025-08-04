using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Ch3_MusicBox : BaseInteractable
{
    // QTE는 2번까지 실패할 수 있고 3번째 실패했을 때는 울보가 큰 울음을 한다.
    // 3개를 다 성공했을 때 울음을 그친다.

    // 상호작용키
    // 완료했을 때 더이상 상호작용 되지 않게
    
    private bool playAble; // 오르골을 플레이 할 수 있는 영역에 있는지 확인
     [SerializeField] private GameObject arrowPrefab; //생성될 프리팹
    [SerializeField] private Transform arrowContainer; //프리팹 생성 위치
    [SerializeField] private Sprite leftArrow, rightArrow, upArrow, downArrow; //화살표 방향(생성된 프리팹 Sprite 바꿈)
    [SerializeField] private Image highlightImage; // 화살표 하이라이트 이미지
    [SerializeField] private Image timeBar;
    [SerializeField] private int arrowCount = 5; // 생성될 프리팹 개수
    [SerializeField] private float timeLimit = 6f; // 제한 시간
    public static int musicBox_FailCount; // 오르골 실패 카운트
    private List<KeyCode> targetSequence = new List<KeyCode>(); // 방향키 순서를 정해두고 사용자 키 입력과 맞는지 확인용
    KeyCode[] possibleKeys = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };
    //입력 가능한 키
    private int currentIndex = 0;
    private float timer;
    private bool isRunning;

    private bool isQTESuccess = false; //QTE를 성공했는지
    public CryEnemy linkedEnemy; // Inspector에서 지정 or 자동 연결

    [SerializeField] private GameObject QTEUI_MusicBox;

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
            }
        }

        if(!isRunning) return;
        
        timer -= Time.deltaTime;
        timeBar.fillAmount = timer / timeLimit;

        if (timer <= 0f)
        {
            Debug.Log("실패: 시간 초과"); // 울보에게 정보 전달
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
                    isQTESuccess = true;
                    StopQTE();
                }
            }
            else
            {
                Debug.Log("실패: 틀린 키");
                FailQTE();
                // currentIndex++;
                // UpdateHighlight();

            }
        }
    }

    
    void StartQTE()
    {
        QTEUI_MusicBox.SetActive(true);
        GenerateRandomSequence(arrowCount);
        isRunning = true;
    }

    void StopQTE()
    {
        QTEUI_MusicBox.SetActive(false);
        musicBox_FailCount ++;
        
    }
    void FailQTE()
    {
        // QTEUI_MusicBox.SetActive(false);
        musicBox_FailCount ++;
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

        protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //SetHighlight(true);
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
            playAble = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        if (other.CompareTag("Player"))
        {
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


