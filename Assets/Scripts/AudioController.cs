using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioSet: System.Object {
    public string SetName;
    public AudioClip SetAudio;
}

public class AudioController : MonoBehaviour
{
    public List<AudioSet> AudioSets;
    private AudioSource _audioSource;

    private void Start() {
        _audioSource = GetComponent<AudioSource>();
    }

    public void SwitchAudioToSet(string setName) {
        var audioClip = AudioSets.Find(audio => audio.SetName == setName).SetAudio;
        _audioSource.clip = audioClip;
        _audioSource.Play();
    }

}
