using UnityEngine;

public class MoveBasePossessable : BasePossessable
{
    [SerializeField] private float moveSpeed = 3f;
    protected SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed || !PossessionSystem.Instance.canMove)
            return;

        Move();
    }

    void Move()
    {
        if (anim != null)
        {
            anim.SetBool("Move", true);
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        transform.position += move * moveSpeed * Time.deltaTime;

        // 좌우 Flip
        if (spriteRenderer != null && Mathf.Abs(h) > 0.01f)
        {
            spriteRenderer.flipX = h < 0f;
        }
    }
}
