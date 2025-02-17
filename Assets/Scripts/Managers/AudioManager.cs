using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Audio
{
    public AudioName audioName;
    public AudioSource audioSource;
}

public class AudioManager : MonoBehaviour
{
    public List<Audio> audios = new();
    public Dictionary<AudioName, AudioSource> audioReferences = new();

    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        foreach (Audio audio in audios)
        {
            audioReferences.Add(audio.audioName, audio.audioSource);
        }
    }

    public void PlayAudio(AudioName audioName)
    {
        audioReferences?[audioName].Play();
    }

    public void StopAudio(AudioName audioName)
    {
        audioReferences?[audioName].Stop();
    }
}

public enum AudioName
{
    ButtonAudio,
    TouchTileAudio,
    CountDownAudio,
    DrawTileAudio,
    MadeSetAudio,
    MovedTileOnTableAudio,
    MovedTileOnTableFromRackAudio,
    WrongMoveAudio,
    TimerCompletedAudio,
    FirstTileDistributeAudio,
    DistributeTileAudio
}
