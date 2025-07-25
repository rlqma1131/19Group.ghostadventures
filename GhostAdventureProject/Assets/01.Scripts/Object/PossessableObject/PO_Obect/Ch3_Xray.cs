using UnityEngine;
using UnityEngine.UI;

public class Ch3_Xray : BasePossessable
{
    [Header("X-Ray 기계")]
    [SerializeField] private GameObject[] xRayHead; // 머리 ~ 발 순서

    [Header("X-Ray 줌 화면")]
    [SerializeField] Image zoomPhotoScreen;

    [Header("사진")]
    [SerializeField] private Image[] zoomPhotos; // 머리 ~ 발 사진들
    
    [Header("조작키")]
    [SerializeField] private GameObject aKey;
    [SerializeField] private GameObject dKey;

    private int currentPhotoIndex = 0;

    protected override void Start()
    {
        base.Start();

        aKey.SetActive(false);
        dKey.SetActive(false);

        currentPhotoIndex = 0;
        UpdateXrayDisplay();
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            aKey.SetActive(false);
            dKey.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentPhotoIndex < zoomPhotos.Length - 1)
            {
                currentPhotoIndex++;
                UpdateXrayDisplay();
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentPhotoIndex > 0)
            {
                currentPhotoIndex--;
                UpdateXrayDisplay();
            }
        }
    }
    private void UpdateXrayDisplay()
    {
        // 줌 이미지 전환
        zoomPhotoScreen.sprite = zoomPhotos[currentPhotoIndex].sprite;

        // xRayHead 위치 이동
        for (int i = 0; i < xRayHead.Length; i++)
        {
            xRayHead[i].SetActive(i == currentPhotoIndex);
        }
    }

    public override void OnPossessionEnterComplete()
    {
        aKey.SetActive(true);
        dKey.SetActive(true);
    }
}
