using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Komponen Audio")]
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    [Header("Latar Musik (BGM)")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Efek Suara (SFX)")]
    public AudioClip captureSfx;
    public AudioClip winSfx;
    public AudioClip loseSfx;
    public AudioClip drawSfx;
    public AudioClip promotionSfx;
    public AudioClip jumpSfx;
    public AudioClip checkSfx;

    private void Awake()
    {
        // Pastikan hanya ada satu MusicManager di dalam game (Singleton)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Jangan hancurkan objek ini saat pindah dari Menu ke Game
        DontDestroyOnLoad(gameObject);

        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        
        bgmSource.loop = true; // BGM harus selalu looping
        bgmSource.playOnAwake = false;

        // Buat AudioSource kedua khusus untuk SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    // ================== BGM CONTROLS ==================

    public void PlayMenuMusic()
    {
        if (menuMusic == null) return;
        
        if (bgmSource.clip == menuMusic && bgmSource.isPlaying) return;

        bgmSource.clip = menuMusic;
        bgmSource.Play();
    }

    public void PlayGameMusic()
    {
        if (gameMusic == null) return;
        
        if (bgmSource.clip == gameMusic && bgmSource.isPlaying) return;

        bgmSource.clip = gameMusic;
        bgmSource.Play();
    }

    public void StopMusic()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    // ================== SFX CONTROLS ==================

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayCapture()
    {
        PlaySFX(captureSfx);
    }

    public void PlayWin()
    {
        PlaySFX(winSfx);
    }

    public void PlayLose()
    {
        PlaySFX(loseSfx);
    }

    public void PlayDraw()
    {
        PlaySFX(drawSfx);
    }

    public void PlayPromotion()
    {
        PlaySFX(promotionSfx);
    }

    public void PlayJump()
    {
        PlaySFX(jumpSfx);
    }

    public void PlayCheck()
    {
        PlaySFX(checkSfx);
    }
}
