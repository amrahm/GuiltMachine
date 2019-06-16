using JetBrains.Annotations;
using System;
using UnityEngine;

public class PuffchildRepelCircleSpecialPart : WeaponSpecialPart {
    [HideInInspector] public float energy = 1;
    private float _oldEnergy;
    [CanBeNull] public SpriteRenderer sprite;
    [CanBeNull] public ParticleSystem particleTrail;
    [CanBeNull, NonSerialized] public Collider2D repelTrigger;
    [SerializeField] private float minScale = .5f;
    [SerializeField] private float energyResetTime = .5f;
    private float _timeTillReset;
    [SerializeField] private float energyResetSpeed = .5f;
    private Vector3 _initRepelScale;

    private void Start() {
        repelTrigger = GetComponent<Collider2D>();
        if(repelTrigger is null) return;
        _initRepelScale = repelTrigger.transform.localScale;
    }

    private void Update() {
        float lerpedMult = Mathf.Lerp(minScale, 1, energy);
        if(!(sprite is null)) {
            Color spriteColor = sprite.color;
            spriteColor.a = lerpedMult;
            sprite.color = spriteColor;
        }
        if(!(repelTrigger is null)) repelTrigger.transform.localScale = _initRepelScale * lerpedMult;

        _timeTillReset = energy < _oldEnergy ? energyResetTime : _timeTillReset - Time.deltaTime;
        if(energy < 1 && _timeTillReset <= 0) energy += Time.deltaTime * energyResetSpeed;

        _oldEnergy = energy;
    }
}