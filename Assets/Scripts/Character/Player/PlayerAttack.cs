using UnityEngine;

public class PlayerAttack : MonoBehaviour {
    /// <summary> Reference to the player's animator component </summary>
    private Animator _anim;

    /// <summary> Reference to Parts script, which contains all of the player's body parts </summary>
    private PlayerParts _parts;

    /// <summary> Rigidbody component of the gameObject </summary>
    private Rigidbody2D _rb;

    void Awake() {
        _parts = GetComponent<PlayerParts>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update() { }
}