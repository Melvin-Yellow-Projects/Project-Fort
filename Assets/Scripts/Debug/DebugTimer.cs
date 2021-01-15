using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Timers;

public class DebugTimer : MonoBehaviour
{
    /************************************************************/
    #region Variables

    private static Timer timer;

    #endregion
    /************************************************************/
    #region Unity Functions

    void Start()
    {
        // creating timer with 1 sec interval
        timer = new System.Timers.Timer(1000); 

        // hook up the Elapsed event for the timer
        timer.Elapsed += OnTimedEvent;
        timer.Elapsed += Hello;
        timer.AutoReset = true;
        timer.Enabled = true;

        enabled = false;
    }

    private void Update()
    {
        Debug.LogError("Oh no! Update Fired!");
    }

    private void OnDestroy()
    {
        timer.Stop();
        timer.Dispose();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private static void OnTimedEvent(System.Object source, ElapsedEventArgs e)
    {
        Debug.Log("Hello, I am a static Timer handler");
    }

    private void Hello(System.Object source, ElapsedEventArgs e)
    {
        Debug.Log("Hello, I am a non-static Timer handler");
    }

    #endregion
}
