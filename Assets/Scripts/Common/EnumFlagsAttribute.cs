using System.Collections.Generic;
using UnityEngine;

public class EnumFlagsAttribute : PropertyAttribute {
    public static List<int> ReturnSelectedElements<T>(int flagsHolder) {
        List<int> selectedElements = new List<int>();
        for(int i = 0; i < System.Enum.GetValues(typeof(T)).Length; i++)
            if((flagsHolder & 1 << i) != 0)
                selectedElements.Add(i);

        return selectedElements;
    }
}