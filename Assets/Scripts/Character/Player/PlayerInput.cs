﻿using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour {
    private PlayerMovement _playerMovement; //Reference to PlayerMovement script
    private float _move;
    private bool _movePressed;
    private float _sprint;
    private bool _jump;
    private bool _crouching;

    private void Awake() {
        //References
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update() {
        _move = CrossPlatformInputManager.GetAxis("Horizontal");
        _movePressed = CrossPlatformInputManager.GetButton("Horizontal");
        _sprint = CrossPlatformInputManager.GetAxis("Sprint");
        _jump = CrossPlatformInputManager.GetButton("Jump");
        _crouching = CrossPlatformInputManager.GetAxis("Vertical") < -0.01f;

        _playerMovement.Crouch(_crouching);
    }

    private void FixedUpdate() {
        _playerMovement.Move(_move, _movePressed, _sprint);
        _playerMovement.Jump(_jump);
    }
}