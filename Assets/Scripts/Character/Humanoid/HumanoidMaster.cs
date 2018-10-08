using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(HumanoidParts))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerPhysics))]
public class HumanoidMaster : CharacterMasterAbstract {
    [Tooltip("If God Mode on, player cannot take damage.")]
    public bool godMode;

    [SerializeField] [CanBeNull] private PlayerStatusIndicator _playerStatusIndicator;

    private PlayerAttack _playerAttack;

    private void Awake() {
        //References
        _playerAttack = GetComponent<PlayerAttack>();
        characterStats = Instantiate(characterStats); //create local instance that can be modified
        movement = Instantiate(movement); //create local instance that can be modified
        control = Instantiate(control); //create local instance that can be modified

        movement.Initialize(GetComponent<HumanoidParts>(), GetComponent<Animator>(), GetComponent<Rigidbody2D>(), transform);
        characterStats.Initialize(_playerStatusIndicator);
    }

    private void Update() {
        control.UpdateInput(); //TODO This probably can run less often for NPCs. Figure out better way?
        _playerAttack.Attack(control.attackHorizontal, control.attackVertical, control.attackHPressed, control.attackVPressed);
    }

    private void FixedUpdate() {
        movement.UpdateGrounded();
        movement.MoveHorizontal(control.moveHorizontal, control.hPressed, control.sprint);
        movement.MoveVertical(control.moveVertical, control.upPressed, control.downPressed);
    }

    public override void DamageMe(Vector2 point, Vector2 direction, float force, int damage) {
        //TODO
        characterStats.DamagePlayer(damage);
        //TODO playerphysics.addForceAt or whatever
    }
}