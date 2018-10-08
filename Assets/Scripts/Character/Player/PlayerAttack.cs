using UnityEngine;

public class PlayerAttack : MonoBehaviour {
    #region Variables

    [Tooltip("How long until an attack is considered held down")] [SerializeField]
    private float _tapThreshold;

    /// <summary> How long an attack key has been held </summary>
    private float _attackHoldTime;


    /// <summary> Reference to the player's animator component </summary>
    private Animator _anim;

    /// <summary> Reference to Parts script, which contains all of the player's body parts </summary>
    private HumanoidParts _parts;

    /// <summary> Rigidbody component of the gameObject </summary>
    private Rigidbody2D _rb;

    /// <summary> The speed parameter in the animator </summary>
    private int _jabArmRightAnim;

    /// <summary> The vertical speed parameter in the animator </summary>
    private int _swingArmRightAnim;

    /// <summary> Was horizontal attack pressed last frame? </summary>
    private bool hWasPressed;

    /// <summary> Was vertical attack pressed last frame? </summary>
    private bool vWasPressed;

    #endregion

    private void Awake() {
        _parts = GetComponent<HumanoidParts>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();

        _jabArmRightAnim = Animator.StringToHash("JabArmRight");
        _swingArmRightAnim = Animator.StringToHash("SwingArmRight");
    }

    public void Attack(float horizontal, float vertical, bool hPressed, bool vPressed) {
        _anim.ResetTrigger(_jabArmRightAnim); //FIXME? Not sure why I gotta do this, but otherwise the animation plays twice
        _anim.ResetTrigger(_swingArmRightAnim);

        if(_attackHoldTime < _tapThreshold && (!hPressed && hWasPressed || !vPressed && vWasPressed)) _anim.SetTrigger(_jabArmRightAnim);

        if(hPressed || vPressed) {
            if(!_anim.IsInTransition(1))
                _anim.SetTrigger(_swingArmRightAnim);
            _attackHoldTime += Time.deltaTime;
        } else {
            _attackHoldTime = 0;
        }

        hWasPressed = hPressed;
        vWasPressed = vPressed;
    }
}