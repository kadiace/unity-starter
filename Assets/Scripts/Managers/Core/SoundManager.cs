using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    private readonly Dictionary<Define.Sound, AudioClip> _audioClips = new();

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private float _masterVolume = 1f;
    private float _bgmVolume = 1f;
    private float _sfxVolume = 1f;

    public float MasterVolume => _masterVolume;
    public float BgmVolume => _bgmVolume;
    public float SfxVolume => _sfxVolume;

    public void Init(Transform appRoot)
    {
        GameObject soundRoot = new("@Sound");
        soundRoot.transform.SetParent(appRoot, false);

        _bgmSource = CreateAudioSource(soundRoot.transform, "Bgm", loop: true);
        _sfxSource = CreateAudioSource(soundRoot.transform, "Sfx", loop: false);
    }

    public void PlayBgm(Define.Sound sound)
    {
        AudioClip clip = GetOrLoadClip(sound);
        _bgmSource.clip = clip;
        ApplyVolumes();
        _bgmSource.Play();
    }

    public void PlaySfx(Define.Sound sound)
    {
        AudioClip clip = GetOrLoadClip(sound);
        _sfxSource.PlayOneShot(clip, GetEffectiveSfxVolume());
    }

    public void StopBgm()
    {
        _bgmSource.Stop();
        _bgmSource.clip = null;
    }

    public void StopAll()
    {
        StopBgm();
        _sfxSource.Stop();
    }

    public void SetBgmVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void Clear()
    {
        StopAll();
        _audioClips.Clear();
    }

    private static AudioSource CreateAudioSource(Transform parent, string name, bool loop)
    {
        GameObject sourceObject = new(name);
        sourceObject.transform.SetParent(parent, false);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        return source;
    }

    private AudioClip GetOrLoadClip(Define.Sound sound)
    {
        if (_audioClips.TryGetValue(sound, out AudioClip clip))
            return clip;

        string resourcePath = GetSoundPath(sound);
        clip = Managers.Resource.Load<AudioClip>(resourcePath);
        if (clip == null)
            throw new InvalidOperationException($"AudioClip not found: Resources/{resourcePath}");

        _audioClips.Add(sound, clip);
        return clip;
    }

    private static string GetSoundPath(Define.Sound sound)
    {
        return $"Sounds/{sound}";
    }

    private void ApplyVolumes()
    {
        _bgmSource.volume = GetEffectiveBgmVolume();
        _sfxSource.volume = GetEffectiveSfxVolume();
    }

    private float GetEffectiveBgmVolume()
    {
        return _masterVolume * _bgmVolume;
    }

    private float GetEffectiveSfxVolume()
    {
        return _masterVolume * _sfxVolume;
    }
}
