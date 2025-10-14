using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ch4_ExitEndingdoor : MonoBehaviour
{
    public void moveScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("End_Exit");
    }



}
