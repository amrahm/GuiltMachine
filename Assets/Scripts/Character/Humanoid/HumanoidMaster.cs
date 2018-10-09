using UnityEngine;

[RequireComponent(typeof(CharacterPhysics))]
public class HumanoidMaster : CharacterMasterAbstract {
    private CharacterPhysics _characterPhysics;

    protected override void Awake() {
        base.Awake();
        _characterPhysics = GetComponent<CharacterPhysics>();
    }

    private void Update() {
        control.UpdateInput(); //TODO This probably can run less often for NPCs. Figure out better way?
        weapon?.Attack(control.attackHorizontal, control.attackVertical, control.attackHPressed, control.attackVPressed);
        
        if(Input.GetKey("n")) characterStats.DamagePlayer(10); //TODO temp code
        if(Input.GetKey("m")) characterStats.HealPlayer(10);
        if(Input.GetKey("v")) characterStats.DecreaseGuilt(10);
        if(Input.GetKey("b")) characterStats.IncreaseGuilt(10);
    }

    public override void DamageMe(Vector2 point, Vector2 force, int damage, Collider2D hitCollider) {
        if (!godMode) characterStats.DamagePlayer(damage);
        _characterPhysics.AddForceAt(point, force, hitCollider);
    }
}