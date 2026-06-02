using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float speed = 5;

    private void Update()
    {
        var current = Keyboard.current;

        var move = Vector3.zero;

        if (current[Key.A].isPressed) { move.x = -1; }
        if (current[Key.D].isPressed) { move.x = 1; }
        if (current[Key.W].isPressed) { move.z = 1; }
        if (current[Key.S].isPressed) { move.z = -1; }

        transform.position += speed * Time.deltaTime * move;
    }
}