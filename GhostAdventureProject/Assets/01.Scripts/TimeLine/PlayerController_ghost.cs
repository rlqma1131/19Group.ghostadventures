using UnityEngine;

public class PlayerController_ghost : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;


    public Animator animator { get; private set; }

    private void Start()
    {
        animator = GetComponent<Animator>();


    }

    void Update()
    {


        HandleMovement();

    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        transform.position += move * moveSpeed * Time.deltaTime;

        // 회전
        if (h > 0.01f)
        {
            transform.localScale = new Vector3(1, 1, 1);

        }
        else if (h < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);

        }

        bool isMoving = move.magnitude > 0.01f;
        animator.SetBool("Move", isMoving);
    }

}