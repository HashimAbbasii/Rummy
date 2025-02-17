using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarksAssets.FullscreenWebGL;
using status = MarksAssets.FullscreenWebGL.FullscreenWebGL.status;
using navigationUI = MarksAssets.FullscreenWebGL.FullscreenWebGL.navigationUI;

public class FullscreenWebGLManager : MonoBehaviour
{
    void Start()
    {
        if (FullscreenWebGL.isFullscreenSupported())
        {           
        }
    }
    public void IsFullScreen(bool toggle)
    {
        var fss = FindObjectsOfType<FullscreenWebGLSettings>(true);
        foreach (var s in fss)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
               // if (CanvasHandler.Instance.IsRunningOnAndroid())
                {
                    //s.exitFullscreenBtn.SetActive(false);
                    //s.exitFullscreenToggle.enabled = !toggle;
                }
            }

        }
    }

    //call this on a pointerdown event
    public void EnterFullscreen()
    {
        var fss = FindObjectsOfType<FullscreenWebGLSettings>(true);
        foreach (var s in fss)
        {
            s.EnterFullscreen();
        }
    }

    public void ExitFullscreen()
    {
        var fss = FindObjectsOfType<FullscreenWebGLSettings>(true);
        foreach (var s in fss)
        {
            s.ExitFullscreen();
        }
    }
}
