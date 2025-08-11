using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SewerPuzzleManager : MonoBehaviour
{
    public static Ch2_SewerPuzzleManager Instance { get; private set; }

    [SerializeField] private GameObject mazeGroup;
    [SerializeField] private GameObject autoLights;
    [SerializeField] private GameObject sewerMemoryObj;
    [SerializeField] private GameObject sewerVolumeObj;

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
        sewerVolumeObj.SetActive(true);
        autoLights.SetActive(true);
        mazeGroup.SetActive(false);
        sewerMemoryObj.SetActive(true);
    }

    public bool IsPuzzleSolved() => isSolved;
}
