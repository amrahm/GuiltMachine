using System;
using System.Collections;
using ExtensionMethods;
using UnityEngine;

public class PuffchildRepelCircleSpecialPart : WeaponSpecialPart {
    [HideInInspector] public float energy = 1;
    private float _oldEnergy;
    public SpriteRenderer sprite;
    public ParticleSystem particleTrail;
    public ParticleSystem particleBurst;
    [HideInInspector] public Collider2D repelTrigger;
    [SerializeField] private float minScale = .5f;
    [SerializeField] private float energyResetTime = .5f;
    private float _timeTillReset;
    [SerializeField] private float energyResetSpeed = .5f;
    private Vector3 _initRepelScale;
    [NonSerialized] public bool touchingHittable;
    [NonSerialized] public bool exploded;
    private bool _recovering;
    private IDamageable _hitOther;
    private Vector2 _hitOtherPoint;
    private Vector2 _hitOtherDir;
    private Collider2D _hitOtherCollider;
    private Rigidbody2D _rb;
    private WeaponScript _weapon;
    private bool _repelTriggerExists;

    private void Start() {
        _rb = GetComponentInParent<Rigidbody2D>();
        _weapon = GetComponentInParent<WeaponScript>();
        repelTrigger = GetComponent<Collider2D>();
        if(!(_repelTriggerExists = repelTrigger != null)) return;
        _initRepelScale = repelTrigger.transform.localScale;
    }

    private void Update() {
        if(!exploded || _recovering) {
            float lerpedMult = exploded ? energy : Mathf.Lerp(minScale, 1, energy);
            Color spriteColor = sprite.color;
            spriteColor.a = lerpedMult;
            sprite.color = spriteColor;
            if(_repelTriggerExists) repelTrigger.transform.localScale = _initRepelScale * lerpedMult;

            _timeTillReset = energy < _oldEnergy ? energyResetTime : _timeTillReset - Time.deltaTime;
            if(energy < 1 && _timeTillReset <= 0) energy += Time.deltaTime * energyResetSpeed;

            _oldEnergy = energy;
        }
    }

    public IEnumerator _Explode(int damage, float knockback, CharacterControlAbstract ctrl, MovementAbstract mvmt) {
        void AirControl() => _rb.AddForce(10 * _rb.mass * new Vector2(ctrl.moveHorizontal, ctrl.moveVertical));
        _hitOther.DamageMe(_hitOtherPoint, knockback * _rb.mass * _hitOtherDir, damage, _hitOtherCollider);
        exploded = true;
        float initGrav = _rb.gravityScale;
        _rb.gravityScale = 0;
        float initDrag = _rb.drag;
        _rb.drag = 1;
        mvmt.disableMovement = true;
        float startTime = Time.time;
        Color spriteColor = sprite.color;
        spriteColor.a = 0;
        sprite.color = spriteColor;
        if(_repelTriggerExists) repelTrigger.transform.localScale = Vector3.zero;
        if(particleBurst != null) {
            particleBurst.Play();
            float particleLife = particleBurst.main.startLifetimeMultiplier;
            while(Time.time < startTime + particleLife / 2) {
                AirControl();
                yield return null;
            }
            _recovering = true;
            mvmt.disableMovement = false;
            var velMod = particleBurst.velocityOverLifetime;
            float initialRadial = velMod.radialMultiplier;
            var limitVelMod = particleBurst.limitVelocityOverLifetime;
            float initialDrag = limitVelMod.dragMultiplier;
            limitVelMod.dragMultiplier = 0;
            while(Time.time < startTime + particleLife + .4f) {
                velMod.radialMultiplier = velMod.radialMultiplier.SharpInDamp(-10, 0.1f);
                float howCloseToEnd = Mathf.Min((Time.time - (startTime + particleLife / 2)) / (particleLife / 2), 1);
                energy = howCloseToEnd;
                _rb.gravityScale = howCloseToEnd;

                _rb.AddForce(new Vector2(ctrl.moveHorizontal, ctrl.moveVertical));
                yield return null;
            }
            velMod.radialMultiplier = initialRadial;
            limitVelMod.dragMultiplier = initialDrag;
        }
        exploded = false;
        _recovering = false;
        _rb.gravityScale = initGrav;
        _rb.drag = initDrag;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(_weapon.primaryAttack.state != WeaponScript.AttackState.Attacking) return;
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if(damageable is null) return;
        touchingHittable = true;
        _hitOther = damageable;
        if(_repelTriggerExists) {
            var dist = repelTrigger.Distance(other);
            _hitOtherPoint = dist.pointB;
            _hitOtherDir = dist.normal;
        }
        _hitOtherCollider = other;
    }

    private void OnTriggerExit2D(Collider2D other) { touchingHittable = false; }
}