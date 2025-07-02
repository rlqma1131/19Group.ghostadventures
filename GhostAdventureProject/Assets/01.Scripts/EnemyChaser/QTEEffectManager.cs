using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QTEEffectManager : MonoBehaviour
{
    public static QTEEffectManager Instance;

    [Header("어두운 오버레이")]
    public CanvasGroup darkOverlay;
    public float fadeDuration = 2.5f;

    [Header("카메라 연출")]
    public Camera mainCamera;
    public float zoomFOV = 30f;
    private float initialFOV;
    private Vector3 initialCamPosition;

    public float zoomDuration = 0.5f;
    public float moveDuration = 0.5f;

    [Header("QTE 대상")]
    public Transform playerTarget;
    public Transform enemyTarget;

    private void Awake()
    {
        Instance = this;

        if (darkOverlay != null)
        {
            darkOverlay.alpha = 0f;
            darkOverlay.blocksRaycasts = false;
            darkOverlay.interactable = false;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        // 최초 시작 시 기준값 저장
        initialFOV = mainCamera.fieldOfView;
        initialCamPosition = mainCamera.transform.position;
    }

    public void StartQTEEffects()
    {
     
        zoomDuration = 5f;

        StartCoroutine(FadeToAlphaRange(0f, 0.9f, fadeDuration));
        StartCoroutine(ZoomTo(zoomFOV, zoomDuration));

        if (playerTarget != null && enemyTarget != null)
        {
            Vector3 mid = (playerTarget.position + enemyTarget.position) / 2f;
            Vector3 camPos = new Vector3(mid.x, mid.y, mainCamera.transform.position.z);
            StartCoroutine(MoveCameraTo(camPos, zoomDuration));
        }
    }

    public void EndQTEEffects()
    {
  
        StopAllCoroutines(); // 모든 코루틴 중지 !!! 
        zoomDuration = 0.5f;

        StartCoroutine(FadeToAlphaRange(darkOverlay.alpha, 0f, fadeDuration));
        StartCoroutine(ZoomTo(initialFOV, zoomDuration));
        StartCoroutine(MoveCameraTo(initialCamPosition, zoomDuration));
    }

    private IEnumerator FadeToAlphaRange(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        darkOverlay.alpha = fromAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            darkOverlay.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        darkOverlay.alpha = toAlpha;
    }

    private IEnumerator ZoomTo(float targetFOV, float duration)
    {
        float startFOV = mainCamera.fieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }

        mainCamera.fieldOfView = targetFOV;
    }

    private IEnumerator MoveCameraTo(Vector3 targetPos, float duration)
    {
        Vector3 start = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            mainCamera.transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
    }
}
