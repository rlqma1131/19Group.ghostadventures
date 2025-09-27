using System.Collections.Generic;
using _01.Scripts.CustomPropertyAttribute;
using UnityEngine;

/// <summary>
/// Player와 까마귀 둘다 상호작용을 할 수 있는 객체
/// 플레이어가 까마귀에 빙의해 접근하여 Q를 누르면 모래성이 무너지고 인형그림 단서와 기억조각이 나타난다.
/// 이때 플레이어가 까마귀에 빙의하지 않고 접근하여 E를 통해 상호작용이 가능한데 이때 힌트를 제공한다.
/// </summary>
public class Ch2_SandCastle : BaseInteractable
{
    [Header("Must be filled References")]
    [SerializeField] GameObject carToy; // 장난감자동차 - Ch2_부정기억1
    [SerializeField] GameObject dollpicture;
    [SerializeField] GameObject SandCastle_intactly; // 모래성
    [SerializeField] GameObject SandCastle_crumble; // 무너진 모래성
    [SerializeField] GameObject q_key;
    
    [Header("Dynamic Reference")]
    [SerializeField] Ch2_Raven raven; // 까마귀
    
    [Header("Lines of Hint")]
    [SerializeField] List<string> hints = new()
    {
        "모래성 안에 무언가 들어있다.", 
        "파해쳐야 꺼낼 수 있을 것 같다.", 
        "근데 어떻게 파해치지?"
    };
    
    [SerializeField, ReadOnly] public bool isInRange; // 모래성을 무너뜨릴 수 있는 범위에 있는지 확인
    bool crumbled; // 모래성을 무너뜨렸는지 확인

    override protected void Start() {
        base.Start();
        carToy.SetActive(false);
        dollpicture.SetActive(false);
        SandCastle_crumble.SetActive(false);
        q_key.SetActive(false);
    }

    void Update() {
        if (isScannable && isInRange) {
            if (Input.GetKeyDown(KeyCode.E)) {
                UIManager.Instance.PromptUI.ShowPrompt_2(hints.ToArray());
            }
        }

        if (isInRange && !crumbled && raven && raven.IsPossessed) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                SandCastle_crumble.SetActive(true);
                carToy.SetActive(true);
                dollpicture.SetActive(true);
                SandCastle_intactly.SetActive(false);
                q_key.SetActive(false);
                crumbled = true;
                isScannable = false;
            }
        }
    }

    override protected void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player") || collision.CompareTag("Animal")) {
            isInRange = true;
            if (collision.TryGetComponent(out Ch2_Raven value)) {
                raven = value;
                if (!value.HasActivated()) return;
            }
            else {
                return;
            }

            if (raven.IsPossessed && !crumbled) {
                UIManager.Instance.PromptUI.ShowPrompt("무너뜨릴까?");
                q_key.SetActive(true);
            }
        }
    }

    override protected void OnTriggerExit2D(Collider2D other) {
        base.OnTriggerEnter2D(other);

        if (other.CompareTag("Player") || other.CompareTag("Animal")) {
            isInRange = false;
            if (!crumbled) q_key.SetActive(false);
        }
    }

    public bool IsCrumbled() => crumbled;
}