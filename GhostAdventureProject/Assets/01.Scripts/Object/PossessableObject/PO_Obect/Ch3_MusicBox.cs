using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_MusicBox : BaseInteractable
{
    [Header("오르골QTE UI")]
    [SerializeField] private GameObject arrowPrefab;        // 생성될 화살표 프리팹
    [SerializeField] private Transform arrowContainer;      // 화살표 프리팹 생성 위치
    [SerializeField] private Sprite leftArrow, rightArrow, upArrow, downArrow; // 화살표 방향(생성된 프리팹 Sprite를 바꿈)
    [SerializeField] private Image highlightImage;          // 하이라이트 이미지
    [SerializeField] private Transform highlightTransform;  // 하이라이트 오브젝트가 있을 위치
    [SerializeField] private int arrowCount = 5;            // 생성될 프리팹 개수
    [SerializeField] private float timeLimit = 5f;          // 제한 시간
    [SerializeField] private Image timeBar;                 // 제한시간 타임바
    [SerializeField] private TextMeshProUGUI text;          // 실패횟수 텍스트
    private float timer;                                    // 타이머
    private int currentIndex = 0;                           // 현재 입력해야 할 순서

    private bool isplayAble;                                // 오르골과 상호작용할 수 있는 영역에 있는지
    private bool isRunning;                                 // QTE가 실행되고 있는지
    private bool isPlay = false;                            // 오르골이 작동됐는지
    private bool ignoreFirstInput;                          // 첫입력 무시(상호작용키(E)가 입력되는것 방지)
    
    private enum Dir { Left, Right, Up, Down }
    private List<Dir> targetSequence = new List<Dir>();
    public CryEnemy linkedEnemy;                            // 연결된 울보 * CryEnemy에서 정보 넣어줌(인스펙터연결x)
    [SerializeField] private GameObject QTEUI_MusicBox;     // QTE UI Canvas
    private List<Image> arrowImages = new List<Image>();    // 화살표 이미지

    [Header("오르골 사운드")]
    [SerializeField] private AudioClip Play_Sound;          // 작동 사운드


     protected override void Start()
    {
        base.Start();

        isplayAble = false;
        isRunning = false;
        timer = timeLimit;
        QTEUI_MusicBox.SetActive(false);
    }

    private void Update()
    {
        if(!isplayAble || isPlay) return;
    
        if(Input.GetKeyDown(KeyCode.E)) StartQTE();

        if(!isRunning) return;
        
        timer -= Time.deltaTime;
        timeBar.fillAmount = timer / timeLimit;

        if (timer <= 0f)
        {
            FailPlayMusicBox();
            return;
        }

        if (Input.anyKeyDown)
        {
            if (ignoreFirstInput) // QTE 시작 직후 E키 입력 1번 무시
            {
                ignoreFirstInput = false;
                return;
            }
            if (currentIndex >= targetSequence.Count) return;

            var dir = targetSequence[currentIndex];
            if (IsDirectionPressed(dir))  SuccessArrow();
            else                          FailArrow();

            if (linkedEnemy.failCount >= 5) { StopQTE(); return; }

            if (currentIndex >= targetSequence.Count)
            {
                SuccessPlayMusicBox();
                isPlay = true;
            }
        }
    }

    // QTE 시작
    void StartQTE()
    {
        if(isRunning) return;
        currentIndex = 0;
        text.text = (linkedEnemy.failCount + " / 5").ToString();
        isRunning = true;
        player.PossessionSystem.CanMove = false;
        QTEUI_MusicBox.SetActive(true);
        GenerateRandomSequence(arrowCount);

        ignoreFirstInput = true;
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
        text.text = (linkedEnemy.failCount + " / 5").ToString();
        UpdateHighlight();
    }

    // 오르골 작동 성공
    void SuccessPlayMusicBox()
    {
         if (!isRunning) return;   // 중복 방지
        isRunning = false;
        SoundManager.Instance.PlaySFX(Play_Sound);
        StopQTE();
        linkedEnemy.OnMusicBoxSuccess();
    }

    // QTE 실패
    void FailPlayMusicBox()
    {
        if (!isRunning) return;   // 중복 방지
        isRunning = false;
        linkedEnemy.OnMusicBoxFail();
        text.text = (linkedEnemy.failCount + " / 5").ToString();
        StopQTE();
        // linkedEnemy.OnMusicBoxFail();
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
        player.PossessionSystem.CanMove = true;
    }

    // 랜덤으로 화살표 생성
    private void GenerateRandomSequence(int count)
    {
        targetSequence.Clear();
        for (int i = 0; i < count; i++)
        {
            var dir = (Dir)UnityEngine.Random.Range(0, 4); // 0~3
            targetSequence.Add(dir);

            GameObject arrow = Instantiate(arrowPrefab, arrowContainer);
            Image img = arrow.GetComponent<Image>();
            img.sprite = GetSpriteForDirection(dir);
            arrowImages.Add(img);
        }
        UpdateHighlight();
    }

    // 화살표 스프라이트 변경 
    private Sprite GetSpriteForDirection(Dir d)
    {
        switch (d)
        {
            case Dir.Left:  return leftArrow;
            case Dir.Right: return rightArrow;
            case Dir.Up:    return upArrow;
            case Dir.Down:  return downArrow;
        }
        return null;
    }
    // private KeyCode GetRandomArrowKey()
    // {
    //     return possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Length)];
    // }

    // 하이라이트 이미지가 화살표(arrow)를 따라다니도록 만듬
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
    
    // 작동되지 않은 오르골에 닿으면 isPlayAble = true, 아니면 false
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag("Player") && linkedEnemy.failCount < 5 && !isPlay)
            isplayAble = true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && linkedEnemy.failCount < 5 && isPlay)
            player.InteractSystem.RemoveInteractable(gameObject);
    }
    protected override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        if (other.CompareTag("Player")) isplayAble = false;
    }

    // 키를 맞게 입력했는지 확인
    bool IsDirectionPressed(Dir d)
    {
        switch (d)
        {
            case Dir.Left:  return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            case Dir.Right: return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
            case Dir.Up:    return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            case Dir.Down:  return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
        }
        return false;
    }



}


