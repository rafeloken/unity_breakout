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
        if(instance == null) {
            Debug.Log("[UnitySingleton] Initializing Singleton in Awake.");
            instance = this as T;
            instance.Init();
            if(persist)
                DontDestroyOnLoad(gameObject);
        } else if(instance != this) {
            // This check is to eliminate duplicates that may be created by persisting 
            // game objects across scenes, but also having said object be in each scene.
            // An example would be if you have a 'GameManager' and want to be
            // able to to start the game from any scene, therefore you have an object
            // with 'GameManager' in each scene.  This will make sure no matter which
            // scene in the game you start at there will only be the original 'GameManager'.
            print("[UnitySingleton] Destroying duplicate.");
            Destroy(gameObject);            
        }
    }

    // Override this in child classes rather than Awake().
    virtual protected void Init() { }

    // Make sure no "ghost" objects are left behind when applicaton quits.
    void OnApplicationQuit() {
        instance = null;
    }
}
