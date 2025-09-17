using UnityEngine;

namespace _01.Scripts.Object.BaseClasses.Interfaces
{
    public interface IMovable {
        void Move();
        Vector2 GetInputVector();
    }
}