using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/NpcGroundHumanoidControl")]
public class NpcGroundHumanoidControl : CharacterControlAbstract {
    private float _nextUpdate;
    public override void UpdateInput() {
        //TODO Add some actual AI lol
        if(Time.time < _nextUpdate) return;
        _nextUpdate = Time.time + 1; //Update once every second

        //Movement
        moveHorizontal = Random.Range(-1f, 1f);
        hPressed = Mathf.Abs(moveHorizontal) > 0.05f;
        sprint = Random.Range(0f, 1f);
        moveVertical = Random.Range(-1f, 1f);
        upPressed = moveVertical > 0.7f;
        downPressed = moveVertical < -0.8f;

        //Attack
        float horiz = Random.Range(-1f, 1f);
        float vert = Random.Range(-1f, 1f);
        attackHorizontal = Mathf.Abs(horiz) > 0.9f ? horiz : 0;
        attackVertical = Mathf.Abs(vert) > 0.9f ? vert : 0;
        attackHPressed = Mathf.Abs(horiz) > 0.9f;
        attackVPressed = Mathf.Abs(vert) > 0.9f;
    }
}
