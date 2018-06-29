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
        //Call movement functions in PlayerMovement
        _playerMovement.Move(CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetButton("Horizontal"), CrossPlatformInputManager.GetAxis("Sprint"));
        _playerMovement.Jump(CrossPlatformInputManager.GetButton("Jump"));
        _playerMovement.Crouch(CrossPlatformInputManager.GetAxis("Vertical") < -0.01f);
    }
}