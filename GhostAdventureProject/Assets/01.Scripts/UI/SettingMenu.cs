using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour, IUIClosable
{
    [Header("Settings Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject keybindingPanel;
    [SerializeField] private GameObject escMenuPanel;

    [Header("Sound")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Language")]
    [SerializeField] private Dropdown languageDropdown;

    [Header("Keybind")]
    [SerializeField] private Button rebindJumpButton;



    // 필요 시 더 많은 키 바인딩 버튼들...

    private void Start()
    {
        settingsPanel.SetActive(false);
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
        bgmSlider.value = SoundManager.Instance.BGMVolume;
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);
        sfxSlider.value = SoundManager.Instance.SFXVolume;

        // 초기화
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (rebindJumpButton != null)
        {
            rebindJumpButton.onClick.AddListener(() => StartRebind("Jump"));
        }

        
    }

    public void OpenSettings()
    {
        escMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void Close()
    {
        escMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public bool IsOpen()
    {
        return settingsPanel.activeInHierarchy;
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        Debug.Log($"Master Volume: {value}");
    }
    private void OnLanguageChanged(int index)
    {
        string selectedLang = languageDropdown.options[index].text;
        Debug.Log($"Language changed to: {selectedLang}");

        // TODO: 언어 변경 적용
    }

    private void StartRebind(string actionName)
    {
        Debug.Log($"Rebinding key for: {actionName}");

        // TODO: Input System을 통한 키 바인딩 재설정 구현
    }

    //--------------

    private int waitingSlot = -1;

    public void OnClick_RebindClueKey(int slotIndex)
    {
        waitingSlot = slotIndex;
        Debug.Log($"단서 {slotIndex + 1} 키 변경 대기 중...");
    }

    // private void Update()
    // {
    //     if (waitingSlot != -1)
    //     {
    //         foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
    //         {
    //             if (Input.GetKeyDown(key))
    //             {
    //                 keybindConfig.SetKeyForSlot(waitingSlot, key);
    //                 PlayerPrefs.SetString($"ClueKey{waitingSlot}", key.ToString());
    //                 Debug.Log($"단서 {waitingSlot + 1} 키가 {key}로 설정됨");
    //                 waitingSlot = -1;
    //                 break;
    //             }
    //         }
    //     }
    // }


}
