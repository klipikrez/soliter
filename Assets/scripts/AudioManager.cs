using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    Dictionary<string, AudioClip> audioDictionary = new Dictionary<string, AudioClip>();
    public AudioMixerGroup audioMixer;
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Object[] allAudios = Resources.LoadAll("Audio", typeof(AudioClip));

        foreach (AudioClip clip in allAudios)
        {
            audioDictionary.Add(clip.name, clip);
        }
    }

    public static void Play(string audioClipName, float volume = 1, bool vairyPitch = false)
    {
        if (!Instance.audioDictionary.ContainsKey(audioClipName)) return;
        Instance.StartCoroutine(Instance.Play(Instance.audioDictionary[audioClipName], volume, vairyPitch));
    }
    IEnumerator Play(AudioClip audio, float volume, bool vairyPitch)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audio;
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = audioMixer;
        audioSource.pitch = vairyPitch ? (0.8f + 0.4f * Random.value) : 1;
        //PlayingAudio[id].source = audioSource;
        audioSource.Play();
        yield return new WaitForSeconds(audio.length);
        audioSource.Stop();
        Destroy(audioSource);
        //PlayingAudio.Remove(id);
    }
}
