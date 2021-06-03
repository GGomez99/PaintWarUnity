using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalOptions : MonoBehaviour
{
    public int FXSound = 100;
    public int MusicSound = 100;
    public int CameraSensitivity = 3;

    public void UpdateFxSound(float value)
    {
        FXSound = (int) value;
    }

    public void UpdateMusicSound(float value)
    {
        MusicSound = (int) value;
    }


    public void UpdateCameraSensi(float value)
    {
        CameraSensitivity = (int) value;
    }


}
