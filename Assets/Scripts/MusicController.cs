using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public GameData CurrentGame;
    public LocalOptions Options;
    public List<AudioSource> Musics;
    public int InGameMusic;
    public int EndRound;
    private float basedVolume;
    private int lastVolumeUp;
    private AudioSource currentMusic;

    private void Start()
    {
        if (Musics.Count > 0)
            basedVolume = Musics[0].volume;

        lastVolumeUp = 100;
    }

    private void StartInGame()
    {
        StartMusic(Musics[InGameMusic]);
    }
    private void StartEndRound()
    {
        StartMusic(Musics[EndRound]);
    }

    private void StartMusic(AudioSource music)
    {
        currentMusic.Stop();
        music.Play();
        currentMusic = music;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastVolumeUp != Options.MusicSound)
        {
            foreach (AudioSource music in Musics)
            {
                music.volume = basedVolume * Options.MusicSound / 100f;
            }
            lastVolumeUp = Options.MusicSound;
        }

        if (CurrentGame.GameStarted.Value && !CurrentGame.GameEnded.Value && !Musics[InGameMusic].Equals(currentMusic))
        {
            if (currentMusic != null)
                currentMusic.Stop();
            Invoke("StartInGame", 3.3f);
            currentMusic = Musics[InGameMusic];
        }

        if (CurrentGame.GameEnded.Value && !Musics[EndRound].Equals(currentMusic))
        {
            StartEndRound();
        }

    }
}
