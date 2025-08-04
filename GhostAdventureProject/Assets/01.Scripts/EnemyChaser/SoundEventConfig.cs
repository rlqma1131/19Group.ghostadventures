using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEvent", menuName = "SoundEvent/SoundEventConfig")]
public class SoundEventConfig : ScriptableObject
{
    public float soundRange = 10f;
    public float chaseDuration = 10f;
}
