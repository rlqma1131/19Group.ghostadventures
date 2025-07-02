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
    private float defaultFOV;

    public float zoomDuration = 0.5f;
    public float moveDuration = 0.5f;

    [Header("QTE 대상")]
    public Transform playerTarget;
    public Transform enemyTarget;

    private Vector3 originalCamPosition;

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

        // ✅ 최초 시작 시 카메라 원래 FOV 저장
        defaultFOV = mainCamera.fieldOfView;
        originalCamPosition = mainCamera.transform.position;
    }

    public void StartQTEEffects()
    {
        zoomDuration = 5f; // QTE 연출은 천천히
        StartCoroutine(FadeToAlphaRange(0f, 0.9f, fadeDuration));
        StartCoroutine(ZoomTo(zoomFOV, zoomDuration));

        if (playerTarget != null && enemyTarget != null)
        {
            Vector3 mid = (playerTarget.position + enemyTarget.position) / 2f;
            Vector3 camPos = new Vector3(mid.x, mid.y, mainCamera.transform.position.z);
            originalCamPosition = mainCamera.transform.position;
            StartCoroutine(MoveCameraTo(camPos, zoomDuration));
        }
    }

    public void EndQTEEffects()
    {
        zoomDuration = 0.5f; // 복귀는 빠르게

        StartCoroutine(FadeToAlphaRange(darkOverlay.alpha, 0f, fadeDuration));
        StartCoroutine(ZoomTo(defaultFOV, zoomDuration));
        StartCoroutine(MoveCameraTo(originalCamPosition, zoomDuration));
    }

    private IEnumerator FadeToAlphaRange(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        darkOverlay.alpha = fromAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // 부드러운 SmootherStep
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
