using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponAbstract : MonoBehaviour {

    public abstract void Attack(float horizontal, float vertical, bool hPressed, bool vPressed);
}
