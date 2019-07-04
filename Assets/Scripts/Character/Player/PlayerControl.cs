using Cinemachine;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerControl : CharacterControlAbstract {
    private void OnEnable() {
        Camera main = Camera.main;
        if(main != null) {
            main.GetComponentInChildren<CinemachineVirtualCamera>().Follow = transform;
            GetComponent<CharacterMasterAbstract>().statusIndicator =
                main.GetComponentInChildren<PlayerStatusIndicator>();
        }
    }

    private void Update() {
        //Movement
        moveHorizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        sprint = CrossPlatformInputManager.GetButton("Sprint");
        moveVertical = CrossPlatformInputManager.GetAxis("Vertical");
        jumpPressed = CrossPlatformInputManager.GetButton("Jump");
        crouchPressed = moveVertical < -0.01f;

        //Attack
        // Set attackVertical on ButtonDown, and only unset it on ButtonUp if attackVertical is still in that direction
        // This lets the player switch attack direction without having to ButtonUp the other direction first
        // So if they rapidly press one direction then the other, both inputs are always recieved
        if(CrossPlatformInputManager.GetButtonDown("AttackUp"))
            attackVertical = 1;
        else if(CrossPlatformInputManager.GetButtonDown("AttackDown"))
            attackVertical = -1;
        else if(CrossPlatformInputManager.GetButtonUp("AttackUp") && attackVertical == 1 ||
                CrossPlatformInputManager.GetButtonUp("AttackDown") && attackVertical == -1)
            attackVertical = 0;
        // Same for horizontal
        if(CrossPlatformInputManager.GetButtonDown("AttackRight"))
            attackHorizontal = 1;
        else if(CrossPlatformInputManager.GetButtonDown("AttackLeft"))
            attackHorizontal = -1;
        else if(CrossPlatformInputManager.GetButtonUp("AttackRight") && attackHorizontal == 1 ||
                CrossPlatformInputManager.GetButtonUp("AttackLeft") && attackHorizontal == -1)
            attackHorizontal = 0;

        blockPressed = CrossPlatformInputManager.GetButton("Block");
    }
}