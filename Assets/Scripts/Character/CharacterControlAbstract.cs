using UnityEngine;

public abstract class CharacterControlAbstract : MonoBehaviour {
#if UNITY_EDITOR
    [Tooltip("Show the input fields in the inspector")] [SerializeField]
    private bool _showCurrentInput;
#endif
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public float moveHorizontal;
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public float moveVertical;
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public bool hPressed;
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public bool sprint;
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public bool upPressed;
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public bool downPressed;
    
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public float attackHorizontal;
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public float attackVertical;
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public bool attackHPressed;
    [ConditionalHide(nameof(_showCurrentInput), true, true)] public bool attackVPressed;
}
