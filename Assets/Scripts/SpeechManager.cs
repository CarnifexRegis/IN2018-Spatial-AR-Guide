using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using UnityEngine.UI;
using System;

public class SpeechManager : MonoBehaviour
{
    private static SpeechManager _instance;
    private static object _lock = new object();

    /// <summary>
    /// Singleton instance of the SpeechManager.
    /// </summary>
    public static SpeechManager Instance
    {
        get
        {
            lock (_lock)
            {
                if (!_instance)
                {
                    _instance = (SpeechManager)FindObjectOfType(typeof(SpeechManager));
                    if (!_instance)
                    {
                        var singleton = new GameObject { name = "SpeechManager" };
                        _instance = singleton.AddComponent<SpeechManager>();
                        DontDestroyOnLoad(singleton);
                    }
                }
                return _instance;
            }
        }
    }

    public enum VoiceLanguage
    {
        EnglishUS,
        EnglishGB,
        German
    }

    public enum VoiceName
    {
        Aria,
        Guy,
        Zira,
        Benjamin,
        Hazel,
        Susan,
        George,
        Hedda,
    }

    [Header("References")]
    [SerializeField]
    private InputField _inputField;
    [SerializeField]
    private Button _speakButton;
    [SerializeField]
    public AudioSource _audioSource;

    [Header ("Configuration")]
    [SerializeField]
    [Tooltip("Subscription key used to create a speech config instance.")]
    private string _subscriptionKey;
    [SerializeField]
    [Tooltip("Server region used to create a speech config instance (e.g., \"westus\").")]
    private string _serverRegion;
    [SerializeField]
    [Tooltip ("Language used to synthesize audio.")]
    private VoiceLanguage _voiceLanguage;
    [SerializeField]
    [Tooltip("Name tag used to synthesize audio.")]
    private VoiceName _voiceName;

    private object _threadLocker = new object();
    private bool _waitingForSpeak;

    private SpeechConfig _speechConfig;
    private SpeechSynthesizer _synthesizer;

    private readonly int _audioChannelCount = 1;
    private readonly int _audioFrequency = 16000;

    void Start()
    {
        if (!_audioSource)
            throw new Exception("No referenced audio source present for speech!");

        if (string.IsNullOrEmpty(_subscriptionKey) || string.IsNullOrEmpty(_serverRegion))
            throw new Exception("Subscription key and server region have to be filled!");
        _speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _serverRegion);

        // Set output format
        _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw16Khz16BitMonoPcm);

        // Set voice language and name
        if (_voiceLanguage != GetCompatibleLanguage(_voiceName))
            throw new Exception($"Given voice name {_voiceName} is not compatible with language {_voiceLanguage}!");
        string voiceLanguage = ConvertVoiceLanguage(_voiceLanguage);
        string voiceName = $"{voiceLanguage}-{ConvertVoiceName(_voiceName)}";
        _speechConfig.SpeechSynthesisLanguage = voiceLanguage;
        _speechConfig.SpeechSynthesisVoiceName = voiceName;

        _synthesizer = new SpeechSynthesizer(_speechConfig, null);
    }

    void Update()
    {
        lock (_threadLocker)
        {
            if (_speakButton)
                _speakButton.interactable = !_waitingForSpeak;
        }
    }

    void OnDestroy()
    {
        _synthesizer.Dispose();
    }

    /// <summary>
    /// Synthesize audio from the text given in input field.
    /// </summary>
    public void Speak()
    {
        if (_waitingForSpeak)
            return;
        if (!_inputField)
            throw new Exception("No input field is referenced for speech!");
        Speak(_inputField.text);
    }

    /// <summary>
    /// Synthesize audio from the given text.
    /// </summary>
    /// <param name="text"> Text to synthesize audio from. </param>
    public void Speak(string text)
    {
        lock (_threadLocker)
        {
            _waitingForSpeak = true;
        }
        using (var result = _synthesizer.SpeakTextAsync(text).Result)
        {
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
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
                Debug.Log("Speech synthesis succeeded!");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Debug.Log($"CANCELED: Reason=[{cancellation.Reason}] - ErrorDetails=[{cancellation.ErrorDetails}]");
            }
        }
        lock (_threadLocker)
        {
            _waitingForSpeak = false;
        }
    }

    /// <summary>
    /// Converts voice language into string form.
    /// </summary>
    /// <param name="voiceLanguage"> Voice language to convert. </param>
    /// <returns></returns>
    private string ConvertVoiceLanguage(VoiceLanguage voiceLanguage)
    {
        switch (voiceLanguage)
        {
            case VoiceLanguage.EnglishUS: 
                return "en-US";
            case VoiceLanguage.EnglishGB:
                return "en-GB";
            case VoiceLanguage.German: 
                return "de-DE";
            default:
                throw new Exception($"Voice language {voiceLanguage} is not supported!");
        }
    }

    /// <summary>
    /// Converts voice name into string form.
    /// </summary>
    /// <param name="voiceName"> Voice name to convert. </param>
    /// <returns></returns>
    private string ConvertVoiceName(VoiceName voiceName)
    {
        switch (voiceName)
        {
            case VoiceName.Aria:
                return "AriaRUS";
            case VoiceName.Guy:
                return "GuyRUS";
            case VoiceName.Zira:
                return "ZiraRUS";
            case VoiceName.Benjamin:
                return "BenjaminRUS";
            case VoiceName.Hazel:
                return "HazelRUS";
            case VoiceName.Susan:
                return "Susan";
            case VoiceName.George:
                return "George";
            case VoiceName.Hedda:
                return "HeddaRUS";
            default:
                throw new Exception($"Voice name {voiceName} is not supported!");
        }
    }

    /// <summary>
    /// Returns compatible voice language for a given name.
    /// </summary>
    /// <param name="voiceName"> Voice name to check. </param>
    /// <returns></returns>
    private VoiceLanguage GetCompatibleLanguage(VoiceName voiceName)
    {
        return voiceName switch
        {
            VoiceName.Aria => VoiceLanguage.EnglishUS,
            VoiceName.Guy => VoiceLanguage.EnglishUS,
            VoiceName.Zira => VoiceLanguage.EnglishUS,
            VoiceName.Benjamin => VoiceLanguage.EnglishUS,
            VoiceName.Hazel => VoiceLanguage.EnglishGB,
            VoiceName.Susan => VoiceLanguage.EnglishGB,
            VoiceName.George => VoiceLanguage.EnglishGB,
            VoiceName.Hedda => VoiceLanguage.German,
            _ => throw new Exception($"Voice name {voiceName} is not supported!"),
        };
    }
}
