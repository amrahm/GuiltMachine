using System;
using UnityEngine;

public class ExamplesCamera : MonoBehaviour {
  public Int32 Index;
  public Transform[] Positions;

  void Next() {
    Index = ++Index % Positions.Length;
  }

  void Update() {
    SetPosition(Positions[Index]);
  }

  void OnGUI() {
    float w = 200;
    float hw = w / 2;
    float sw = Screen.width / 2;

    if (GUI.Button(new Rect(sw - hw, 10, w, 50), "Next Example")) {
      Next();
    }
  }

  void SetPosition(Transform t) {
    transform.position = t.position;
    transform.rotation = t.rotation;
  }
}
