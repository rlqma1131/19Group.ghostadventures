using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Ch2_SewerLightingTrigger : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;

    private Color originalGlobalColor;
    private bool hasStoredOriginal = false;
    private Light2D playerLight;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == GameManager.Instance.Player)
        {
            // 1. 플레이어 Light2D 자동 탐색
            if (playerLight == null)
            {
                playerLight = other.GetComponentInChildren<Light2D>(includeInactive: true);
            }

            // 2. Global Light 어둡게
            if (globalLight != null)
            {
                if (!hasStoredOriginal)
                {
                    originalGlobalColor = globalLight.color;
                    hasStoredOriginal = true;
                }

                globalLight.color = Color.black;
                globalLight.intensity = 1f;
            }

            // 3. Player Light 켜기
            if (playerLight != null)
            {
                playerLight.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == GameManager.Instance.Player && !other.gameObject.activeInHierarchy)
            return;
        
        if (other.gameObject == GameManager.Instance.Player)
        {
            // Global Light 복원
            if (globalLight != null && hasStoredOriginal)
            {
                globalLight.color = originalGlobalColor;
            }

            // Player Light 끄기
            if (playerLight != null)
            {
                playerLight.enabled = false;
            }
        }
    }
    
    public void ForceRestoreLighting()
    {
        if (globalLight != null && hasStoredOriginal)
        {
            globalLight.color = originalGlobalColor;
        }

        if (playerLight == null)
        {
            playerLight = GameManager.Instance.Player?.GetComponentInChildren<Light2D>(includeInactive: true);
        }

        if (playerLight != null)
        {
            playerLight.enabled = false;
        }
    }
}
