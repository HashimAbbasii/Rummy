using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarksAssets.FullscreenWebGL;
using status = MarksAssets.FullscreenWebGL.FullscreenWebGL.status;
using navigationUI = MarksAssets.FullscreenWebGL.FullscreenWebGL.navigationUI;
using UnityEngine.UI;

public class FullscreenWebGLSettings : MonoBehaviour
{
    public GameObject enterFullscreenBtn;
    public GameObject exitFullscreenBtn;
    public Toggle exitFullscreenToggle;
    public RectTransform fullScreenToggleSlider;

    //void Start()
    //{
    //    if (FullscreenWebGL.isFullscreenSupported())
    //    {
    //        FullscreenWebGL.subscribeToFullscreenchangedEvent();
    //        FullscreenWebGL.onfullscreenchange += () =>
    //        {
    //            if (FullscreenWebGL.isFullscreen())
    //            {
    //                enterFullscreenBtn.SetActive(false);
    //                exitFullscreenBtn.SetActive(true);
    //            }
    //            else
    //            {
    //                enterFullscreenBtn.SetActive(true);
    //                exitFullscreenBtn.SetActive(false);
    //            }
    //        };
    //    }
    //}

    //call this on a pointerdown event
    public void EnterFullscreen()
    {
        FullscreenWebGL.requestFullscreen(stat =>
        {
            if (stat == status.Success)
            {
                enterFullscreenBtn.SetActive(false);
                exitFullscreenBtn.SetActive(true);
            }
        }, navigationUI.hide);
    }

    public void ExitFullscreen()
    {
        FullscreenWebGL.exitFullscreen(stat =>
        {
            if (stat == status.Success)
            {
                enterFullscreenBtn.SetActive(true);
                exitFullscreenBtn.SetActive(false);
            }
        });
    }
}
