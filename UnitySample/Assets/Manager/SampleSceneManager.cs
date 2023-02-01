using Unity.Netcode;
using UnityEngine;

public class SampleSceneManager : MonoBehaviour
{

    private bool isConnected = false;

    private  void Update() {
        if (isConnected) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                isConnected = false;
                NetworkManager.Singleton.Shutdown();
            }
        } else {
            if (Input.GetKeyDown(KeyCode.S)) {
                isConnected = true;
                NetworkManager.Singleton.StartServer();
            } else if (Input.GetKeyDown(KeyCode.H)) {
                isConnected = true;
                NetworkManager.Singleton.StartHost();
            } else if (Input.GetKeyDown(KeyCode.C)) {
                isConnected = true;
                NetworkManager.Singleton.StartClient();
            }
        }
    }

}
