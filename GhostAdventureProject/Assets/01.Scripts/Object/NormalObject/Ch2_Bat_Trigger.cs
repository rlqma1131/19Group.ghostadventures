using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;

public class Ch2_Bat_Trigger : MonoBehaviour
{
    Animator ani;
    private bool triggerOn = false;
    private float moveSpeed = 2f;
    private SpriteRenderer sr;

    void Start()
    {
        ani = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        
    }

    void Update()
    {
        if(triggerOn)
            MoveTo();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            ani.SetBool("Move", true);
            triggerOn = true;
            SoundTriggerer.TriggerSound(collision.transform.position);
            Debug.Log("박쥐에 부딪혔습니다. 사운드트리거 발생");
        }
    }

    void MoveTo()
    {   
        Vector3 targetPos = transform.position;
        float xPos = transform.position.x + UnityEngine.Random.Range(-1, 1);
        float yPos = transform.position.y + UnityEngine.Random.Range(-1, 1);
        targetPos.x = xPos;
        targetPos.y = yPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        float distance = Vector3.Distance(transform.position, targetPos);
        if(distance <= 0)
        {
            sr.flipX = true;
        }
        else
            sr.flipX = false;
    }
}
// public class CarMover : MonoBehaviour
// {
//     [SerializeField] private float moveDistance = 5f;
//     [SerializeField] private float moveDuration = 2f;
//     [SerializeField] private SpriteRenderer spriteRenderer;
//     [SerializeField] private float bounceHeight = 0.2f;     // 들썩 높이
//     [SerializeField] private float bounceDuration = 0.3f;   // 들썩 속도

//     private bool facingRight = true;

//     void Start()
//     {
//         if (spriteRenderer == null)
//             spriteRenderer = GetComponent<SpriteRenderer>();

//         // 위치 저장
//         Vector3 startPos = transform.position;
//         Vector3 endPos = new Vector3(startPos.x + moveDistance, startPos.y, startPos.z);

//         // 좌우 이동 트윈
//         transform.DOMoveX(endPos.x, moveDuration)
//             .SetLoops(-1, LoopType.Yoyo)
//             .SetEase(Ease.InOutSine)
//             .OnStepComplete(FlipX);

//         // 들썩거리는 트윈 (Y축으로 위아래 반복)
//         transform.DOMoveY(startPos.y + bounceHeight, bounceDuration)
//             .SetLoops(-1, LoopType.Yoyo)
//             .SetEase(Ease.InOutSine);

//         flipX = true
//         spriteRenderer.flipX = true;
//         facingRight = false;
//     }

//     void FlipX()
//     {
//         facingRight = !facingRight;
//         spriteRenderer.flipX = !facingRight;
//     }
// }