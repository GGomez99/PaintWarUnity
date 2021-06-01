using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

[XmlInclude(typeof(PlayerOptions))]
[System.Serializable]
public class PlayerOptions
{
    public int FXSound;
    public int MusicSound;

    PlayerOptions()
    {

    }

    public PlayerOptions(int fx = 20, int music = 20)
    {
        FXSound = fx;
        MusicSound = music;
    }
}

[XmlInclude(typeof(GameRoundOptions))]
[System.Serializable]
public class GameRoundOptions
{
    public int MaxCanvasX;
    public int MaxCanvasY;
    public float MinimalDrawingLength;
    public float BaseSpeedFillingPerSecond;
    public float BaseInk;
    public float InkPerTeamPixel;
    public float BaseInkRegenPerSecond;
    public float InkRegenPerTeamPixel;
    public float InkToAreaPaintRatio;
    public int RoundTime;

    GameRoundOptions()
    {

    }

    public GameRoundOptions(int MaxCanvasX = 15,
        int MaxCanvasY = 15,
        float MinimalDrawingLength = 1f,
        float BaseSpeedFillingPerSecond = 6f,
        float BaseInk = 150f,
        float InkPerTeamPixel = 0.05f,
        float BaseInkRegenPerSecond = 20f,
        float InkRegenPerTeamPixel = 0.003f,
        float InkToAreaPaintRatio = 0.06f,
        int RoundTime = 300)
    {
        this.MaxCanvasX = MaxCanvasX;
        this.MaxCanvasY = MaxCanvasY;
        this.MinimalDrawingLength = MinimalDrawingLength;
        this.BaseSpeedFillingPerSecond = BaseSpeedFillingPerSecond;
        this.BaseInk = BaseInk;
        this.InkPerTeamPixel = InkPerTeamPixel;
        this.BaseInkRegenPerSecond = BaseInkRegenPerSecond;
        this.InkRegenPerTeamPixel = InkRegenPerTeamPixel;
        this.InkToAreaPaintRatio = InkToAreaPaintRatio;
        this.RoundTime = RoundTime;
    }
}



public class OptionsFileManager : MonoBehaviour
{
    public GameData CurrentGameData;
    public LocalOptions CurrentLocalOptions;

    public string PlayerOptionFilename = "options.dat";
    public string GameRoundOptionFilename = "roundOptions.dat";

    private void SaveFile<T>(T dataToSave, string filename)
    {
        string destination = Application.dataPath + "/" + filename;
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);

        XmlSerializer xs = new XmlSerializer(typeof(T));
        xs.Serialize(file, dataToSave);
        file.Close();
    }

    private T LoadFile<T>(string filename)
    {
        string destination = Application.dataPath + "/" + filename;
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            //file not found
            return default(T);
        }

        XmlSerializer xs = new XmlSerializer(typeof(T));
        T data = (T)xs.Deserialize(file);
        file.Close();

        return data;
    }

    public void LoadPlayerOptions()
    {
        PlayerOptions playOptions = LoadFile<PlayerOptions>(PlayerOptionFilename);
        //load default options if no files
        if (playOptions == null)
        {
            playOptions = new PlayerOptions();
            SaveFile(playOptions, PlayerOptionFilename);
        }

        CurrentLocalOptions.FXSound = playOptions.FXSound;
        CurrentLocalOptions.MusicSound = playOptions.MusicSound;
    }

    public void SavePlayerOptions()
    {
        PlayerOptions newPlayOptions = new PlayerOptions(CurrentLocalOptions.FXSound, CurrentLocalOptions.MusicSound);
        SaveFile(newPlayOptions, PlayerOptionFilename);
    }

    public void LoadGameRoundOptions()
    {
        GameRoundOptions roundOptions = LoadFile<GameRoundOptions>(GameRoundOptionFilename);
        //load default options if no files
        if (roundOptions == null)
        {
            roundOptions = new GameRoundOptions();
            SaveFile(roundOptions, GameRoundOptionFilename);
        }

        CurrentGameData.MaxCanvasX.Value = roundOptions.MaxCanvasX;
        CurrentGameData.MaxCanvasY.Value = roundOptions.MaxCanvasY;
        CurrentGameData.MinimalDrawingLength.Value = roundOptions.MinimalDrawingLength;
        CurrentGameData.BaseSpeedFillingPerSecond.Value = roundOptions.BaseSpeedFillingPerSecond;
        CurrentGameData.BaseInk.Value = roundOptions.BaseInk;
        CurrentGameData.InkPerTeamPixel.Value = roundOptions.InkPerTeamPixel;
        CurrentGameData.BaseInkRegenPerSecond.Value = roundOptions.BaseInkRegenPerSecond;
        CurrentGameData.InkRegenPerTeamPixel.Value = roundOptions.InkRegenPerTeamPixel;
        CurrentGameData.InkToAreaPaintRatio.Value = roundOptions.InkToAreaPaintRatio;
        CurrentGameData.RoundTime.Value = roundOptions.RoundTime;
    }

    public void SaveGameRoundOptions()
    {
        GameRoundOptions newRoundOptions = new GameRoundOptions(
            CurrentGameData.MaxCanvasX.Value,
            CurrentGameData.MaxCanvasY.Value,
            CurrentGameData.MinimalDrawingLength.Value,
            CurrentGameData.BaseSpeedFillingPerSecond.Value,
            CurrentGameData.BaseInk.Value,
            CurrentGameData.InkPerTeamPixel.Value,
            CurrentGameData.BaseInkRegenPerSecond.Value,
            CurrentGameData.InkRegenPerTeamPixel.Value,
            CurrentGameData.InkToAreaPaintRatio.Value,
            CurrentGameData.RoundTime.Value);

        SaveFile(newRoundOptions, GameRoundOptionFilename);
    }

    private void Awake()
    {
        LoadGameRoundOptions();
        LoadPlayerOptions();
    }
}
