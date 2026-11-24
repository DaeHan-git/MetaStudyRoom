using UnityEngine;

public class KeepRootPersistent : MonoBehaviour
{
    private static KeepRootPersistent instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}