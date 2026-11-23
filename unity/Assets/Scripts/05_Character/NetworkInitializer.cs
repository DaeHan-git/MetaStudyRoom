using Unity.Netcode;
using UnityEngine;

public class Initialize : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartHost();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
