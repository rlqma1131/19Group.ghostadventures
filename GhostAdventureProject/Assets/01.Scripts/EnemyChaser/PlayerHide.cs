using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    private KeyCode hideKey = KeyCode.F;


    [SerializeField] private bool isHiding = false;
    [SerializeField] private bool canHide = false;

    private HideAreaID currentHideArea;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Rigidbody2D rb;

    public bool IsHiding => isHiding;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isHiding)
        {
            if (Input.GetKeyDown(hideKey))
            {
                ShowPlayer();
            }
            return;
        }

        if (Input.GetKeyDown(hideKey) && canHide)
        {
            HidePlayer();
        }
    }

    private void HidePlayer()
    {
        isHiding = true;
        spriteRenderer.enabled = false;
        col.enabled = false;
        rb.velocity = Vector2.zero;     // 멈추기
        rb.isKinematic = true;

        if (currentHideArea != null)
        {
            Ch1_HideAreaEvent.Instance.RegisterArea(currentHideArea.areaID);
        }
    }

    public void ShowPlayer()
    {
        isHiding = false;
        spriteRenderer.enabled = true;
        col.enabled = true;
        rb.isKinematic = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HideArea"))
        {
            canHide = true;
            currentHideArea = other.GetComponent<HideAreaID>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("HideArea"))
        {
            canHide = false;

            if (!isHiding)
            {
                currentHideArea = null;
            }
        }
    }
}
