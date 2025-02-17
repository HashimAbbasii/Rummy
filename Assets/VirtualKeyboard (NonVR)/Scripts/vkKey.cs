using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vkKey : MonoBehaviour
{

    public string k = "xyz"; // Default key value, set this for each button in Inspector

    public void KeyClick()
    {
        if (!string.IsNullOrEmpty(k))
        {
            TNVirtualKeyboard.Instance.KeyPress(k);
            Debug.Log("Key pressed: " + k);
        }
        else
        {
            Debug.LogWarning("Key value is empty!");
        }
    }
}
