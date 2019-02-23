using UnityEngine;

public abstract class CharacterControlAbstract : MonoBehaviour {
    [Tooltip("Show the input fields in the inspector"), SerializeField] 
    private bool showCurrentInput;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float moveHorizontal;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float moveVertical;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool hPressed;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool sprint;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool jumpPressed;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool crouchPressed;
    
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float attackHorizontal;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float attackVertical;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool attackHPress;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool attackHPressed;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool attackHRelease;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool attackVPress;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool attackVPressed;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool attackVRelease;
}
