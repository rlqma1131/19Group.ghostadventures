using System;
using System.Linq;
using UnityEngine;

namespace _01.Scripts.Extensions
{
    public static class GameObjectExtensions
    {
        public static T GetComponentInChildren_SearchByName<T>(this GameObject obj, string name, bool includeInactive = false) where T : Component {
            var components = obj.GetComponentsInChildren<T>(includeInactive);
            var results = components.Where(comp =>
                string.Compare(comp.gameObject.name, name, StringComparison.OrdinalIgnoreCase) == 0);

            var enumerable = results as T[] ?? results.ToArray();
            if (!enumerable.Any()) Debug.LogWarning($"{typeof(T)}를 가진 {name}의 게임오브젝트가 {obj.name} 안에 존재하지 않습니다.");
            return enumerable.FirstOrDefault();
        }

        public static int CompareLayerPriority(this GameObject source, GameObject target) {
            return source.layer < target.layer ? -1 : source.layer > target.layer ? 1 : 0;
        }
    }
}