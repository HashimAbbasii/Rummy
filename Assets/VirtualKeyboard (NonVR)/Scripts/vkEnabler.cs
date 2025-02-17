using TMPro;
using UnityEngine;

using System.Collections;

public class vkEnabler : MonoBehaviour
{
    public TMP_InputField inputField; // Reference to the input field this script is attached to

    public void OnFieldSelected()
    {
#if UNITY_WEBGL
       
        if (Application.isMobilePlatform)
        {
            // Check the orientation
            if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            {
                Debug.Log("Device is in Portrait mode.");
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                Debug.Log("Device is in Landscape mode.");
            }

            // Show the virtual keyboard
          
            TNVirtualKeyboard.Instance.activeField = inputField;
            StartCoroutine(ShowKeyboardWithDelay()); // ShowKeyboardWithDelay
            //TNVirtualKeyboard.Instance.ShowVirtualKeyboard();
        }
        else
        {
            Debug.Log("Virtual keyboard not shown because it's a desktop WebGL build.");

           

        }
#else
        Debug.Log("Not running on WebGL.");
#endif
    }

    private IEnumerator ShowKeyboardWithDelay()
    {
        Debug.Log("Active field is now: " + inputField.name);
        yield return new WaitForSeconds(0.2f); // Add a small delay
        TNVirtualKeyboard.Instance.ShowVirtualKeyboard();
    }

}
