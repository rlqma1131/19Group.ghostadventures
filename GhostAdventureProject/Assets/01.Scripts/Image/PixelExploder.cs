using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PixelExploder : MonoBehaviour
{


    public float pixelSize = 0.05f; //픽셀 크키 (스케일)

    //흩어지는 범위
    public float explosionMin = 0.6f;
    public float explosionMax = 1.4f;
    //흩어지는 시간
    public float explodeDuration = 1.0f;
    //모이는 시간
    public float absorbDuration = 1.0f;
    // 흩어지고 모이는 시간 사이의 딜레이
    public float delayBeforeAbsorb = 0.8f;
    // 픽셀 간격 낮을수록 촘촘히 자름
    public int pixelStep = 2;

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


        //스프라이트에서 체크해야함
        if (!tex.isReadable)
        {
            Debug.LogError("Read/Write' 체크했는지 확인");
            return;
        }
        

        //스프라이트의 사영역과 피벗 위치 계산
        Rect spriteRect = sprite.rect;
        Vector2 pivotOffset = sprite.pivot / sprite.pixelsPerUnit;


        // 스프라이트 텍스처 영역을 pixelStep 단위로 순회하며 픽셀 조각 생성
        for (int x = 0; x < spriteRect.width; x += pixelStep)
        {
            for (int y = 0; y < spriteRect.height; y += pixelStep)
            {

                // 텍스처 좌표 계산
                int texX = (int)(spriteRect.x + x);
                int texY = (int)(spriteRect.y + y);

                //색상 가져오기
                Color color = tex.GetPixel(texX, texY);


                // 알파값이 0인 픽셀은 무시
                if (color.a < 0.1f) continue;

                // 픽셀 하나 생성
                GameObject pixelObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                pixelObj.transform.localScale = Vector3.one * pixelSize;
                Vector3 localPos = new Vector3(x, y, 0f) / sprite.pixelsPerUnit - (Vector3)pivotOffset;
                pixelObj.transform.position = transform.position + localPos;

                //머티리얼 설정
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                // 기본 색
                mat.SetColor("_BaseColor", color);

                // Emission HDR (Glow 효과용)
                Color emissionColor = color.linear * 300f;
                mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");

                // Transparent 블렌딩
                mat.SetFloat("_Surface", 1); // 1 = Transparent
                mat.SetFloat("_Blend", 0);
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.renderQueue = 3000;

                pixelObj.GetComponent<Renderer>().material = mat;
                pixelPieces.Add(pixelObj);

                // 폭발 방향 설정
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float explosionDist = Random.Range(explosionMin, explosionMax);
                Vector3 explodeTarget = pixelObj.transform.position + (Vector3)(randomDir * explosionDist);
                float delay = Random.Range(0f, 0.2f);

                // DOTween 폭발 → 흡수 → 페이드
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

        // 원본 오브젝트 제거
        Destroy(gameObject);
    }
}
