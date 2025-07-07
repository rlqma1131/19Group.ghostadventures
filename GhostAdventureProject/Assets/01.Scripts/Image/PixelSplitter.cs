using UnityEngine;

public class PixelExploder : MonoBehaviour
{
    public float pixelSize = 0.05f;        // 픽셀 크기 (씬에서 스케일에 맞게 조절)
    public float explosionForce = 100f;    // 얼마나 세게 흩어질지
    public float duration = 2f;            // 파편 유지 시간

    private void Start()
    {
        Explode();
    }

    void Explode()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Texture2D tex = sr.sprite.texture;

        if (!tex.isReadable)
        {
            Debug.LogError("텍스처가 읽을 수 없습니다! 'Read/Write Enabled' 체크했는지 확인하세요.");
            return;
        }

        Vector2 pivotOffset = sr.sprite.pivot / sr.sprite.pixelsPerUnit;

        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                Color color = tex.GetPixel(x, y);
                if (color.a < 0.1f) continue; // 투명한 픽셀은 무시

                GameObject pixelObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                pixelObj.transform.localScale = Vector3.one * pixelSize;

                // 픽셀 위치 계산
                Vector3 localPos = new Vector3(x, y, 0f) / sr.sprite.pixelsPerUnit - (Vector3)pivotOffset;

                pixelObj.transform.position = transform.position + localPos;

                // 색상 설정
                pixelObj.GetComponent<Renderer>().material.color = color;

                // Rigidbody로 폭발 효과
                Rigidbody2D rb = pixelObj.AddComponent<Rigidbody2D>();
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDir * explosionForce);

                // 몇 초 후 삭제
                Destroy(pixelObj, duration);
            }
        }

        // 원래 오브젝트 삭제
        Destroy(gameObject);
    }
}
