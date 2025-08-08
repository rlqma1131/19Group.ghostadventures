using TMPro;
using UnityEngine;

public class Ch1_Shower : BasePossessable
{
    public bool IsHotWater => isWater && temperature == 3;

    [SerializeField] private GameObject water;
    [SerializeField] private GameObject steamEffect;
    [SerializeField] private GameObject temperatureArch;
    [SerializeField] private GameObject Needle;
    [SerializeField] private AudioClip onWaterSound; // 물소리
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject q_key;

    private bool isWater = false;
    private bool isWaterSoundPlaying = false;

    private int temperature = 0;

    private Quaternion initialNeedleRotation;

    protected override void Start()
    {
        base.Start();
        UI.SetActive(false);
        q_key.SetActive(false);
        water.SetActive(false);
        temperatureArch.SetActive(false);
        Needle.SetActive(false);
        initialNeedleRotation = Needle.transform.rotation; // 바늘 회전값 저장 & 초기화
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (!isWater)
        {
            q_key.SetActive(true);
        }
        else
        {
            q_key.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            temperature = Mathf.Max(-3, temperature - 1);
            UpdateNeedleRotation();
            Debug.Log("온도 조절: " + temperature);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            temperature = Mathf.Min(3, temperature + 1);
            UpdateNeedleRotation();
            Debug.Log("온도 조절: " + temperature);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            UI.SetActive(true);
            q_key.SetActive(false);
            isWater = !isWater;
            water.SetActive(isWater);
            temperatureArch.SetActive(isWater);
            Needle.SetActive(isWater);

            UpdateWaterSound();
        }

        if (steamEffect != null)
        {
            steamEffect.SetActive(isWater && temperature == 3);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            temperatureArch.SetActive(false);
            Needle.SetActive(false);

            temperature = 0;
            UpdateNeedleRotation();
            UI.SetActive(false);
            q_key.SetActive(false);
            return;
        }

    }

    private void UpdateWaterSound()
    {
        if (isWater && !isWaterSoundPlaying)
        {
            SoundManager.Instance.PlaySFX(onWaterSound);
            isWaterSoundPlaying = true;
        }
        else if (!isWater && isWaterSoundPlaying)
        {
            SoundManager.Instance.FadeOutAndStopSFX();
            isWaterSoundPlaying = false;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated)
            return;

        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.AddInteractable(gameObject);

        UpdateWaterSound();
    }


    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);

        UpdateWaterSound();
    }

    private void UpdateNeedleRotation()
    {
        if (Needle != null)
        {
            float zAngle = temperature * 20f;
            Needle.transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
        }
    }
}
