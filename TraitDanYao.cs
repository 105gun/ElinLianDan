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

public class TraitDanYao : TraitFood
{
    public override void OnEat(Chara c)
    {
        bool flag = this.owner.blessedState < BlessedState.Blessed && (this.owner.blessedState <= BlessedState.Cursed || EClass.rnd(2) == 0);

        for (int i = 70; i <= 77; i++)
        {
            Element charaElement = c.elements.GetElement(i);
            int increaseElementLvl = this.owner.elements.GetElement(i).Value / 10;
            int vBase = charaElement.vBase;

            c.elements.ModBase(i, increaseElementLvl);
            c.elements.OnLevelUp(charaElement, vBase);
        }
        for (int i = 0; i < owner.LV; i++)
        {
            c.LevelUp();
        }
    }
}