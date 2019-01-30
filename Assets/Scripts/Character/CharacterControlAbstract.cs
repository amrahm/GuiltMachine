using UnityEngine;

public abstract class CharacterControlAbstract : MonoBehaviour {
#if UNITY_EDITOR
    [Tooltip("Show the input fields in the inspector"), SerializeField] 
    private bool showCurrentInput;
#endif
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float moveHorizontal;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float moveVertical;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool hPressed;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool sprint;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool upPressed;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool downPressed;
    
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float attackHorizontal;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public float attackVertical;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool attackHPressed;
    [ConditionalHide(nameof(showCurrentInput), true, true)] public bool attackVPressed;
}
