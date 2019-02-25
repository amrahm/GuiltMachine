using UnityEngine;

[RequireComponent(typeof(CharacterPhysics))]
public class HumanoidMaster : CharacterMasterAbstract {
    private void Update() {
        if(Input.GetKey("n")) characterStats.DamagePlayer(10); //TODO temp code
        if(Input.GetKey("m")) characterStats.HealPlayer(10);
        if(Input.GetKey("v")) characterStats.DecreaseGuilt(10);
        if(Input.GetKey("b")) characterStats.IncreaseGuilt(10);
    }

    protected override void Die() {
        if(!(rb is null)) rb.freezeRotation = false;
        Destroy(control);
    }
}