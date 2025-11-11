using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch4_ExitEndingdoor : MonoBehaviour
{
    public void MoveScene()
    {
        SceneManager.LoadScene("End_Exit");
        UIManager.Instance.PlayModeUI_CloseAll();

        if (GameManager.Instance != null)
        {

            var player = GameManager.Instance.Player;
            if (player != null)
            {
                Debug.Log("Player Destroyed");
                Destroy(player);
            }
        }


    }
}
