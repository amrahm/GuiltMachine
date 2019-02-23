using UnityStandardAssets.CrossPlatformInput;

public class PlayerControl : CharacterControlAbstract {
    private void Update() {
        //Movement
        moveHorizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        hPressed = CrossPlatformInputManager.GetButton("Horizontal");
        sprint = CrossPlatformInputManager.GetButton("Sprint");
        moveVertical = CrossPlatformInputManager.GetAxis("Vertical");
        jumpPressed = CrossPlatformInputManager.GetButton("Jump");
        crouchPressed = CrossPlatformInputManager.GetAxis("Vertical") < -0.01f;

        //Attack
        attackHorizontal = CrossPlatformInputManager.GetAxis("AttackHorizontal");
        attackVertical = CrossPlatformInputManager.GetAxis("AttackVertical");
        attackHPress = CrossPlatformInputManager.GetButtonDown("AttackHorizontal");
        attackHPressed = CrossPlatformInputManager.GetButton("AttackHorizontal");
        attackHRelease = CrossPlatformInputManager.GetButtonUp("AttackHorizontal");
        attackVPress = CrossPlatformInputManager.GetButtonDown("AttackVertical");
        attackVPressed = CrossPlatformInputManager.GetButton("AttackVertical");
        attackVRelease = CrossPlatformInputManager.GetButtonUp("AttackVertical");
    }
}