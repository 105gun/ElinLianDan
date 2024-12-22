using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Assertions;
using System.Diagnostics;
using DG.Tweening;
using LianDan;

public class TraitDanLu : TraitWorkbench
{
    public override int MaxFuel
	{
		get
		{
			return 200;
		}
	}

	public override ToggleType ToggleType
	{
		get
		{
			return ToggleType.Fire;
		}
	}

    public override void OnInstall(bool byPlayer)
    {
        Plugin.ModLog("TraitDanLu.OnUse");
        if (byPlayer && !EClass.pc.HasElement(6516))
        {
            EClass.pc.GainAbility(6516);
        }
        base.OnInstall(byPlayer);
    }
}