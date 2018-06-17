using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour {
    private PlayerMovement _cMovement; //Reference to PlayerMovement script
    private bool _isJumping; //To determine if the player is jumping

    private void Awake() {
        //References
        _cMovement = GetComponent<PlayerMovement>();
    }

    private void Update() {
        //If he is not jumping...
        if(!_isJumping) {
            //See if button is pressed...
            _isJumping = CrossPlatformInputManager.GetButtonDown("Jump");
        }
    }

    private void FixedUpdate() {
        //Get horizontal axis
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        //Call movement function in PlayerMovement
        _cMovement.Move(h, _isJumping);
        //Reset
        _isJumping = false;
    }
}