using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Assertions;

namespace LianDan;

public enum PrivateLogLevel
{
    None,
    Error,
    Warning,
    Info,
    Debug
};

[BepInPlugin($"{MOD_AUTHOR}.{MOD_NAME_LOWER}.mod", MOD_NAME, MOD_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string MOD_AUTHOR = "105gun";
    public const string MOD_NAME = "LianDan";
    public const string MOD_NAME_LOWER = "liandan";
    public const string MOD_VERSION = "1.0.2.0";
    public static bool enableDebug = false;
    static PrivateLogLevel pluginLogLevel = PrivateLogLevel.Info;

    private void Start()
    {
        ModLog("Initializing");
        var harmony = new Harmony($"{MOD_AUTHOR}.{MOD_NAME_LOWER}.mod");
        harmony.PatchAll();
        Settings.LoadSettings();
        ModLog("Initialization completed");
    }

    public static void ModLog(string message, PrivateLogLevel logLevel = PrivateLogLevel.Info)
    {
        if (logLevel > pluginLogLevel)
        {
            return;
        }
        switch (logLevel)
        {
            case PrivateLogLevel.Error:
                message = $"[{MOD_NAME}][Error] {message}";
                break;
            case PrivateLogLevel.Warning:
                message = $"[{MOD_NAME}][Warning] {message}";
                break;
            case PrivateLogLevel.Info:
                message = $"[{MOD_NAME}][Info] {message}";
                break;
            case PrivateLogLevel.Debug:
                message = $"[{MOD_NAME}][Debug] {message}";
                break;
            default:
                break;
        }
        System.Console.WriteLine(message);
    }
}