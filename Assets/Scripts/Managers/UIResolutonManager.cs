using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


public class UIResolutonManager : MonoBehaviour
{
    // [SerializeField] public FullscreenWebGLManager fullScreenWebGLManager;
    [SerializeField] public TNVirtualKeyboard tNVirtualKeyboard;
    // Start is called before the first frame update
    void Start()
    {
        tNVirtualKeyboard = TNVirtualKeyboard.FindObjectOfType<TNVirtualKeyboard>();
    }

    // Update is called once per frame
    void Update()
    {
        float ratio = (Screen.width * 1f / Screen.height);
        if (IsRunningOnAndroid() || IsRunningOniOS())
        {
            if (ratio < 1 || ratio >= 2)
            {
                //  fullScreenWebGLManager.EnterFullscreen();
            }
        }
       CheckForOutsideClick();
    }

    public bool IsRunningOnAndroid()
    {
        return SystemInfo.operatingSystem.ToLower().Contains("android");

    }


    public bool IsRunningOniOS()
    {
        return SystemInfo.operatingSystem.ToLower().Contains("iphone") ||
               SystemInfo.operatingSystem.ToLower().Contains("ipad") ||
               SystemInfo.operatingSystem.ToLower().Contains("ios");
    }

    private bool IsPointerOverUIObject()
    {
        // Check if the pointer is over a UI element
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void CheckForOutsideClick()
    {
        Debug.Log("Checking for outside click");
        if (Input.GetMouseButtonDown(0)) // Detect left mouse click or touch
        {
            if (!IsPointerOverUIObject() &&tNVirtualKeyboard.vkCanvas.activeSelf) // Check if click is outside UI
            {
                Debug.Log("Click on UI");
                tNVirtualKeyboard.HideVirtualKeyboard();
            }
        }
    }



}
