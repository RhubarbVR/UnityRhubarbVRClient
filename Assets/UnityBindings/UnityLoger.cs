using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;

public class UnityLoger : IRLog
{
    public void Err(string value)
    {
        Log(LogLevel.Error, value);
    }

    public void Info(string value)
    {
        Log(LogLevel.Info, value);
    }
    public void Warn(string v)
    {
        Log(LogLevel.Warning, v);
    }

    public void Log(LogLevel level, string v)
    {
        switch (level)
        {
            case LogLevel.Diagnostic:
                Debug.Log("Diagnostic: " + v);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(v);
                break;
            case LogLevel.Error:
                Debug.LogError(v);
                break;
            default:
                Debug.Log(v);
                break;
        }
        callback?.Invoke(level,$"[{level.ToString().ToLower()}] " + v + "\n");
    }

    event Action<LogLevel, string> callback;

    public void Subscribe(Action<LogLevel, string> logCall)
    {
        Debug.Log("Bound in");
        callback += logCall;
    }

    public void Unsubscribe(Action<LogLevel, string> logCall)
    {
        callback -= logCall;
    }


}
