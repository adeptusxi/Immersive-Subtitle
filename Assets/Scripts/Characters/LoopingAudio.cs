using UnityEngine;

public sealed class LoopingAudio
{
    private readonly AudioSource source;
    private readonly float fadeSeconds;
    private readonly float fullVolume;
    private bool isPaused;
    private bool resumeOnUnpause;

    public LoopingAudio(AudioSource source, float fadeSeconds)
    {
        this.source = source;
        this.fadeSeconds = fadeSeconds;
        fullVolume = source.volume;

        ConfigureSpatial(source);
        source.loop = true;
        source.volume = 0f;
    }

    public static void ConfigureSpatial(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialize = true;
        source.spatialBlend = 1f;
        source.dopplerLevel = 0f;
    }

    public void Tick(bool wanted, bool paused, float deltaTime)
    {
        if (paused != isPaused)
        {
            isPaused = paused;
            if (paused)
            {
                resumeOnUnpause = source.isPlaying;
                if (resumeOnUnpause)
                {
                    source.Pause();
                }
            }
            else if (resumeOnUnpause)
            {
                source.UnPause();
            }
        }

        if (paused)
        {
            return;
        }

        if (wanted && !source.isPlaying)
        {
            source.Play();
        }

        if (!source.isPlaying)
        {
            return;
        }

        float target = wanted ? fullVolume : 0f;
        source.volume = Mathf.MoveTowards(source.volume, target, fullVolume * deltaTime / fadeSeconds);

        if (!wanted && source.volume <= 0f)
        {
            source.Stop();
        }
    }
}
