using RTLTMPro;
using TMPro;
using UnityEngine;

public class TranslateText : MonoBehaviour
{
    [TextArea] public string englishText;
    [TextArea] public string hebrewText;

    private void OnEnable()
    {
        Response();
    }

    private void Start()
    {
        GameManager.Instance.languageSwitch.AddListener(Response);
    }

    private void Response()
    {
        switch (PreferenceManager.Language)
        {
            case "English":
            {
                if (GetComponent<RTLTextMeshPro>().alignment != TextAlignmentOptions.Center)
                    GetComponent<RTLTextMeshPro>().alignment = TextAlignmentOptions.Left;

                GetComponent<TextMeshProUGUI>().text = englishText;
                break;
            }
            case "Hebrew":
            {
                if (GetComponent<RTLTextMeshPro>().alignment != TextAlignmentOptions.Center)
                    GetComponent<RTLTextMeshPro>().alignment = TextAlignmentOptions.Right;

                GetComponent<TextMeshProUGUI>().text = hebrewText;
                break;
            }
        }
    }
}


