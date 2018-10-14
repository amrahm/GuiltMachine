using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using UnityEditor;
using UnityEngine;
using static CharacterPhysics;

[CustomEditor(typeof(CharacterPhysics))]
public class CharacterPhysicsInspector : Editor {
    private CharacterPhysics _t;
    private SerializedObject _getTarget;
    private SerializedProperty _bodyParts;

    private void OnEnable() {
        _t = (CharacterPhysics) target;
        _getTarget = new SerializedObject(_t);
        _bodyParts = _getTarget.FindProperty(nameof(CharacterPhysics.bodyParts)); // Find the List in our script and create a refrence of it
    }

    public override void OnInspectorGUI() {
        //Update serialized class
        _getTarget.Update();

        //Show the script field
        SerializedProperty prop = serializedObject.FindProperty("m_Script");
        GUI.enabled = false;
        EditorGUILayout.PropertyField(prop, true);
        GUI.enabled = true;

        SerializedProperty animationMode = _getTarget.FindProperty(nameof(CharacterPhysics.animationMode));
        EditorGUILayout.PropertyField(animationMode);
        SerializedProperty crouchSpeed = _getTarget.FindProperty(nameof(CharacterPhysics.crouchSpeed));
        EditorGUILayout.PropertyField(crouchSpeed);
        SerializedProperty nonLegBendParts = _getTarget.FindProperty(nameof(CharacterPhysics.nonLegBendParts));
        SerializedProperty nonLegBendAmounts = _getTarget.FindProperty(nameof(CharacterPhysics.nonLegBendAmounts));
        PlusMinusGameObjectList(nonLegBendParts, -1, nonLegBendAmounts);
        GUILayout.Space(15);

        //Display our list to the inspector window
        for(int i = 0; i < _bodyParts.arraySize; i++) {
            SerializedProperty bodyPartClassRef = _bodyParts.GetArrayElementAtIndex(i);
            SerializedProperty bodyPart = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.bodyPart));
            SerializedProperty parentPart = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.parentPart));
            SerializedProperty colliderObjects = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.colliderObjects));
            SerializedProperty partWeakness = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.partWeakness));
            SerializedProperty lowerLimit = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.lowerLimit));
            SerializedProperty upperLimit = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.upperLimit));
            SerializedProperty partDir = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.partDir));
            SerializedProperty visSettings = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.visSettings));
            SerializedProperty isLeg = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.isLeg));
            SerializedProperty bendParts = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.bendParts));
            SerializedProperty bendAmounts = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.bendAmounts));
            SerializedProperty foot = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.foot));
            SerializedProperty stepVec = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.stepVec));
            SerializedProperty isLeadingLeg = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.isLeadingLeg));
            SerializedProperty maxStepHeight = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.maxStepHeight));
            SerializedProperty footStepHeight = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.footStepHeight));
            SerializedProperty stepHeightMult = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.stepHeightMult));
            SerializedProperty steppingThreshold = bodyPartClassRef.FindPropertyRelative(nameof(BodyPartClass.steppingThreshold));

            string partName = bodyPart.objectReferenceValue == null ? "Part " + i : bodyPart.objectReferenceValue.name;

            bodyPart.isExpanded = EditorGUILayout.Foldout(bodyPart.isExpanded, partName, true);
            if(bodyPart.isExpanded) {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(bodyPart);
                EditorGUILayout.PropertyField(parentPart);
                PlusMinusGameObjectList(colliderObjects, i);
                EditorGUILayout.PropertyField(partWeakness);
                EditorGUILayout.PropertyField(lowerLimit);
                EditorGUILayout.PropertyField(upperLimit);
                EditorGUILayout.PropertyField(partDir);
                EditorGUILayout.PropertyField(isLeg);
                if(isLeg.boolValue) {
                    EditorGUI.indentLevel++;
                    PlusMinusGameObjectList(bendParts, i, bendAmounts);
                    EditorGUILayout.PropertyField(foot);
                    EditorGUILayout.PropertyField(stepVec);
                    EditorGUILayout.PropertyField(isLeadingLeg);
                    EditorGUILayout.PropertyField(maxStepHeight);
                    EditorGUILayout.PropertyField(footStepHeight);
                    EditorGUILayout.PropertyField(stepHeightMult);
                    EditorGUILayout.PropertyField(steppingThreshold);

                    EditorGUILayout.PropertyField(visSettings);
                    EditorGUI.indentLevel--;
                }

                //Add a delete button
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("   Delete " + partName + " Body Part   ")) {
                    _bodyParts.DeleteArrayElementAtIndex(i);
                }
                GUILayout.Space(10);
                GUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space();
        if(GUILayout.Button("Add New Body Part")) {
            List<BodyPartClass> temp = _t.bodyParts.ToList();
            temp.Add(new BodyPartClass());
            _t.bodyParts = temp.ToArray();
            _getTarget.Update();
            _bodyParts.GetArrayElementAtIndex(_bodyParts.arraySize - 1).FindPropertyRelative(nameof(BodyPartClass.bodyPart)).isExpanded = true;
        }

        //Apply the changes to our list
        _getTarget.ApplyModifiedProperties();
    }

    /// <summary> Displays a collapsible list with a plus next to the list name and a minus next to each entry </summary>
    /// <param name="list">The list to display</param>
    /// <param name="bodyPartClassIndex">Index in the bodyParts list where this BodyPartClass is</param>
    /// <param name="list2">A second list to display right next to the first</param>
    private void PlusMinusGameObjectList(SerializedProperty list, int bodyPartClassIndex, SerializedProperty list2 = null) {
        GUILayout.BeginHorizontal();
        GUIContent tooltip = bodyPartClassIndex == -1 ? new GUIContent(list.displayName, HelperMethods.GetTooltip(_t.GetType().GetField(list.name), true))
                                 : new GUIContent(list.displayName, HelperMethods.GetTooltip(_t.bodyParts[bodyPartClassIndex].GetType().GetField(list.name), true));
        list.isExpanded = EditorGUILayout.Foldout(list.isExpanded, tooltip, true);
        if(list.isExpanded) {
            if(list2 != null) {
                while(list2.arraySize < list.arraySize) {
                    list2.InsertArrayElementAtIndex(list2.arraySize);
                    list2.GetArrayElementAtIndex(list2.arraySize - 1).floatValue = 1;
                }
            }
            if(GUILayout.Button("", GUIStyle.none, GUILayout.ExpandWidth(true))) list.isExpanded = !list.isExpanded;
            if(GUILayout.Button("   +   ", GUILayout.MaxWidth(60), GUILayout.MaxHeight(15))) {
                list.InsertArrayElementAtIndex(list.arraySize);
                list2?.InsertArrayElementAtIndex(list2.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = null;
                if(list2 != null) list2.GetArrayElementAtIndex(list2.arraySize - 1).floatValue = 1;
            }
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;
            if(list.arraySize == 0) {
                list.InsertArrayElementAtIndex(0);
                list2?.InsertArrayElementAtIndex(0);
                if(list2 != null) list2.GetArrayElementAtIndex(0).floatValue = 1;
            }

            for(int a = 0; a < list.arraySize; a++) {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(a), GUIContent.none);
                if(list2 != null) {
                    GUILayout.Space(-EditorGUI.indentLevel * 10);
                    EditorGUILayout.PropertyField(list2.GetArrayElementAtIndex(a), GUIContent.none, GUILayout.Width(100));
                    GUILayout.Space(5);
                }

                if(GUILayout.Button("  -  ", GUILayout.MaxWidth(40), GUILayout.MaxHeight(15))) {
                    if(list.GetArrayElementAtIndex(a).objectReferenceValue != null)
                        list.DeleteArrayElementAtIndex(a); //Delete the value first
                    list.DeleteArrayElementAtIndex(a); //Then delete the whole entry
                    list2?.DeleteArrayElementAtIndex(a);
                }
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        } else {
            GUILayout.EndHorizontal();
        }
    }
}