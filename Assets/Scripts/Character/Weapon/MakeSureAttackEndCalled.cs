using UnityEngine;

public class MakeSureAttackEndCalled : StateMachineBehaviour {
    public WeaponScript weapon;

#if UNITY_EDITOR || DEBUG
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Debug.Assert(weapon.primaryAttack.attackDefinition != null);
        Debug.Assert(weapon.primaryAttack.attackDefinition != WeaponScript.AttackAction.NoMatches);
    }
#endif

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
#if UNITY_EDITOR || DEBUG
        Debug.Assert(weapon.fadingAttack.attackDefinition != null);
        Debug.Assert(weapon.fadingAttack.attackDefinition != WeaponScript.AttackAction.NoMatches);
        Debug.Assert(weapon.fadingAttack.state == WeaponScript.AttackState.FadingOut);
#endif
        // Just in case the animation event was skipped, send one from the StateMachineBehaviour, which is more reliable
        // but we can't control it's timing. This won't cause an immediate next attack to fade out because this will be
        // called right as the next attack begins, and will only do something if the WeaponScript's primaryAttack is in
        // the recovery state. So this is a safe redundant thing to do unless attacks are literally several milliseconds.
        weapon.ReceiveAnimationEvent(CommonObjectsSingleton.Instance.attackFadeOut, .3f);
    }
}