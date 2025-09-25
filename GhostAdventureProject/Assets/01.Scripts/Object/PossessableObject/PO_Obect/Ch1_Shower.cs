using TMPro;
using UnityEngine;

public class Ch1_Shower : BasePossessable
{
    [Header("References")]
    [SerializeField] private GameObject water;
    [SerializeField] private GameObject steamEffect;
    [SerializeField] private GameObject temperatureArch;
    [SerializeField] private GameObject needle;
    [SerializeField] private AudioClip onWaterSound; // 물소리
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject q_key;

    private bool isWater;
    private bool isWaterSoundPlaying;
    private int temperature;
    private Quaternion initialNeedleRotation;

    public bool IsHotWater { get; private set; }

    protected override void Start()
    {
        base.Start();

        UI.SetActive(false);
        q_key.SetActive(false);
        water.SetActive(false);
        temperatureArch.SetActive(false);
        needle.SetActive(false);
        initialNeedleRotation = needle.transform.rotation; // 바늘 회전값 저장 & 초기화
    }

    protected override void Update()
    {
        if (!isPossessed) return;
        
        if (Input.GetKeyDown(KeyCode.E)) {
            Unpossess();
            temperatureArch.SetActive(false);
            needle.SetActive(false);

            temperature = 0;
            UpdateNeedleRotation();
            UI.SetActive(false);
            q_key.SetActive(false);
        }

        ShowerSwitch();
        ChangeWaterTemperature();
        CheckForSteamEffect();
    }

    void CheckForSteamEffect() {
        if (!steamEffect) return;

        steamEffect.SetActive(isWater && temperature == 3);
        
        if (steamEffect.activeInHierarchy && !IsHotWater) {
            IsHotWater = true;
            UIManager.Instance.PromptUI.ShowPrompt("따뜻하니 김이 나네...");
        }
        if (!steamEffect.activeInHierarchy && IsHotWater) {
            IsHotWater = false;
        }
    }

    void ShowerSwitch() {
        if (!Input.GetKeyDown(KeyCode.Q)) return;
        
        isWater = !isWater;

        // 조작키 UI 상태
        q_key.SetActive(!isWater);
        water.SetActive(isWater);
        UI.SetActive(isWater);
        temperatureArch.SetActive(isWater);
        needle.SetActive(isWater);
            
        // 물소리 껐다 켰다
        if (!isWaterSoundPlaying) {
            SoundManager.Instance.PlayLoopingSFX(onWaterSound);
            isWaterSoundPlaying = true;
        }
        else {
            SoundManager.Instance.FadeOutAndStopLoopingSFX();
            isWaterSoundPlaying = false;
        }
    }

    void ChangeWaterTemperature() {
        if (Input.GetKeyDown(KeyCode.D)) {
            temperature = Mathf.Max(-3, temperature - 1);
            UpdateNeedleRotation();
            // Debug.Log("온도 조절: " + temperature);
        }
        else if (Input.GetKeyDown(KeyCode.A)) {
            temperature = Mathf.Min(3, temperature + 1);
            UpdateNeedleRotation();
            // Debug.Log("온도 조절: " + temperature);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated) return;

        if (other.CompareTag("Player"))
        {
            player.InteractSystem.AddInteractable(gameObject);
            //isPlayerNear = true;

            if (isWater)
            {
                SoundManager.Instance.PlayLoopingSFX(onWaterSound);
                isWaterSoundPlaying = true;
            }
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player.InteractSystem.RemoveInteractable(gameObject);
            //isPlayerNear = false;

            if (player.PossessionSystem.CanMove)
            {
                SoundManager.Instance.FadeOutAndStopLoopingSFX();
                isWaterSoundPlaying = false;
            }
        }
    }

    private void UpdateNeedleRotation()
    {
        if (needle)
        {
            float zAngle = temperature * 20f;
            needle.transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
        }
    }

    public void InActiveShower()
    {
        hasActivated = false;
    }

    public override void OnPossessionEnterComplete() 
    {
        base.OnPossessionEnterComplete();
        q_key.SetActive(!isWater);
    }
}
