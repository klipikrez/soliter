using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    Dictionary<string, AudioClip> audioDictionary = new Dictionary<string, AudioClip>();
    List<AudioClip> musics = new List<AudioClip>();
    public AudioMixerGroup masterAudioMixer;
    public AudioMixerGroup musicAudioMixer;
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("AudioManager Awake");
        Instance = this;
        Object[] allAudios = Resources.LoadAll("Audio", typeof(AudioClip));
        Object[] allMusic = Resources.LoadAll("Music", typeof(AudioClip));

        foreach (AudioClip clip in allAudios)
        {
            audioDictionary.Add(clip.name, clip);
        }

        foreach (AudioClip clip in allMusic)
        {
            musics.Add(clip);
        }

        Instance.StartCoroutine(PlaySong(RandomRangeExcept(0, musics.Count, -52)));
    }

    public static void Play(string audioClipName, float volume = 1, bool vairyPitch = false)
    {
        if (!Instance.audioDictionary.ContainsKey(audioClipName)) return;
        Instance.StartCoroutine(Instance.Play(Instance.audioDictionary[audioClipName], volume, vairyPitch));
    }
    IEnumerator Play(AudioClip audio, float volume, bool vairyPitch, AudioMixerGroup audioMixer = null)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audio;
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = audioMixer ?? masterAudioMixer;
        audioSource.pitch = vairyPitch ? (0.8f + 0.4f * Random.value) : 1;
        //PlayingAudio[id].source = audioSource;
        audioSource.Play();
        yield return new WaitForSeconds(audio.length);
        audioSource.Stop();
        Destroy(audioSource);
        //PlayingAudio.Remove(id);
    }

    IEnumerator PlaySong(int index)
    {
        Debug.Log("Playing song: " + musics[index].name);
        Instance.StartCoroutine(Instance.Play(musics[index], 1, false, musicAudioMixer));
        float time = musics[index].length;
        yield return new WaitForSeconds(time);
        Instance.StartCoroutine(PlaySong(RandomRangeExcept(0, musics.Count, index)));
    }

    int RandomRangeExcept(int min, int max, int except)
    {
        int number = -52;
        do
        {
            number = Random.Range(min, max);
        } while (number == except);
        return number;
    }

}
