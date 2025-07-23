using Unity.Mathematics;
using UnityEngine;

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
