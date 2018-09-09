using UnityEngine;
using UnityEditor;

public static class MyMenuCommands
{
    // Creates a menu in the Editor called "My Shortcuts" and adds an item called Redo with the shortcut key Ctrl+Shift+Z 
    [MenuItem("My Shortcuts/Redo %#Z")]
    static void Redo()
    {
        Undo.PerformRedo();
    }
}