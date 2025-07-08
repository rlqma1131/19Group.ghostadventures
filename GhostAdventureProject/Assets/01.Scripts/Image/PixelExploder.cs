using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PixelExploder : MonoBehaviour
{
    [Header("💥 Pixel Explosion Settings")]
    public float minPixelSize = 0.03f;
    public float maxPixelSize = 0.06f;

    public float explosionMin = 0.6f;
    public float explosionMax = 1.4f;
    public float explodeDuration = 1.0f;
    public float absorbDuration = 1.0f;
    public float delayBeforeAbsorb = 0.8f;
    public int pixelStep = 2;

    [Header("🌈 Glow Brightness")]
    public float ColorValue = 5f;

    private List<GameObject> pixelPieces = new List<GameObject>();
    private Transform playerTransform;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;

            Explode();
        }
    }

    void Explode()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;
        Texture2D tex = sprite.texture;

        if (!tex.isReadable)
        {
            Debug.LogError("⚠️ Sprite 텍스처의 Read/Write Enable이 꺼져있습니다!");
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

                // 픽셀 크기를 랜덤으로 지정
                float randomSize = Random.Range(minPixelSize, maxPixelSize);
                pixelObj.transform.localScale = Vector3.one * randomSize;

                Vector3 localPos = new Vector3(x, y, 0f) / sprite.pixelsPerUnit - (Vector3)pivotOffset;
                pixelObj.transform.position = transform.position + localPos;

                // 머티리얼 생성 (Particles/Unlit Shader 사용)
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

                // Emission Glow 밝기 일정하게 맞추기
                Color emissionColor = color.linear;
                float luminance = emissionColor.r * 0.2126f + emissionColor.g * 0.7152f + emissionColor.b * 0.0722f;
                float correction = ColorValue / Mathf.Max(luminance, 0.001f);
                emissionColor *= correction;

                mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");

                // Transparent 블렌딩
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0);
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.renderQueue = 3000;

                // 머티리얼 적용
                var renderer = pixelObj.GetComponent<MeshRenderer>();
                var filter = pixelObj.GetComponent<MeshFilter>();
                renderer.material = mat;

                // 버텍스 컬러로 색상 표현
                Mesh mesh = filter.mesh;
                Color[] colors = new Color[mesh.vertexCount];
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = color;
                mesh.colors = colors;

                pixelPieces.Add(pixelObj);

                // 폭발 방향 설정
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float explosionDist = Random.Range(explosionMin, explosionMax);
                Vector3 explodeTarget = pixelObj.transform.position + (Vector3)(randomDir * explosionDist);
                float delay = Random.Range(0f, 0.2f);

                // 애니메이션 설정
                pixelObj.transform.DOMove(explodeTarget, explodeDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutExpo)
                    .OnComplete(() =>
                    {
                        if (playerTransform != null)
                        {
                            DOVirtual.DelayedCall(delayBeforeAbsorb, () =>
                            {
                                pixelObj.transform.DOMove(playerTransform.position, absorbDuration)
                                    .SetEase(Ease.InCubic)
                                    .OnComplete(() =>
                                    {
                                        mat.DOFade(0f, 0.3f).OnComplete(() =>
                                        {
                                            Destroy(pixelObj);
                                        });
                                    });
                            });
                        }
                    });
            }
        }

        // Destroy(gameObject); // 원본 제거 여부는 상황에 따라
    }
}
