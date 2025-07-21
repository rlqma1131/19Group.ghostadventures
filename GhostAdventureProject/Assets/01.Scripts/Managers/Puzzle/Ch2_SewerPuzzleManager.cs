using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SewerPuzzleManager : MonoBehaviour
{
    public static Ch2_SewerPuzzleManager Instance;
    [SerializeField] private GameObject mazeGroup;
    [SerializeField] private GameObject autoLights;

    private bool isSolved = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mazeGroup.SetActive(!isSolved);
    }

    public void SetPuzzleSolved()
    {
        isSolved = true;
        mazeGroup.SetActive(false);
        autoLights.SetActive(true);
        Debug.Log(" 지하수로 퍼즐 완료 ");
    }

    public bool IsPuzzleSolved() => isSolved;
}
