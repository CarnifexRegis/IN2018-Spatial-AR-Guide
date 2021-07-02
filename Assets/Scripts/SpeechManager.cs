using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using UnityEngine.UI;

public class SpeechManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Subscription key used to create a speech config instance.")]
    private string _subscriptionKey;
    [SerializeField]
    [Tooltip("Server region used to create a speech config instance (e.g., \"westus\").")]
    private string _serverRegion;

    [SerializeField]
    private InputField _inputField;
    [SerializeField]
    private Button _speakButton;
    [SerializeField]
    private AudioSource _audioSource;

    private object _threadLocker = new object();
    private bool _waitingForSpeak;

    private SpeechConfig _speechConfig;
    private SpeechSynthesizer _synthesizer;

    private readonly int _audioChannelCount = 1;
    private readonly int _audioFrequency = 16000;

    public void Speak()
    {
        lock (_threadLocker)
        {
            _waitingForSpeak = true;
        }

        using (var result = _synthesizer.SpeakTextAsync(_inputField.text).Result)
        {
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                // Native playback is not supported on Unity yet (currently only supported on Windows/Linux Desktop).
                // Use the Unity API to play audio here as a short term solution.
                // Native playback support will be added in the future release.
                var sampleCount = result.AudioData.Length / 2;
                var audioData = new float[sampleCount];
                for (var i = 0; i < sampleCount; ++i)
                {
                    audioData[i] = (short)(result.AudioData[i * 2 + 1] << 8 | result.AudioData[i * 2]) / 32768.0F;
                }

                var audioClip = AudioClip.Create("SynthesizedAudio", sampleCount, _audioChannelCount, _audioFrequency, false);
                audioClip.SetData(audioData, 0);
                _audioSource.clip = audioClip;
                _audioSource.Play();

                Debug.Log( "Speech synthesis succeeded!");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Debug.Log($"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?");
            }
        }

        lock (_threadLocker)
        {
            _waitingForSpeak = false;
        }
    }

    void Start()
    {
        _speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _serverRegion);

        // The default format is Riff16Khz16BitMonoPcm.
        // We are playing the audio in memory as audio clip, which doesn't require riff header.
        // So we need to set the format to Raw16Khz16BitMonoPcm.
        _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw16Khz16BitMonoPcm);

        // Max profanity needed
        _speechConfig.SetProfanity(ProfanityOption.Raw);

        _synthesizer = new SpeechSynthesizer(_speechConfig, null);
    }

    void Update()
    {
        lock (_threadLocker)
        {
            _speakButton.interactable = !_waitingForSpeak;
        }
    }

    void OnDestroy()
    {
        _synthesizer.Dispose();
    }
}
