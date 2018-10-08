using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                AttributeTargets.Class | AttributeTargets.Struct)]
public class ConditionalHideAttribute : PropertyAttribute {
    //The name of the bool field that will be in control
    public readonly string conditionalSourceField;

    //TRUE = Hide in inspector / FALSE = Disable in inspector 
    public readonly bool hideInInspector;

    //TRUE = Always Disable
    public readonly bool disable;
    
    public ConditionalHideAttribute(string conditionalSourceField) {
        this.conditionalSourceField = conditionalSourceField;
        hideInInspector = false;
    }

    public ConditionalHideAttribute(bool disable) {
        this.disable = disable;
    }

    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector) {
        this.conditionalSourceField = conditionalSourceField;
        this.hideInInspector = hideInInspector;
    }
    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector, bool disable) {
        this.disable = disable;
        this.conditionalSourceField = conditionalSourceField;
        this.hideInInspector = hideInInspector;
    }
}