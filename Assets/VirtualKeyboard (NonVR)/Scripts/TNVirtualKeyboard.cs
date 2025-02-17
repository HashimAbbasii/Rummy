using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TNVirtualKeyboard : MonoBehaviour
{
    public static TNVirtualKeyboard Instance;

    public TextMeshProUGUI textDisplay; // Temporary text display
    public TMP_InputField activeField; // Tracks the currently active input field
    public GameObject vkCanvas;        // Virtual keyboard canvas

    private Coroutine hideTextCoroutine;

    void Start()
    {
        Instance = this;
        HideVirtualKeyboard(); // Hide the virtual keyboard on start
    }

    // Updates text in the active input field
    public void KeyPress(string k)
    {
        if (activeField != null)
        {
            // Append the pressed key to the current text
            activeField.text += k;

            // Update temporary text display
            textDisplay.text = activeField.text;

            // Reset the hide coroutine
            if (hideTextCoroutine != null)
            {
                StopCoroutine(hideTextCoroutine);
            }
            hideTextCoroutine = StartCoroutine(HideTextAfterDelay());
        }
        else
        {
            Debug.LogWarning("No active input field found");
        }
    }


    public void Del()
    {
        if (activeField != null && activeField.text.Length > 0)
        {
            // Remove the last character from the active field's text
            activeField.text = activeField.text.Remove(activeField.text.Length - 1, 1);

            // Update the temporary text display
            textDisplay.text = activeField.text;

            Debug.Log("Deleted last character. Remaining text: " + activeField.text);
        }
        else
        {
            Debug.LogWarning("No text to delete or no active input field set.");
        }
    }

    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Wait for 3 seconds
        textDisplay.text = ""; // Clear the temporary display
    }

    public void ShowVirtualKeyboard()
    {
        vkCanvas.SetActive(true); // Show the keyboard
    }

    public void HideVirtualKeyboard()
    {
        vkCanvas.SetActive(false); // Hide the keyboard
    }

    public void SetActiveField(TMP_InputField inputField)
    {
        activeField = inputField; // Set the active input field
    }
}
