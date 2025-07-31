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
    [Header("Rigidbody Settings")]
    public float rigidbodyMass = 0.05f;
    public float rigidbodyDrag = 0.3f;




    [SerializeField] private float explosionDuration = 1.2f;
    void Awake()
    {

        DOTween.SetTweensCapacity(5000, 200);


    }
    //// 테스트용 Update 함수
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
        }
    }


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

    public void RadialFallExplosion()
    {
        if (pixelParticleShader == null)
        {
            Debug.LogError("Pixel Particle Shader 없음 Inspector에서 할당해주세요.");
            return;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("SpriteRenderer 또는 Sprite가 없습니다.");
            return;
        }

        Sprite sprite = sr.sprite;
        Rect spriteRect = sprite.rect;
        Vector2 pivotOffset = sprite.pivot / sprite.pixelsPerUnit;

        Texture2D tex = sprite.texture;
        if (!tex.isReadable)
        {
            Debug.LogError("Sprite 텍스처의 Read/Write Enable을 켜주세요.");
            return;
        }

        // 파란 색상 (원하는 색으로 변경 가능)
        Color baseColor = new Color32(0x2D, 0x72, 0xBF, 255);
        Color emissionColor = baseColor.linear;
        float luminance = emissionColor.r * 0.2126f + emissionColor.g * 0.7152f + emissionColor.b * 0.0722f;
        float correction = ColorValue / Mathf.Max(luminance, 0.001f);
        emissionColor *= correction;

        // 투명하지 않은 픽셀 위치 수집
        List<Vector2Int> validPixels = new List<Vector2Int>();
        for (int x = 0; x < spriteRect.width; x++)
        {
            for (int y = 0; y < spriteRect.height; y++)
            {
                int texX = (int)(spriteRect.x + x);
                int texY = (int)(spriteRect.y + y);
                Color color = tex.GetPixel(texX, texY);
                if (color.a > 0.1f)
                {
                    validPixels.Add(new Vector2Int(x, y));
                }
            }
        }

        if (validPixels.Count == 0)
        {
            Debug.LogWarning("유효한 픽셀이 없습니다.");
            return;
        }

        // 한 픽셀당 몇 개 파편 생성할지
        int fragmentsPerPixel = 10;

        // 최대 생성 파편 개수 (원하는 만큼 조절)
        int maxFragments = 50;

        int totalFragments = Mathf.Min(maxFragments, validPixels.Count * fragmentsPerPixel);

        for (int i = 0; i < totalFragments; i++)
        {
            // 픽셀 인덱스 선택 (모듈로 연산으로 반복 선택)
            Vector2Int pos = validPixels[i / fragmentsPerPixel];

            // 파편 위치에 작은 랜덤 오프셋 추가 (0.01f 단위 조절 가능)
            Vector3 offset = new Vector3(
                Random.Range(-0.02f, 0.02f),
                Random.Range(-0.02f, 0.02f),
                0f);

            Vector3 localPos = new Vector3(pos.x, pos.y, 0f) / sprite.pixelsPerUnit - (Vector3)pivotOffset + offset;
            Vector3 worldPos = transform.position + transform.TransformDirection(localPos);

            GameObject pixelObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(pixelObj.GetComponent<Collider>()); // 기본 콜라이더 제거
            pixelObj.AddComponent<BoxCollider>();      // 3D 리지드바디용 콜라이더 추가

            float randomSize = Random.Range(minPixelSize, maxPixelSize);
            pixelObj.transform.localScale = Vector3.one * randomSize;

            pixelObj.transform.position = worldPos;
            pixelObj.transform.rotation = transform.rotation;

            Material mat = new Material(pixelParticleShader);
            var renderer = pixelObj.GetComponent<MeshRenderer>();
            renderer.material = mat;
            renderer.sortingLayerName = "Front";
            renderer.sortingOrder = sr.sortingOrder;

            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_EmissionColor", emissionColor);
            mat.EnableKeyword("_EMISSION");

            pixelPieces.Add(pixelObj);

            // Rigidbody (3D) 추가
            var rb = pixelObj.AddComponent<Rigidbody>();
            rb.mass = rigidbodyMass;
            rb.drag = rigidbodyDrag;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

            Vector3 randomDir = Random.onUnitSphere.normalized;
            float force = Random.Range(15f, 30f);
            rb.AddForce(randomDir * force);
            rb.AddTorque(Random.onUnitSphere * Random.Range(-30f, 30f));

            // 사라짐 연출
            var fadeSeq = DOTween.Sequence();
            fadeSeq.AppendInterval(delayBeforeAbsorb + 1.0f);
            fadeSeq.Append(mat.DOColor(new Color(baseColor.r, baseColor.g, baseColor.b, 0), "_BaseColor", 3f));
            fadeSeq.Join(mat.DOColor(new Color(emissionColor.r, emissionColor.g, emissionColor.b, 0), "_EmissionColor", 3f));
            fadeSeq.OnComplete(() =>
            {
                fadeSeq.Kill();
                Destroy(pixelObj);
            });
        }

        sr.enabled = false;
    }

    public void SnapToDust()
    {
        if (pixelParticleShader == null)
        {
            Debug.LogError("Pixel Particle Shader 없음! Inspector에서 셰이더를 할당해주세요.");
            return;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("SpriteRenderer 또는 Sprite가 없습니다.");
            return;
        }

        Sprite sprite = sr.sprite;
        pixelStep = Mathf.Max(1, (int)(sprite.rect.width / 48));
        Texture2D tex = sprite.texture;

        if (!tex.isReadable)
        {
            Debug.LogError("Sprite 텍스처의 Read/Write Enable을 켜주세요 (Advanced 설정).");
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

                float size = Random.Range(minPixelSize, maxPixelSize);
                pixelObj.transform.localScale = Vector3.one * size;

                Vector3 localPos = new Vector3(x, y, 0f) / sprite.pixelsPerUnit - (Vector3)pivotOffset;
                pixelObj.transform.position = transform.position + transform.TransformDirection(localPos);
                pixelObj.transform.rotation = transform.rotation;

                Material mat = new Material(pixelParticleShader);
                var renderer = pixelObj.GetComponent<MeshRenderer>();
                renderer.material = mat;

                renderer.sortingLayerName = sr.sortingLayerName;
                renderer.sortingOrder = sr.sortingOrder;

                // 발광 색 계산
                Color emissionColor = color.linear;
                float luminance = emissionColor.r * 0.2126f + emissionColor.g * 0.7152f + emissionColor.b * 0.0722f;
                float correction = ColorValue / Mathf.Max(luminance, 0.001f);
                emissionColor *= correction;

                mat.SetColor("_BaseColor", color);
                mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");

                pixelPieces.Add(pixelObj);

                // 오른쪽 위로 흩날리는 방향
                Vector2 baseDir = new Vector2(1f, 1f).normalized;
                Vector2 randomOffset = Random.insideUnitCircle * 0.3f;
                Vector2 flyDir = (baseDir + randomOffset).normalized;

                float explosionDist = Random.Range(explosionMin, explosionMax);
                Vector3 explodeTarget = pixelObj.transform.position + (Vector3)(flyDir * explosionDist);
                float duration = 5f;
                float delay = Random.Range(5f, 10f);
                float spinZ = Random.Range(-90f, 90f);

                Sequence explodeSeq = DOTween.Sequence();
                explodeSeq.AppendInterval(delay);
                explodeSeq.Join(pixelObj.transform.DOMove(explodeTarget, duration).SetEase(Ease.OutExpo));
                explodeSeq.Join(pixelObj.transform.DORotate(new Vector3(0, 0, spinZ), duration));
                explodeSeq.Join(pixelObj.transform.DOScale(0f, duration * 0.8f));
                explodeSeq.Join(mat.DOColor(new Color(color.r, color.g, color.b, 0), "_BaseColor", duration));
                explodeSeq.Join(mat.DOColor(new Color(emissionColor.r, emissionColor.g, emissionColor.b, 0), "_EmissionColor", duration));

                explodeSeq.OnComplete(() =>
                {
                    Destroy(pixelObj);
                    explodeSeq.Kill();
                });
            }
        }

        // 원래 스프라이트 안 보이게
        sr.enabled = false;
    }




}