using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_MusicBox : BaseInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject arrowPrefab; //생성될 프리팹
    [SerializeField] private Transform arrowContainer; //프리팹 생성 위치
    [SerializeField] private Sprite leftArrow, rightArrow, upArrow, downArrow; //화살표 방향(생성된 프리팹 Sprite 바꿈)
    [SerializeField] private Image highlightImage; // 하이라이트 이미지
    [SerializeField] private Transform highlightTransform; // 하이라이트 오브젝트가 있을 위치
    [SerializeField] private int arrowCount = 5; // 생성될 프리팹 개수
    [SerializeField] private float timeLimit = 5f; // 제한 시간
    [SerializeField] private Image timeBar; // 제한시간 타임바
    private float timer; // 타이머
    private int currentIndex = 0;
    [SerializeField] private TextMeshProUGUI text;

    [Header("오르골 QTE")]
    private bool playAble; // 오르골과 상호작용할 수 있는 영역에 있는지
    private bool isRunning; // QTE가 실행되고 있는지
    private bool isQTESuccess = false; // QTE를 성공했는지
    private List<KeyCode> targetSequence = new List<KeyCode>(); // 방향키 순서를 정해두고 사용자 키 입력과 맞는지 확인용
    KeyCode[] possibleKeys = { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S }; //입력 가능한 키
    public CryEnemy linkedEnemy; // CryEnemy에서 정보 넣어줌(인스펙터연결x)
    [SerializeField] private GameObject QTEUI_MusicBox; // QTE UI Canvas
    private List<Image> arrowImages = new List<Image>();

    [Header("오르골 사운드")]
    [SerializeField] private AudioClip successArrow_Sound;
    [SerializeField] private AudioClip successQTE_Sound;


    void Start()
    {
        playAble = false;
        isRunning = false;
        timer = timeLimit;
        QTEUI_MusicBox.SetActive(false);
    }

    private void Update()
    {
        if(!playAble || isQTESuccess) return;

        if(!isRunning)
        {
            if(Input.GetKeyDown(KeyCode.E)) StartQTE();
        }

        if(!isRunning) return;
        
        timer -= Time.deltaTime;
        timeBar.fillAmount = timer / timeLimit;

        if (timer <= 0f)
        {
            FailQTE();
            return;
        }

        if(linkedEnemy.failCount >= 3)
        {
            StopQTE();
            return;
        }

        if (Input.anyKeyDown)
        {
            if(Input.GetKeyDown(KeyCode.E)) return; // 임시
            if(currentIndex >= targetSequence.Count) return;
            if (Input.GetKeyDown(targetSequence[currentIndex]))
            {
                SuccessArrow();
            }
            else
            {
                FailArrow();
            }

            if(linkedEnemy.failCount >= 3)
            {
                StopQTE();
                return;
            }

            if (currentIndex >= targetSequence.Count) SuccessQTE();
        }
    }

    // QTE 시작
    void StartQTE()
    {
        currentIndex = 0;
        isRunning = true;
        PossessionSystem.Instance.CanMove = false;
        QTEUI_MusicBox.SetActive(true);
        GenerateRandomSequence(arrowCount);
    }

    // 화살표 입력 성공
    void SuccessArrow()
    {   
        arrowImages[currentIndex].color = Color.green;
        currentIndex++;
        UpdateHighlight();
    }

    // 화살표 입력 실패
    void FailArrow()
    {
        arrowImages[currentIndex].color = Color.red;
        currentIndex++;
        linkedEnemy.OnMusicBoxFail();
        text.text = (linkedEnemy.failCount + " / 3").ToString();
        UpdateHighlight();
    }

    // QTE 성공
    void SuccessQTE()
    {
        SoundManager.Instance.PlaySFX(successArrow_Sound);
        StopQTE();
        linkedEnemy.OnMusicBoxSuccess();
    }

    // QTE 실패
    void FailQTE()
    {
        linkedEnemy.OnMusicBoxFail();
        text.text = (linkedEnemy.failCount + " / 3").ToString();
        StopQTE();
        linkedEnemy.OnMusicBoxFail();
    }

    // QTE 멈춤
    void StopQTE()
    {   
        highlightImage.transform.SetParent(highlightTransform);
        StartCoroutine(DelayedQTEStop());
    }
    IEnumerator DelayedQTEStop()
    {
        yield return new WaitForSeconds(0.2f); // 짧게 보여주기
        foreach (Transform child in arrowContainer)
        {
            Destroy(child.gameObject);
        }
        arrowImages.Clear(); // 리스트도 초기화
        QTEUI_MusicBox.SetActive(false);
        isRunning = false;
        PossessionSystem.Instance.CanMove = true;
    }

    // 랜덤으로 화살표 생성
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
            arrowImages.Add(img); 
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
            case KeyCode.A: return leftArrow;
            case KeyCode.D: return rightArrow;
            case KeyCode.W: return upArrow;
            case KeyCode.S: return downArrow;
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
    }
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && linkedEnemy.failCount < 3)
        {
            //SetHighlight(true);
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
            playAble = true;
        }
    }
    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetHighlight(false);
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
            playAble = false;
        }
    }

}


