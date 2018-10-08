using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerControl")]
public class PlayerControl : CharacterControlAbstract {
    public override void UpdateInput() {
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