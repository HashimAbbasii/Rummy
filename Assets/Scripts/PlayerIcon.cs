using Photon.Pun;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIcon : MonoBehaviour
{
    public string playerNickName;
    public int tilesCount;
    private bool turnEndAudioPlayed;

    [Header("Icon")]
    public GameObject gameIcon;
    public RTLTextMeshPro playerNameText;
    public TextMeshProUGUI tilesText;
    public TextMeshProUGUI positionText;
    public Image avatarImage;
    public Image timerImage;
    public GameObject chatMessage;
    public RTLTextMeshPro chatMessageText;

    [Header("Waiting")]
    public GameObject waitingIcon;

    private void OnEnable()
    {
        tilesCount = 0;
        timerImage.fillAmount = 0;
        HideMessage();
    }

    private void Update()
    {
        tilesText.text = tilesCount.ToString();
        
        if (PhotonNetwork.NickName == playerNickName)
        {
            float lastAudioRange = 1 - (1 / GameManager.Instance.room.standByTime);

            //Play timer audio if timer fill amount is less than 75% 
            if (timerImage.fillAmount > 0.75f && timerImage.fillAmount < lastAudioRange && !AudioManager.Instance.audioReferences[AudioName.CountDownAudio].isPlaying)
            {
                AudioManager.Instance.PlayAudio(AudioName.CountDownAudio);
            }
            else if ((timerImage.fillAmount <= 0.75f || timerImage.fillAmount >= lastAudioRange) && AudioManager.Instance.audioReferences[AudioName.CountDownAudio].isPlaying)
            {
                AudioManager.Instance.StopAudio(AudioName.CountDownAudio);
            }

            if (timerImage.fillAmount == 0)
            {
                turnEndAudioPlayed = false;
            }

            if (!turnEndAudioPlayed && timerImage.fillAmount == 1 && !AudioManager.Instance.audioReferences[AudioName.TimerCompletedAudio].isPlaying)
            {
                turnEndAudioPlayed = true;
                AudioManager.Instance.PlayAudio(AudioName.TimerCompletedAudio);
            }
        }
    }

    public void DisplayMessage(string message)
    {
        chatMessage.SetActive(true);
        chatMessageText.text = message;
        Invoke(nameof(HideMessage), 5f);
    }

    private void HideMessage()
    {
        chatMessage.SetActive(false);
    }

    private void OnDisable()
    {
        AudioManager.Instance.StopAudio(AudioName.CountDownAudio);
    }
}
