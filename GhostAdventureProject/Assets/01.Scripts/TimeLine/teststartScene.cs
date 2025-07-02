using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class teststartScene : MonoBehaviour
{
    // Start is called before the first frame update
   public void introScenestart()
    {


        SceneManager.LoadScene("IntroScene_Real");
    }
}
