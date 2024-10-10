using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource soundSource;
    public Toggle musicToggle;
    public Toggle soundToggle;
    public AudioClip[] soundEffects;
    public string[] soundEffectNames;

    private void Start()
    {
        LoadAudioSettings();
        musicToggle.onValueChanged.AddListener(delegate { ToggleMusic(musicToggle.isOn); });
        soundToggle.onValueChanged.AddListener(delegate { ToggleSound(soundToggle.isOn); });
    }

    public void ToggleMusic(bool isOn)
    {
        musicSource.mute = !isOn;
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
    }

    public void ToggleSound(bool isOn)
    {
        soundSource.mute = !isOn;
        PlayerPrefs.SetInt("SoundEnabled", isOn ? 1 : 0);
    }

    private void LoadAudioSettings()
    {
        if (PlayerPrefs.HasKey("MusicEnabled"))
        {
            bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled") == 1;
            musicToggle.isOn = musicEnabled;
            musicSource.mute = !musicEnabled;
        }

        if (PlayerPrefs.HasKey("SoundEnabled"))
        {
            bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled") == 1;
            soundToggle.isOn = soundEnabled;
            soundSource.mute = !soundEnabled;
        }
    }

    public void PlaySoundByIndex(int index)
    {
        if (!soundSource.mute && index >= 0 && index < soundEffects.Length)
        {
            soundSource.PlayOneShot(soundEffects[index]);
        }
    }

    public void PlaySoundByName(string soundName)
    {
        if (!soundSource.mute)
        {
            int index = System.Array.IndexOf(soundEffectNames, soundName);
            if (index != -1 && index < soundEffects.Length)
            {
                soundSource.PlayOneShot(soundEffects[index]);
            }
        }
    }
}
