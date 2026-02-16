using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Ses yönetim sistemi (Singleton Pattern)
/// Tüm oyun seslerini ve müzik kontrol eder.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundGroup
    {
        public string soundName;
        [Tooltip("Bu ses grubuna ait tüm varyantlar")]
        public List<AudioClip> clips = new List<AudioClip>();
        public float volume = 1f;
        [Range(0f, 1f)] public float pitchVariation = 0.1f; // Pitch değişim miktarı
    }

    [System.Serializable]
    public class BackgroundMusicClip
    {
        public AudioClip clip;
        [Tooltip("Müzik bittikten sonra sonraki müziğe geçmeden önce bekleme süresi (saniye)")] 
        public float delayBetweenTracks = 0f;
    }

    [SerializeField] private List<SoundGroup> soundEffects = new List<SoundGroup>();
    [SerializeField] private List<BackgroundMusicClip> backgroundMusicList = new List<BackgroundMusicClip>();
    [SerializeField] private float backgroundMusicVolume = 0.5f;
    [SerializeField] private bool randomizeMusic = false; // Müzikleri rastgele çal

    private AudioSource sfxAudioSource;
    private AudioSource musicAudioSource;
    private bool isMusicPlaying = false;
    private int currentMusicIndex = 0;
    private Coroutine musicPlaybackCoroutine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource componentleri oluştur
        CreateAudioSources();
    }

    private void CreateAudioSources()
    {
        // Ses efektleri için AudioSource
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.volume = 1f;

        // Müzik için AudioSource
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = false; // Loop'u kapat, biz yönetiriz
        musicAudioSource.volume = backgroundMusicVolume;
    }

    /// <summary>
    /// Ses efekti oynat (rastgele varyant seçer)
    /// </summary>
    public void PlaySound(string soundName)
    {
        SoundGroup soundGroup = soundEffects.Find(s => s.soundName == soundName);

        if (soundGroup == null || soundGroup.clips.Count == 0)
        {
            Debug.LogWarning($"[AudioManager] Ses bulunamadı veya hiç varyantı yok: {soundName}");
            return;
        }

        // Rastgele bir varyant seç
        AudioClip selectedClip = soundGroup.clips[Random.Range(0, soundGroup.clips.Count)];

        if (selectedClip == null)
        {
            Debug.LogWarning($"[AudioManager] {soundName} grubu içinde null clip!");
            return;
        }

        // Pitch'e hafif rastgelelik ekle (daha doğal ses)
        float randomPitch = 1f + Random.Range(-soundGroup.pitchVariation, soundGroup.pitchVariation);
        sfxAudioSource.pitch = randomPitch;
        sfxAudioSource.PlayOneShot(selectedClip, soundGroup.volume);
    }

    /// <summary>
    /// Arka plan müzik başlat
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusicList.Count == 0)
        {
            Debug.LogWarning("[AudioManager] Arka plan müzik listesi boş!");
            return;
        }

        // Eğer zaten bir müzik çalıyorsa coroutine'i durdurma
        if (isMusicPlaying) return;

        // Müzik playback coroutine'ini başlat
        isMusicPlaying = true;
        currentMusicIndex = 0;
        
        if (musicPlaybackCoroutine != null)
            StopCoroutine(musicPlaybackCoroutine);
        
        musicPlaybackCoroutine = StartCoroutine(PlayMusicSequence());
    }

    /// <summary>
    /// Müzikleri sırayla oynat
    /// </summary>
    private IEnumerator PlayMusicSequence()
    {
        while (isMusicPlaying)
        {
            // Müzik indeksini belirle
            int musicIndex = randomizeMusic ? Random.Range(0, backgroundMusicList.Count) : currentMusicIndex;
            
            if (musicIndex >= backgroundMusicList.Count)
            {
                musicIndex = 0; // Başa dön
                currentMusicIndex = 0;
            }

            BackgroundMusicClip musicClip = backgroundMusicList[musicIndex];

            if (musicClip.clip == null)
            {
                Debug.LogWarning($"[AudioManager] Müzik {musicIndex} null!");
                currentMusicIndex++;
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // Müziği oynat
            musicAudioSource.clip = musicClip.clip;
            musicAudioSource.Play();

            // Müziğin bitişini bekle
            float musicDuration = musicClip.clip.length;
            yield return new WaitForSeconds(musicDuration);

            // Bekleme süresini bekle
            if (musicClip.delayBetweenTracks > 0)
            {
                yield return new WaitForSeconds(musicClip.delayBetweenTracks);
            }

            // Sonraki müziğe geç
            currentMusicIndex++;
        }
    }

    /// <summary>
    /// Arka plan müzik durdur
    /// </summary>
    public void StopBackgroundMusic()
    {
        isMusicPlaying = false;
        musicAudioSource.Stop();
        
        if (musicPlaybackCoroutine != null)
        {
            StopCoroutine(musicPlaybackCoroutine);
            musicPlaybackCoroutine = null;
        }
    }

    /// <summary>
    /// Belirli bir müziği oynat (index ile)
    /// </summary>
    public void PlayMusicByIndex(int index)
    {
        if (index < 0 || index >= backgroundMusicList.Count)
        {
            Debug.LogWarning($"[AudioManager] Geçersiz müzik indeksi: {index}");
            return;
        }

        StopBackgroundMusic();
        currentMusicIndex = index;
        PlayBackgroundMusic();
    }

    /// <summary>
    /// Rastgele müzik modu aç/kapat
    /// </summary>
    public void SetRandomizeMusic(bool randomize)
    {
        randomizeMusic = randomize;
    }

    /// <summary>
    /// Arka plan müzik seviyesi ayarla
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicAudioSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Ses efektleri seviyesi ayarla
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxAudioSource.volume = Mathf.Clamp01(volume);
    }
}
