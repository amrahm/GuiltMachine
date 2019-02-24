﻿using UnityEngine;

[RequireComponent(typeof(CharacterPhysics))]
public class HumanoidMaster : CharacterMasterAbstract {
    [SerializeField] private GameObject bloodEffect;

    private void Update() {
        if(Input.GetKey("n")) characterStats.DamagePlayer(10); //TODO temp code
        if(Input.GetKey("m")) characterStats.HealPlayer(10);
        if(Input.GetKey("v")) characterStats.DecreaseGuilt(10);
        if(Input.GetKey("b")) characterStats.IncreaseGuilt(10);
    }

    public override void DamageMe(Vector2 point, Vector2 force, int damage, Collider2D hitCollider) {
        if(hitCollider.isTrigger) return;

        if(!godMode) characterStats.DamagePlayer(damage);
        characterPhysics?.AddForceAt(point, force, hitCollider);

        //TODO Object pool
        if(!(bloodEffect is null)) {
            var blood = Instantiate(bloodEffect, transform);
            blood.transform.position = point;
            blood.transform.right = force;
        }
    }
}