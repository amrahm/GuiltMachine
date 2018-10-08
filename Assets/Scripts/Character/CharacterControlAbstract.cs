using UnityEngine;

public abstract class CharacterControlAbstract : ScriptableObject {
#if UNITY_EDITOR
    [Tooltip("Show the input fields in the inspector")] [SerializeField]
    private bool _showCurrentInput;
#endif
    [ConditionalHide("_showCurrentInput", true, true)] public float moveHorizontal;
    [ConditionalHide("_showCurrentInput", true, true)] public float moveVertical;
    [ConditionalHide("_showCurrentInput", true, true)] public bool hPressed;
    [ConditionalHide("_showCurrentInput", true, true)] public float sprint;
    [ConditionalHide("_showCurrentInput", true, true)] public bool upPressed;
    [ConditionalHide("_showCurrentInput", true, true)] public bool downPressed;
    
    [ConditionalHide("_showCurrentInput", true, true)] public float attackHorizontal;
    [ConditionalHide("_showCurrentInput", true, true)] public float attackVertical;
    [ConditionalHide("_showCurrentInput", true, true)] public bool attackHPressed;
    [ConditionalHide("_showCurrentInput", true, true)] public bool attackVPressed;

    public abstract void UpdateInput();
}
