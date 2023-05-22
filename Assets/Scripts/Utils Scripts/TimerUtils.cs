using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimerUtils
{
    public static string DisplayCurrentTime(float totalSeconds)
    {
        //GET MINUTES AND SECONDS FROM THE REMAINING TIME IN SECONDS
        int minutes = (int)totalSeconds / 60;
        int seconds = (int)totalSeconds % 60;

        // DISPLAY TIME IN THIS FORMAT MM : SS
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
