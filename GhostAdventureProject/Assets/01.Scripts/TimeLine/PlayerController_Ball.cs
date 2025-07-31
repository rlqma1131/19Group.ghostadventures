using UnityEngine;

public class PlayerController_Ball : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private ParticleSystem moveParticle; // <- 파티클 시스템 추가

    private void Awake()
    {
        if(moveParticle == null)
        {
            Debug.LogError("Move Particle System is not assigned in the inspector.");
        }
    }
    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        transform.position += move * moveSpeed * Time.deltaTime;

        // 회전 방향 계산 (좌우 반전은 유지할지 선택 가능)
        if (move.magnitude > 0.01f)
        {
            // 이동 방향 각도 계산 (2D 평면 기준)
            float angle = Mathf.Atan2(v, h) * Mathf.Rad2Deg;

            // 파티클 shape 회전 적용
            var shape = moveParticle.shape;
            shape.rotation = new Vector3(0, 0, angle);
        }

        // 움직임 여부 판단
        bool isMoving = move.magnitude > 0.01f;

        // 파티클 제어
        if (isMoving && moveParticle != null)
        {
            if (!moveParticle.isPlaying)
                moveParticle.Play();
        }
        else
        {
            if (moveParticle.isPlaying)
                moveParticle.Stop();
        }
    }

}
