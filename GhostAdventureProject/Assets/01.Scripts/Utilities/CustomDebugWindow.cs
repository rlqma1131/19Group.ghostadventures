using _01.Scripts.Managers.Puzzle;
using UnityEditor;
using UnityEngine;

namespace _01.Scripts.Utilities
{
    #if UNITY_EDITOR
    public class CustomDebugWindow : EditorWindow
    {
        [MenuItem("Window/Custom Debug Window")]
        public static void ShowWindow() {
            GetWindow<CustomDebugWindow>("Custom Debug Window");
        }

        void OnGUI() {
            GUILayout.Label("Custom Debug Tool", EditorStyles.boldLabel);

            if (GUILayout.Button("Trigger Spirit Animation")) {
                Ch4_FurnacePuzzleManager.TryGetInstance().TriggerSpiritAnimation();
            }
        }
    }
    #endif
}