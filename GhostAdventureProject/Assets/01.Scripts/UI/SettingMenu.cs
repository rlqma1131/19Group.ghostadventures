using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    [Header("Settings Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject escMenuPanel;

    [Header("Sound")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Language")]
    [SerializeField] private Dropdown languageDropdown;

    [Header("Keybind")]
    [SerializeField] private Button rebindJumpButton;
    // 필요 시 더 많은 키 바인딩 버튼들...

    private void Start()
    {
        settingsPanel.SetActive(false);

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

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        escMenuPanel.SetActive(true);
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
}
