/*
	Author: Jon Kenkel (nonathaj)
	Date: 2/9/16
    https://wiki.unity3d.com/index.php/Secure_UnitySingleton
*/

using System;

[AttributeUsage(AttributeTargets.Class)]
public class UnitySingletonAttribute : Attribute {
    /// <summary> What kind of singleton is this and how should it be generated? </summary>
    public enum Type {
        /// <summary> Already exists in the scene, just look for it </summary>
        ExistsInScene,

        /// <summary> Load from the Resources folder, at the given path </summary>
        LoadedFromResources,

        /// <summary> Create a new gameobject and create this singleton on it </summary>
        CreateOnNewGameObject,

        /// <summary> Instantiates a prefab as the singleton </summary>
        FromPrefab
    }

    public readonly Type[] singletonTypePriority;
    public readonly bool destroyOnLoad;
    public readonly string resourcesLoadPath;
    public readonly bool allowSetInstance;

    public UnitySingletonAttribute(Type singletonCreateType, bool destroyInstanceOnLevelLoad = true,
                                   string resourcesPath = "", bool allowSet = false) {
        singletonTypePriority = new[] {singletonCreateType};
        destroyOnLoad = destroyInstanceOnLevelLoad;
        resourcesLoadPath = resourcesPath;
        allowSetInstance = allowSet;
    }

    public UnitySingletonAttribute(Type[] singletonCreateTypePriority, bool destroyInstanceOnLevelLoad = true,
                                   string resourcesPath = "", bool allowSet = false) {
        singletonTypePriority = singletonCreateTypePriority;
        destroyOnLoad = destroyInstanceOnLevelLoad;
        resourcesLoadPath = resourcesPath;
        allowSetInstance = allowSet;
    }
}