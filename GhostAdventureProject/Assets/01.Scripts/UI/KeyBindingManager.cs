// using System;
// using System.Collections.Generic;
// using UnityEngine;

// 키설정 스크립트입니다.(현재 미사용)

// public class KeyBindingManager : MonoBehaviour, IUIClosable
// {
//     public static KeyBindingManager Instance;

//     public Dictionary<int, KeyCode> clueKeyBindings = new Dictionary<int, KeyCode>();

//     private void Awake()
//     {
//         if (Instance == null) Instance = this;

//         // 기본 키 설정 (Clue1~5)
//         for (int i = 0; i < 5; i++)
//         {
//             clueKeyBindings[i] = KeyCode.Alpha1 + i;
//         }
//     }

//     private void Start()
//     {
//         this.gameObject.SetActive(false);
//     }

//     public void SetKey(int clueIndex, KeyCode key)
//     {
//         clueKeyBindings[clueIndex] = key;
//     }

//     public KeyCode GetKey(int clueIndex)
//     {
//         return clueKeyBindings.ContainsKey(clueIndex) ? clueKeyBindings[clueIndex] : KeyCode.None;
//     }

//     internal string GetDisplayName(KeyCode key)
//     {
//         throw new NotImplementedException();
//     }

//     public void Open()
//     {
//         this.gameObject.SetActive(true);
//     }
//     public void Close()
//     {
//         this.gameObject.SetActive(false);
//     }

//     public bool IsOpen()
//     {
//         return this.gameObject.activeInHierarchy;
//     }

//     public static class KeyNameHelper
// {
//     public static string GetDisplayName(KeyCode key)
//     {
//         string name = key.ToString();

//         // 숫자 키 (Alpha1 ~ Alpha9 → 1 ~ 9)
//         if (name.StartsWith("Alpha") && name.Length == 6)
//         {
//             return name.Substring(5);
//         }

//         // // Keypad 숫자 (Keypad1 ~ Keypad9 → KP1 ~ KP9)
//         // if (name.StartsWith("Keypad") && name.Length == 7)
//         // {
//         //     return "KP" + name.Substring(6);
//         // }

//             // // Keypad 숫자 (Keypad1 ~ Keypad9 → KP1 ~ KP9)
//         // if (name.StartsWith("F") && name.Length == 2)
//         // {
//         //     return name;
//         // }
//         // // 스페이스 등 특수 키 포맷 조정 (예시)
//         // if (key == KeyCode.Space)
//         // {
//         //     return "Space";
//         // }

//         // 기본 이름 그대로
//         return name;
//     }
// }
// }
