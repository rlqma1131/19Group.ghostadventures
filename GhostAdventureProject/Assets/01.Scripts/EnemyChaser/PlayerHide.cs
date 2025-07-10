using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    private KeyCode hideKey = KeyCode.F;

    [SerializeField] private bool isHiding = false;
    [SerializeField] private bool canHide = false;

    private GameObject currentHideArea;
    private bool isInMirrorArea = false;  // 거울 영역인지 확인
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Rigidbody2D rb;

    // 거울에서만 키 차단용 변수
    public static bool mirrorKeysBlocked = false;

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
            if (Input.GetKeyDown(hideKey) || Input.GetKeyDown(KeyCode.E))
            {
                ShowPlayer();
            }
            return;
        }

        // 거울 영역이면서 키가 차단되어 있을 때만 차단
        if (isInMirrorArea && mirrorKeysBlocked)
        {
            if (Input.GetKeyDown(hideKey) || Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("거울 키 입력 차단됨! F, E 키 사용 불가");
            }
            return;
        }

        if ((Input.GetKeyDown(hideKey) || Input.GetKeyDown(KeyCode.E)) && canHide)
        {
            HidePlayer();
        }
    }

    private void HidePlayer()
    {
        Debug.Log($"숨기 시작: {currentHideArea.name}");

        isHiding = true;
        spriteRenderer.enabled = false;
        col.enabled = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
    }

    public void ShowPlayer()
    {
        isHiding = false;
        spriteRenderer.enabled = true;
        col.enabled = true;
        rb.isKinematic = false;

        Debug.Log("숨기 해제");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HideArea"))
        {
            Debug.Log($"숨는 곳 진입: {other.name}");
            currentHideArea = other.gameObject;

            // 이름에 "Mirror"가 들어가면 거울 영역으로 판단
            isInMirrorArea = other.name.Contains("Mirror");

            canHide = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("HideArea") && currentHideArea == other.gameObject)
        {
            Debug.Log($"숨는 곳 나감: {other.name}");
            currentHideArea = null;
            isInMirrorArea = false;
            canHide = false;
        }
    }
}