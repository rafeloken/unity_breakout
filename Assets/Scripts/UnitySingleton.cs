using UnityEngine;

// Abstract singleton class to inherit various managers from.
public abstract class UnitySingleton<T> : MonoBehaviour where T : UnitySingleton<T> {
    static T instance;

    // Whether or not this object should persist when loading new scenes.
    // This should be set in the child classes Init() method.
    bool persist = false;

    public static T Instance {
        get {
            // This would only ever be null if some other MonoBehavior requests the instance
            // in its' Awake method.
            if(instance == null) {
                Debug.Log("[UnitySingleton] Finding instance of '" + typeof(T).ToString() + 
                          "' object.");
                instance = FindObjectOfType(typeof(T)) as T;
                // This should only occur if 'T' hasn't been attached to any game
                // objects in the scene.
                if(instance == null) {
                    Debug.LogError("[UnitySingleton] No instance of " + typeof(T).ToString()
                                   + "found!");
                    return null;
                }
                instance.Init();
            }
            return instance;
        }
    }

#region Basic getters/setters
    public bool Persist {
        get { return persist; }
        protected set { persist = value; }
    }
#endregion

    // This will initialize our instance, if it hasn't already been prompted to do so by
    // another MonoBehavior's Awake() requesting it first.
    void Awake() {
        Debug.Log("[UnitySingleton] Awake");
        if(instance == null) {
            Debug.Log("[UnitySingleton] Initializing Singleton in Awake");
            instance = this as T;
            instance.Init();
            if(persist)
                DontDestroyOnLoad(gameObject);
        }
    }

    // Override this in child classes rather than Awake().
    virtual protected void Init() { }

    // Make sure no "ghost" objects are left behind when applicaton quits.
    void OnApplicationQuit() {
        instance = null;
    }
}
