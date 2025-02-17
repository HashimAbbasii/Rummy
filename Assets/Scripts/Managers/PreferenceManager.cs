using UnityEngine;

public static class PreferenceManager
{
    public static string UserID
    {
        get { return PlayerPrefs.GetString("USERID", null); }

        set { PlayerPrefs.SetString("USERID", value); }
    }

    public static string Password
    {
        get { return PlayerPrefs.GetString("PASSWORD", null); }

        set { PlayerPrefs.SetString("PASSWORD", value); }
    }

    public static string Language
    {
        get { return PlayerPrefs.GetString("LANGUAGE", "English"); }

        set { PlayerPrefs.SetString("LANGUAGE", value); }
    }

    public static float Points
    {
        get { return PlayerPrefs.GetFloat("POINTS", 0); }

        set { PlayerPrefs.SetFloat("POINTS", value); }
    }

    public static float Music
    {
        get { return PlayerPrefs.GetFloat("MUSIC", 0.8f); }

        set { PlayerPrefs.SetFloat("MUSIC", value); }
    }

    public static float SFX
    {
        get { return PlayerPrefs.GetFloat("SFX", 0.8f); }

        set { PlayerPrefs.SetFloat("SFX", value); }
    }
}
