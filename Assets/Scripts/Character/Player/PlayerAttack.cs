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
    private PlayerParts _parts;

    /// <summary> Rigidbody component of the gameObject </summary>
    private Rigidbody2D _rb;

    #endregion

    private void Awake() {
        _parts = GetComponent<PlayerParts>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Attack(float horizontal, float vertical, bool hPressed, bool vPressed, bool hUp, bool vUp) {
        _anim.ResetTrigger("JabArmRight"); //FIXME? Not sure why I gotta do this, but otherwise the animation plays twice
        _anim.ResetTrigger("SwingArmRight");

        if(_attackHoldTime < _tapThreshold && (hUp || vUp)) _anim.SetTrigger("JabArmRight");

        if(hPressed || vPressed) {
            if(!_anim.IsInTransition(1))
                _anim.SetTrigger("SwingArmRight");
            _attackHoldTime += Time.deltaTime;
        } else {
            _attackHoldTime = 0;
        }
    }
}