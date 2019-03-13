﻿using UnityStandardAssets.CrossPlatformInput;

public class PlayerControl : CharacterControlAbstract {
    private void Update() {
        //Movement
        moveHorizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        sprint = CrossPlatformInputManager.GetButton("Sprint");
        moveVertical = CrossPlatformInputManager.GetAxis("Vertical");
        jumpPressed = CrossPlatformInputManager.GetButton("Jump");
        crouchPressed = CrossPlatformInputManager.GetAxis("Vertical") < -0.01f;

        //Attack
        // Set attackVertical on ButtonDown, and only unset it on ButtonUp if attackVertical is still in that direction
        // This lets the player switch attack direction without having to ButtonUp the other direction first
        // So if they rapidly press one direction then the other, both inputs are always recieved
        if(CrossPlatformInputManager.GetButtonDown("AttackUp"))
            attackVertical = 1;
        if(CrossPlatformInputManager.GetButtonDown("AttackDown"))
            attackVertical = -1;
        if(CrossPlatformInputManager.GetButtonUp("AttackUp") && attackVertical == 1)
            attackVertical = 0;
        if(CrossPlatformInputManager.GetButtonUp("AttackDown") && attackVertical == -1)
            attackVertical = 0;
        // Same for horizontal
        if(CrossPlatformInputManager.GetButtonDown("AttackRight"))
            attackHorizontal = 1;
        if(CrossPlatformInputManager.GetButtonDown("AttackLeft"))
            attackHorizontal = -1;
        if(CrossPlatformInputManager.GetButtonUp("AttackRight") && attackHorizontal == 1)
            attackHorizontal = 0;
        if(CrossPlatformInputManager.GetButtonUp("AttackLeft") && attackHorizontal == -1)
            attackHorizontal = 0;
    }
}