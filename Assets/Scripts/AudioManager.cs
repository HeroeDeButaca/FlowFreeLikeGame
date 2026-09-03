using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioMixer _audioMixer;

    private AudioMixerGroup _sfxGroup;

    [SerializeField]
    private AudioSource _bgmSource;
    private List<AudioSource> _sfxSources = new List<AudioSource>();
    private const int INITIAL_SFX_POOL_SIZE = 3;

    public static AudioManager Instance;

    void Awake()
    {
        if (Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _sfxGroup = _audioMixer.FindMatchingGroups("SFX")[0];

        for (int i = 0; i < INITIAL_SFX_POOL_SIZE; i++)
        {
            string goName = "SFX_" + (i + 1);
            GenerateSfxSource(goName);
        }
    }

    public void PlayBGM(AudioClip clip, bool activateLoop)
    {
        _bgmSource.clip = clip;
        _bgmSource.loop = activateLoop;

        _bgmSource.Play();
    }

    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float pitch = 1f)
    {
        foreach (AudioSource i in _sfxSources)
        {
            if (!i.isPlaying)
            {
                i.pitch = pitch;
                i.PlayOneShot(clip);
                return;
            }
        }

        // If no AudioSource is available, the function 
        // generates one
        int childCount = transform.childCount;
        string goName = "ExtraSFX_" + childCount.ToString("0");
        GenerateSfxSource(goName);

        // Play the sfx on the new AudioSource
        AudioSource audioSource = transform.GetChild(childCount - 1).GetComponent<AudioSource>();
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip);
    }

    public void ChangeBgmVolume(float decimalVolume)
    {
        float dbVolume = Mathf.Log10(decimalVolume) * 20;
        if (decimalVolume == 0.0f)
            dbVolume = -80.0f;

        _audioMixer.SetFloat("BgmVolume", dbVolume);
    }
    public void ChangeSfxVolume(float decimalVolume)
    {
        float dbVolume = Mathf.Log10(decimalVolume) * 20;
        if (decimalVolume == 0.0f)
            dbVolume = -80.0f;

        _audioMixer.SetFloat("SfxVolume", dbVolume);
    }

    private void GenerateSfxSource(string goName)
    {
        GameObject sfxObj = new GameObject(goName);
        sfxObj.isStatic = true;
        sfxObj.transform.parent = gameObject.transform;
        AudioSource src = sfxObj.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = _sfxGroup;
        src.playOnAwake = false;
        src.spatialBlend = 0;
        _sfxSources.Add(src);
    }
}
