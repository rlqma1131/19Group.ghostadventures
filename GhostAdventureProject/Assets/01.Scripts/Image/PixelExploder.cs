using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PixelExploder : MonoBehaviour
{
    [Header("Pixel Explosion Settings")]
    public Shader pixelParticleShader; // 셰이더를 직접 할당받을 변수
    //픽셀 사이즈
    public float minPixelSize = 0.04f;
    public float maxPixelSize = 0.21f;
    //퍼지는범위
    public float explosionMin = 0.6f;
    public float explosionMax = 1.4f;
    //퍼지는 시간
    public float explodeDuration = 1.0f;
    //흡수 시간
    public float absorbDuration = 1.0f;
    //흡수 전 딜레이
    public float delayBeforeAbsorb = 0.8f;
    //픽셀 간격
    public int pixelStep;

    [Header("Glow Brightness")]
    public float ColorValue = 5f;

    private List<GameObject> pixelPieces = new List<GameObject>();
    private Transform playerTransform;

    void Awake()
    {
        
        DOTween.SetTweensCapacity(500, 125);
    }

    // 테스트용 Update 함수
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        Explode();
    //    }
    //}


    public void Explode()
    {
        // 셰이더가 할당되었는지 확인
        if (pixelParticleShader == null)
        {
            Debug.LogError("Pixel Particle Shader 없음 Inspector에서 할당해주세요.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("SpriteRenderer 또는 Sprite가 없습니다.");
            return;
        }

        Sprite sprite = sr.sprite;
        pixelStep = Mathf.Max(1, (int)(sprite.rect.width / 24));
        Texture2D tex = sprite.texture;

        if (!tex.isReadable)
        {
            Debug.LogError("Sprite 텍스처의 Read/Write Enable를 켜주세용 (advanced 열면 있음)");
            return;
        }

        Rect spriteRect = sprite.rect;
        Vector2 pivotOffset = sprite.pivot / sprite.pixelsPerUnit;

        for (int x = 0; x < spriteRect.width; x += pixelStep)
        {
            for (int y = 0; y < spriteRect.height; y += pixelStep)
            {
                int texX = (int)(spriteRect.x + x);
                int texY = (int)(spriteRect.y + y);
                Color color = tex.GetPixel(texX, texY);

                if (color.a < 0.1f) continue;

                GameObject pixelObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(pixelObj.GetComponent<Collider>());

                float randomSize = Random.Range(minPixelSize, maxPixelSize);
                pixelObj.transform.localScale = Vector3.one * randomSize;

                Vector3 localPos = new Vector3(x, y, 0f) / sprite.pixelsPerUnit - (Vector3)pivotOffset;
                pixelObj.transform.position = transform.position + transform.TransformDirection(localPos);
                pixelObj.transform.rotation = transform.rotation;

                // 할당된 셰이더로 새 머티리얼 생성
                Material mat = new Material(pixelParticleShader);

                var renderer = pixelObj.GetComponent<MeshRenderer>();
                renderer.material = mat;

                renderer.sortingLayerName = sr.sortingLayerName;
                renderer.sortingOrder = sr.sortingOrder;

                // URP 셰이더 프로퍼티 설정
                Color emissionColor = color.linear;
                float luminance = emissionColor.r * 0.2126f + emissionColor.g * 0.7152f + emissionColor.b * 0.0722f;
                float correction = ColorValue / Mathf.Max(luminance, 0.001f);
                emissionColor *= correction;

                mat.SetColor("_BaseColor", color);
                mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");


                pixelPieces.Add(pixelObj);

                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float explosionDist = Random.Range(explosionMin, explosionMax);
                Vector3 explodeTarget = pixelObj.transform.position + (Vector3)(randomDir * explosionDist);
                float delay = Random.Range(0f, 0.2f);

                pixelObj.transform.DOMove(explodeTarget, explodeDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutExpo)
                    .OnComplete(() =>
                    {
                        if (playerTransform != null)
                        {
                            var absorbSeq = DOTween.Sequence();
                            absorbSeq.AppendInterval(delayBeforeAbsorb);
                            absorbSeq.Append(pixelObj.transform.DOMove(playerTransform.position, absorbDuration).SetEase(Ease.InCubic));
                            absorbSeq.Append(mat.DOColor(new Color(color.r, color.g, color.b, 0), "_BaseColor", 0.3f));
                            absorbSeq.Join(mat.DOColor(new Color(emissionColor.r, emissionColor.g, emissionColor.b, 0), "_EmissionColor", 0.3f));
                            absorbSeq.OnComplete(() =>
                            {
                                absorbSeq.Kill();
                                Destroy(pixelObj);
                            });
                        }
                        else
                        {
                            var fadeSeq = DOTween.Sequence();
                            fadeSeq.AppendInterval(delayBeforeAbsorb);
                            fadeSeq.Append(mat.DOColor(new Color(color.r, color.g, color.b, 0), "_BaseColor", 0.3f));
                            fadeSeq.Join(mat.DOColor(new Color(emissionColor.r, emissionColor.g, emissionColor.b, 0), "_EmissionColor", 0.3f));
                            fadeSeq.OnComplete(() =>
                            {
                                fadeSeq.Kill();
                                Destroy(pixelObj);
                            });
                        }
                    });
            }
        }

        sr.enabled = false;
    }
}