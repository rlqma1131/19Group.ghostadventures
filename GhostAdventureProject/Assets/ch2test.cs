using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요
using DG.Tweening; // DOTween을 사용하기 위해 필요

public class TextAnimator : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI targetText;


    void Start()
    {
        PlayTextAnimation();
    }

    public void PlayTextAnimation()
    {

        if (targetText == null)
        {
            Debug.LogError("타겟 텍스트가 할당되지 않았습니다!");
            return;
        }


        Sequence mySequence = DOTween.Sequence();


        mySequence.Append(targetText.transform.DORotate(
            new Vector3(0, 0, 360 * 16), 
            2f,                          
            RotateMode.FastBeyond360     
        ));

  
  
        mySequence.Append(targetText.transform.DOPunchScale(
            new Vector3(5.5f, 5.5f, 0), 
            1.5f,                        
            5,                          
            0.6f
        ));
    }
}