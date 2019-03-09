using UnityEngine;

public class NpcGroundHumanoidControl : CharacterControlAbstract {
    public bool inert;
    private float _nextUpdate;

    private void Update() {
        //TODO Add some actual AI lol
        if(inert || Time.time < _nextUpdate) return;
        _nextUpdate = Time.time + 1; //Update once every second

        //Movement
        moveHorizontal = Random.Range(-1f, 1f);
        sprint = Random.Range(0f, 1f) > 0.1f;
        moveVertical = Random.Range(-1f, 1f);
        jumpPressed = moveVertical > 0.7f;
        crouchPressed = moveVertical < -0.8f;

        //Attack
        //TODO
    }
}
