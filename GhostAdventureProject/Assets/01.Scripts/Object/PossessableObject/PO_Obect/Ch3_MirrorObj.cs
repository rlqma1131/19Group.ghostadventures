using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MirrorObj : BasePossessable
{
    private bool isFound = false;
    private SpriteRenderer sr;
    
    [SerializeField] private bool isCorrect = false;
    [SerializeField] private GameObject q_Key;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        
        if (!isPossessed)
        {
            q_Key.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isFound)
        {
            isFound = true;
            q_Key.SetActive(false);

            if (isCorrect)
            {
                StartCoroutine(FadeAndDisable());
                GetComponentInParent<Ch3_MirrorRoomManager>()?.OnDifferenceFound(this);
            }
            else
            {
                GetComponentInParent<Ch3_MirrorRoomManager>()?.OnWrongObjectReleased();
            }

            Unpossess();
        }
        
        q_Key.SetActive(true);
    }

    private IEnumerator FadeAndDisable()
    {
        float time = 0f;
        Color originalColor = sr.color;
        while (time < 1f)
        {
            time += Time.deltaTime;
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - time);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
