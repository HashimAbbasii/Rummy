using UnityEngine;

public class WebGLOrientationLock : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Calling startGame from Unity...");
#if UNITY_WEBGL && !UNITY_EDITOR
        // Notify JavaScript to manage orientation and fullscreen
        Application.ExternalCall("startGame");
#endif
    }
}
