using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour {
    private PlayerMovement _playerMovement; //Reference to PlayerMovement script

    private void Awake() {
        //References
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void FixedUpdate() {

        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        bool hPressed = CrossPlatformInputManager.GetButton("Horizontal");
        float sprintAxis = CrossPlatformInputManager.GetAxis("Sprint");
        bool isJumping = CrossPlatformInputManager.GetButton("Jump");
        
        //Call movement functions in PlayerMovement
        _playerMovement.Move(h, hPressed, sprintAxis);
        _playerMovement.Jump(isJumping);
    }
}