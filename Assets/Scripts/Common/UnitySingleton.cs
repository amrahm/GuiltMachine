/*
	Author: Jon Kenkel (nonathaj)
	Date: 2/9/16
    https://wiki.unity3d.com/index.php/Secure_UnitySingleton
*/

using System;
using UnityEngine;
using static UnitySingletonAttribute.Type;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

/// <inheritdoc />
/// <summary> Class for holding singleton component instances in Unity. </summary>
/// <example>
/// [UnitySingleton(UnitySingletonAttribute.Type.LoadedFromResources, false, "test")]
/// public class MyClass : UnitySingleton&lt;MyClass&gt; { }
/// </example>
/// <example>
/// [UnitySingleton(UnitySingletonAttribute.Type.CreateOnNewGameObject)]
/// public class MyOtherClass : UnitySingleton&lt;MyOtherClass&gt; { }
/// </example>
/// <typeparam name="T">The type of the singleton</typeparam>
public abstract class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static bool _applicationIsQuitting;

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public void OnDestroy() {
        _applicationIsQuitting = true;
    }

    // ReSharper disable once StaticMemberInGenericType
    private static readonly object Lock = new object();

    /// <summary> Is there an instance active of this singleton? </summary>
    public static bool InstanceExists => _instance != null;

    private static T _instance;

    /// <summary>
    /// Returns an instance of this singleton
    /// (if it does not exist, generates one based on T's UnitySingleton Attribute settings)
    /// </summary>
    public static T Instance {
        get {
            if(_applicationIsQuitting) {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit." +
                                 " Won't create again - returning null.");
                return null;
            }

            lock(Lock) {
                TouchInstance();
                return _instance;
            }
        }
        set {
            if(!(Attribute.GetCustomAttribute(typeof(T), typeof(UnitySingletonAttribute))
                     is UnitySingletonAttribute attribute)) {
                Debug.LogError("Cannot find UnitySingleton attribute on " + typeof(T).Name);
            } else if(attribute.allowSetInstance) {
                _instance = value;
            } else {
                Debug.LogError(typeof(T).Name + " is not allowed to set instances. " +
                               "Please set the allowSetInstace flag to true to enable this feature.");
            }
        }
    }

    /// <summary> Destroy the current static instance of this singleton </summary>
    /// <param name="destroyGameObject"> Should we destroy the gameobject of the instance too? </param>
    public static void DestroyInstance(bool destroyGameObject = true) {
        if(InstanceExists) {
            if(destroyGameObject)
                Destroy(_instance.gameObject);
            else
                Destroy(_instance);
            _instance = null;
        }
    }

    /// <summary> Called when this object is created. Children should call this base method when overriding. </summary>
    protected virtual void Awake() {
        if(InstanceExists && _instance != this)
            Destroy(gameObject);
        TouchInstance();
    }

    /// <summary> Ensures that an instance of this singleton is generated </summary>
    public static void TouchInstance() {
        if(!InstanceExists) Generate();
    }

    /// <summary> Generates this singleton </summary>
    private static void Generate() {
        if(!(Attribute.GetCustomAttribute(typeof(T), typeof(UnitySingletonAttribute))
                 is UnitySingletonAttribute attribute)) {
            Debug.LogError("Cannot find UnitySingleton attribute on " + typeof(T).Name);
            return;
        }

        T[] instances = FindObjectsOfType<T>();
        int count = instances.Length;
        if(count > 0) {
            if(count == 1) {
                _instance = instances[0];
                return;
            }
            Debug.LogWarning(
                $"[{nameof(UnitySingleton<T>)}] There should never be more than one {nameof(UnitySingleton<T>)} " +
                $"in the scene, but {count} were found. " +
                "The first instance found will be used, and all others will be destroyed.");
            for(int i = 1; i < instances.Length; i++) Destroy(instances[i]);
            _instance = instances[0];
            return;
        }

        for(int x = 0; x < attribute.singletonTypePriority.Length; x++) {
            if(TryGenerateInstance(attribute.singletonTypePriority[x], attribute.destroyOnLoad,
                                   attribute.resourcesLoadPath, x == attribute.singletonTypePriority.Length - 1))
                break;
        }
    }

    /// <summary> Attempts to generate a singleton with the given parameters </summary>
    /// <param name="type"> What type of singleton should this be? </param>
    /// <param name="destroyOnLoad"> false iff should persist across scenes </param>
    /// <param name="resourcesLoadPath"> Path used if type is LoadedFromResources </param>
    /// <param name="warn"></param>
    /// <returns> true iff instance was generated </returns>
    private static bool TryGenerateInstance(UnitySingletonAttribute.Type type, bool destroyOnLoad,
                                            string resourcesLoadPath, bool warn) {
        switch(type) {
            case ExistsInScene: {
                _instance = FindObjectOfType<T>();
                if(_instance == null) {
                    if(warn)
                        Debug.LogError($"Cannot find an object with a {typeof(T).Name}. Please add one to the scene.");
                    return false;
                }
                break;
            }
            case LoadedFromResources when string.IsNullOrEmpty(resourcesLoadPath): {
                if(warn)
                    Debug.LogError("UnitySingletonAttribute.resourcesLoadPath is not a " +
                                   $"valid Resources location in {typeof(T).Name}");
                return false;
            }
            case LoadedFromResources: {
                T pref = Resources.Load<T>(resourcesLoadPath);
                if(pref == null) {
                    if(warn)
                        Debug.LogError($"Failed to load prefab with {typeof(T).Name} component attached to it from " +
                                       $"folder Resources/{resourcesLoadPath}. Please add a prefab with the " +
                                       "component to that location, or update the location.");
                    return false;
                }
                _instance = Instantiate(pref);
                if(_instance == null) {
                    if(warn)
                        Debug.LogError($"Failed to create instance of prefab {pref} with component {typeof(T).Name}. " +
                                       "Please check your memory constraints");
                    return false;
                }
                break;
            }
            case CreateOnNewGameObject: {
                GameObject go = new GameObject($"{typeof(T).Name} Singleton");
                if(go == null) {
                    if(warn)
                        Debug.LogError($"Failed to create gameobject for instance of {typeof(T).Name}. " +
                                       "Please check your memory constraints.");
                    return false;
                }
                _instance = go.AddComponent<T>();
                if(_instance == null) {
                    if(warn)
                        Debug.LogError($"Failed to add component of {typeof(T).Name} to new gameobject. " +
                                       "Please check your memory constraints.");
                    Destroy(go);
                    return false;
                }
                break;
            }
            case FromPrefab: {
                // Check if exists a singleton prefab on Resources Folder.
                // -- Prefab must have the same name as the Singleton SubClass
                GameObject singletonPrefab = (GameObject) Resources.Load(typeof(T).ToString(), typeof(GameObject));

                // Create singleton as new or from prefab
                GameObject go;
                if(singletonPrefab != null) {
                    go = Instantiate(singletonPrefab);
                } else {
                    go = new GameObject();
                    if(warn)
                        Debug.LogError($"Failed to find prefab of name {typeof(T)}. " +
                                       "Singleton will be created as new GameObject");
                }

                if(go == null) {
                    if(warn)
                        Debug.LogError($"Failed to create gameobject for instance of {typeof(T).Name}. " +
                                       "Please check your memory constraints.");
                    return false;
                }

                go.name = $"{typeof(T).Name} Singleton";
                _instance = go.GetComponent<T>() ?? go.AddComponent<T>();

                if(_instance == null) {
                    if(warn)
                        Debug.LogError($"Failed to add component of {typeof(T).Name} to new gameobject. " +
                                       "Please check your memory constraints.");
                    Destroy(go);
                    return false;
                }
                break;
            }
        }

        if(!destroyOnLoad) DontDestroyOnLoad(_instance.gameObject);

        return true;
    }
}