using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ch2_MorseKey : BasePossessable
{
    [Header("모스 부호 UI")]
    [SerializeField] private TMP_Text morseDisplayText;      // 화면 하단: 현재 입력 중인 모스부호

    [Header("알파벳 UI")]
    [SerializeField] private TextMeshProUGUI decodedDisplayText;    // 화면 중앙: 해석된 알파벳들
    
    [Header("모스키 입력 소리")]
    [SerializeField] private AudioClip dotInputSFX;
    [SerializeField] private AudioClip dashInputSFX;

    private Coroutine shakeCoroutine;

    private Dictionary<string, char> morseToChar = new Dictionary<string, char>()
{
    { ".-", 'A' },
    { "-...", 'B' },
    { "-.-.", 'C' },
    { "-..", 'D' },
    { ".", 'E' },
    { "..-.", 'F' },
    { "--.", 'G' },
    { "....", 'H' },
    { "..", 'I' },
    { ".---", 'J' },
    { "-.-", 'K' },
    { ".-..", 'L' },
    { "--", 'M' },
    { "-.", 'N' },
    { "---", 'O' },
    { ".--.", 'P' },
    { "--.-", 'Q' },
    { ".-.", 'R' },
    { "...", 'S' },
    { "-", 'T' },
    { "..-", 'U' },
    { "...-", 'V' },
    { ".--", 'W' },
    { "-..-", 'X' },
    { "-.--", 'Y' },
    { "--..", 'Z' }
};

    private string currentMorseChar = "";
    private List<char> decodedLetters = new List<char>();

    private float lastInputTime = 0f;
    private const float letterGap = 1.5f; // 글자 간격
    private const float resetThreshold = 10f; // 전체 리셋 간격

    private bool isPressing = false;
    private float pressStartTime;
    private const float dashThreshold = 0.25f;

    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
            return;

        // 입력 감지 (Dot / Dash)
        if (Input.GetMouseButtonDown(0))
        {
            isPressing = true;
            pressStartTime = Time.time;
        }

        if (isPressing && Input.GetMouseButtonUp(0))
        {
            isPressing = false;
            float heldTime = Time.time - pressStartTime;
            lastInputTime = Time.time;

            if (heldTime < dashThreshold)
            {
                OnDotInput();
                SoundManager.Instance.PlaySFX(dotInputSFX); // 점 입력 소리
            }
            else
            {
                OnDashInput();
                SoundManager.Instance.PlaySFX(dashInputSFX); // 대시 입력 소리
            }
        }

        float timeSinceLastInput = Time.time - lastInputTime;

        // 자동 글자 구분 처리
        if (currentMorseChar.Length > 0 && timeSinceLastInput > letterGap)
        {
            DecodeCurrentMorseChar();
        }

        // 전체 입력 리셋 처리
        if ((decodedLetters.Count > 0 || currentMorseChar.Length > 0) && timeSinceLastInput > resetThreshold)
        {
            Debug.Log("입력이 오래 멈췄습니다. 전체 초기화.");
            currentMorseChar = "";
            decodedLetters.Clear();
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        morseDisplayText.text = ConvertToVisualMorse(currentMorseChar);
        decodedDisplayText.text = new string(decodedLetters.ToArray());
    }

    private string ConvertToVisualMorse(string rawMorse)
    {
        return rawMorse.Replace(".", "·").Replace("-", "–"); // 최종 출력 모스 부호
    }

    private void OnDotInput()
    {
        if (currentMorseChar.Length >= 4)
            return;

        currentMorseChar += ".";
        Debug.Log("입력: Dot (.)");
        UpdateUI();
    }

    private void OnDashInput()
    {
        if (currentMorseChar.Length >= 4)
            return;

        currentMorseChar += "-";
        Debug.Log("입력: Dash (-)");
        UpdateUI();
    }

    private void DecodeCurrentMorseChar()
    {
        if (morseToChar.TryGetValue(currentMorseChar, out char letter))
        {
            decodedLetters.Add(letter);
        }
        else
        {
            Debug.Log($"잘못된 모스부호 입력: {currentMorseChar}");
        }

        currentMorseChar = "";
        lastInputTime = Time.time;
        UpdateUI();

        // 최대 4자 초과 방지
        if (decodedLetters.Count >= 4)
        {
            string currentWord = new string(decodedLetters.ToArray());
            Debug.Log($"입력된 단어: {currentWord}");

            if (currentWord == "HELP")
            {
                ActivateExit();
            }
            else
            {
                Debug.Log("틀린 단어입니다. 초기화");
                StartShakeAndClear(); // 진동 애니메이션
            }
        }

    }

    private void StartShakeAndClear()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeAndClear());
    }

    private IEnumerator ShakeAndClear()
    {
        TMP_TextInfo textInfo = decodedDisplayText.textInfo;
        decodedDisplayText.ForceMeshUpdate();

        float duration = 0.5f; // 흔들리는 총 시간
        float timer = 0f;

        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] = textInfo.meshInfo[i].vertices.Clone() as Vector3[];
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                Vector3 offset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f
                ) * 3f; // 흔들림 세기

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = originalVertices[materialIndex][vertexIndex + j] + offset;
                }
            }

            // Mesh 업데이트
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                decodedDisplayText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }

        // 흔들림 후 텍스트 지우기
        decodedLetters.Clear();
        currentMorseChar = "";
        UpdateUI();
    }

    private void ActivateExit()
    {
        // 탈출구 활성화
    }

}
