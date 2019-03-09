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
}
