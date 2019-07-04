using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterControlAbstract : MonoBehaviour {
    [Tooltip("Show the input fields in the inspector"), SerializeField]
    private bool showCurrentInput;

    [ConditionalHide(nameof(showCurrentInput), true, true)] public float moveHorizontal;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float moveVertical;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool sprint;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool jumpPressed;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool crouchPressed;

    [ConditionalHide(nameof(showCurrentInput), true, true)] public int attackVertical;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public int attackHorizontal;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool blockPressed;

    protected void ResetInput() {
        moveHorizontal = moveVertical = attackHorizontal = attackVertical = 0;
        sprint = jumpPressed = crouchPressed = blockPressed = false;
    }

    public class RegisteredMove {
        public delegate void Move(int polarity = 0, float duration = 0);
        public Move doMove;
        public Vector2 direction;
        public bool continuous;
        public float durationMin;
        public float durationMax;
    }

    public List<RegisteredMove> registeredMoves = new List<RegisteredMove>();
}