using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
public class Ch3_ClearDoor : MonoBehaviour
{
    public GameObject openDoor;
    public GameObject closeDoor; // 닫힌 문 오브젝트
    [SerializeField] private PlayableDirector playableDirector; 
    [SerializeField] private bool isPlayerNear = false;
    [SerializeField] private bool isOpen = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && isOpen && isPlayerNear)
        {
            // 컷씬 연결해주세여 혁준상
            //openDoor.SetActive(true);
            closeDoor.SetActive(false);
            openDoor.SetActive(true); // 문 오브젝트 비활성화
            GameManager.Instance.Player.SetActive(false); // 플레이어 비활성화
            playableDirector.Play();
            UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기


        }
    }

    private void Start()
    {
        playableDirector.stopped += OnTimelineEnd;
    }
    void OnTimelineEnd(PlayableDirector director)
    {
            SceneManager.LoadScene("Ch03_To_Ch04");
    }
    public void OpenDoor()
    {
        isOpen = true;
        openDoor.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
