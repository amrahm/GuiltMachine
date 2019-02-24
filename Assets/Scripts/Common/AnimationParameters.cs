using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace AnimationParameters {
    public static class Humanoid {
        /// <summary> Float: Horizontal speed </summary>
        public static readonly int SpeedAnim = Animator.StringToHash("Speed");

        /// <summary> Float: Vertical speed </summary>
        public static readonly int VSpeedAnim = Animator.StringToHash("vSpeed");

        /// <summary> Bool: Crouching </summary>
        public static readonly int CrouchingAnim = Animator.StringToHash("Crouching");

        /// <summary> Bool: Roll </summary>
        public static readonly int RollAnim = Animator.StringToHash("Roll");

        /// <summary> Trigger: Jump </summary>
        public static readonly int JumpAnim = Animator.StringToHash("Jump");

        /// <summary> Trigger: Roll-jump </summary>
        public static readonly int RollJumpAnim = Animator.StringToHash("RollJump");

        /// <summary> Bool: Is grounded </summary>
        public static readonly int GroundedAnim = Animator.StringToHash("Grounded");

        /// <summary> Bool: Is falling </summary>
        public static readonly int FallingAnim = Animator.StringToHash("Falling");

        /// <summary> Bool: Is Climbimg </summary>
        public static readonly int ClimbAnim = Animator.StringToHash("Climb");

        /// <summary> Float: Climb speed </summary>
        public static readonly int ClimbAnimSpeed = Animator.StringToHash("ClimbSpeed");

        /// <summary> Bool: Is against wall while climbing </summary>
        public static readonly int ClimbIsAgainstWallAnim = Animator.StringToHash("ClimbIsAgainstWall");

        /// <summary> Trigger: Climb step over </summary>
        public static readonly int ClimbStepOverAnim = Animator.StringToHash("ClimbStepOver");

        /// <summary> Trigger: Air-dash </summary>
        public static readonly int AirDashAnim = Animator.StringToHash("AirDash");
    }

    public static class Weapon {
        /// <summary> Trigger: Unequip current weapon </summary>
        public static readonly int UnequipWeapon = Animator.StringToHash("UnequipWeapon");

        /// <summary> Bool: A sword is equipped </summary>
        public static readonly int SwordEquipped = Animator.StringToHash("SwordEquipped");

        /// <summary> Trigger: Tap forward attack </summary>
        public static readonly int TapForwardAnim = Animator.StringToHash("TapForward");

        /// <summary> Trigger: Hold forward attack </summary>
        public static readonly int HoldForwardAnim = Animator.StringToHash("HoldForward");

        /// <summary> Trigger: Tap down attack </summary>
        public static readonly int TapDownAnim = Animator.StringToHash("TapDown");

        /// <summary> Trigger: Hold down attack </summary>
        public static readonly int HoldDownAnim = Animator.StringToHash("HoldDown");

        /// <summary> Trigger: Tap or hold down attack while in midair </summary>
        public static readonly int TapHoldDownAirAnim = Animator.StringToHash("TapHoldDownAir");
    }
}