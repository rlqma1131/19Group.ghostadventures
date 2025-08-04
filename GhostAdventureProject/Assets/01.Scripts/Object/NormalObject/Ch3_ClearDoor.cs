using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
public class Ch3_ClearDoor : MonoBehaviour
{
    public GameObject openDoor;
    [SerializeField] private PlayableDirector playableDirector; 
    bool isPlayerNear = false;
    bool isOpen = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && isOpen && isPlayerNear)
        {
            // 컷씬 연결해주세여 혁준상
            //openDoor.SetActive(true);
            //gameObject.SetActive(false);
            playableDirector.Play();


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
