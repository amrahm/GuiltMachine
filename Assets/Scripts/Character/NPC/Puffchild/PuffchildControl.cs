using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuffchildControl : CharacterControlAbstract {
    public Transform target;
    [SerializeField] private bool learningMode;
    private readonly List<Vector3> _points = new List<Vector3>();
    private RegisteredMove _hMove;

    private IEnumerator Start() {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        _hMove = registeredMoves.First(move => move.continuous);

        if(!learningMode) yield break;
        foreach(var move in registeredMoves) {
//            if(!move.continuous) move.doMove(move.durationMax);
            float time = 0;
            while(time < 3) {
//                if(move.continuous) move.doMove(0);
                _points.Add(transform.position);

                time += Time.deltaTime;
                yield return null;
            }
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            transform.position = Vector3.zero;
        }
    }

    private void Update() {
        foreach(Vector3 point in _points) DebugExtension.DebugPoint(point);
        ResetInput();

        if(Vector3.Distance(transform.position, target.position) > .5f) {
            Vector2 targetDir = target.position - transform.position;
            float bestAngle = float.MaxValue; //TODO make smarter
            RegisteredMove bestMove = null;
            foreach(var move in registeredMoves) {
                if(!move.checkCondition()) continue;
                float angle = Vector2.Angle(targetDir, move.direction);
                if(angle < bestAngle) {
                    bestAngle = angle;
                    bestMove = move;
                }
            }
            bestMove?.doMove(bestMove.durationMax);
            print(bestMove?.name);
        }
    }
}