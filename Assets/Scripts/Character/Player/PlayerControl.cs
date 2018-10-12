using UnityStandardAssets.CrossPlatformInput;

public class PlayerControl : CharacterControlAbstract {
    private void Update() {
        //Movement
        moveHorizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        hPressed = CrossPlatformInputManager.GetButton("Horizontal");
        sprint = CrossPlatformInputManager.GetAxis("Sprint");
        moveVertical = CrossPlatformInputManager.GetAxis("Vertical");
        upPressed = CrossPlatformInputManager.GetButton("Jump");
        downPressed = CrossPlatformInputManager.GetAxis("Vertical") < -0.01f;

        //Attack
        attackHorizontal = CrossPlatformInputManager.GetAxis("AttackHorizontal");
        attackVertical = CrossPlatformInputManager.GetAxis("AttackVertical");
        attackHPressed = CrossPlatformInputManager.GetButton("AttackHorizontal");
        attackVPressed = CrossPlatformInputManager.GetButton("AttackVertical");
    }
}