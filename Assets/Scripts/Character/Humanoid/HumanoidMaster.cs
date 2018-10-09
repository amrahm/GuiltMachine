using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerPhysics))]
public class HumanoidMaster : CharacterMasterAbstract {
    [Tooltip("If God Mode on, character cannot take damage.")]
    public bool godMode;

    [SerializeField] [CanBeNull] private PlayerStatusIndicator _playerStatusIndicator;

    private void Awake() {
        //References
        characterStats = Instantiate(characterStats); //create local instance that can be modified
        control = Instantiate(control); //create local instance that can be modified

        if(_playerStatusIndicator != null) characterStats.HealthChanged += _playerStatusIndicator.SetHealth;
        if(_playerStatusIndicator != null) characterStats.GuiltChanged += _playerStatusIndicator.SetGuilt;
        characterStats.Initialize();

        if(weapon != null) weapon.holder = this;
    }

    private void OnEnable() {
        if(_playerStatusIndicator != null) {
            characterStats.HealthChanged -= _playerStatusIndicator.SetHealth; //Prevent adding it twice
            characterStats.HealthChanged += _playerStatusIndicator.SetHealth;
        }
        if(_playerStatusIndicator != null) {
            characterStats.GuiltChanged -= _playerStatusIndicator.SetGuilt; //Prevent adding it twice
            characterStats.GuiltChanged += _playerStatusIndicator.SetGuilt;
        }
    }

    private void OnDisable() {
        if(_playerStatusIndicator != null) characterStats.HealthChanged -= _playerStatusIndicator.SetHealth;
        if(_playerStatusIndicator != null) characterStats.GuiltChanged -= _playerStatusIndicator.SetGuilt;
    }

    private void Update() {
        control.UpdateInput(); //TODO This probably can run less often for NPCs. Figure out better way?
        weapon?.Attack(control.attackHorizontal, control.attackVertical, control.attackHPressed, control.attackVPressed);
        
        if(Input.GetKey("n")) characterStats.DamagePlayer(10); //TODO temp code
        if(Input.GetKey("m")) characterStats.HealPlayer(10);
        if(Input.GetKey("v")) characterStats.DecreaseGuilt(10);
        if(Input.GetKey("b")) characterStats.IncreaseGuilt(10);
    }

    public override void DamageMe(Vector2 point, Vector2 force, int damage) {
        //TODO
        characterStats.DamagePlayer(damage);
        //TODO playerphysics.addForceAt or whatever
    }
}