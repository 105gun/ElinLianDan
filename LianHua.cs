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

namespace LianDan;

public class AI_LianHua : AI_TargetCard
{
    public enum DanYaoType
    {
        Normal,
        JinDan,
        XianDan,
    };

    List<string> banList = new List<string> {""}; // adv_mutsumi
    public static Func<Chara, bool> funcWitness;

    public AI_LianHua()
    {
        // Porting form AI_Steal
        if (funcWitness != null)
        {
            return;
        }
        funcWitness = delegate(Chara c)
        {
            int num = c.CanSee(TC) ? 0 : 30;
            int num2 = c.PER * 250 / 100;
            if (c.conSleep != null)
            {
                return false;
            }
            if (c.IsUnique)
            {
                num2 *= 2;
            }
            return EClass.rnd(num2) > (TC.HasElement(152) ? TC.Evalue(152) : 0) + num;
        };
    }

    public bool HasDanLu()
    {
        if (Game._zone.map.Installed.Find<TraitDanLu>() != null) // Game._zone.IsPlayerFaction && 
        {
            return true;
        }
        
		foreach (Chara chara in EClass.pc.party.members)
		{
			ThingContainer things = chara.things;
            bool flag = false;
			Action<Thing> action = delegate(Thing t)
            {
                if (t.trait is TraitDanLu)
                {
                    flag = true;
                }
            };
			things.Foreach(action, true);
            if (flag)
            {
                return true;
            }
		}
        return false;
    }

    public DanYaoType GetDanYaoType()
    {
        if (HasDanLu())
        {
            if (EClass.rnd(10) == 0)
            {
                return DanYaoType.XianDan;
            }
            return DanYaoType.JinDan;
        }
        else
        {
            if (EClass.rnd(100) == 0)
            {
                return DanYaoType.XianDan;
            }
            if (EClass.rnd(10) == 0)
            {
                return DanYaoType.JinDan;
            }
            return DanYaoType.Normal;
        }
    }

    public void LianHua()
    {
        string danYaoName;
        float skillMultiplier;
        Thing danYao;
        Chara targetChara = this.target.Chara;
        DanYaoType type = GetDanYaoType();

        switch (type)
        {
            case DanYaoType.JinDan:
                danYaoName = "jin_dan";
                skillMultiplier = 0.333f;
                break;
            case DanYaoType.XianDan:
                danYaoName = "xian_dan";
                skillMultiplier = 1.0f;
                break;
            default:
                danYaoName = "dan_yao";
                skillMultiplier = 0.2f;
                break;
        }

        // Make dan yao
        danYao = ThingGen.Create(danYaoName);
        danYao.MakeRefFrom(targetChara, null);
        danYao.LV = (int)(targetChara.LV * skillMultiplier + 1);
        foreach (Element ele in targetChara.elements.dict.Values)
        {
            if (ele.id >= 70 && ele.id <= 77)
            {
                danYao.elements.dict[ele.id].vSource = (int)(ele.Value * skillMultiplier + 1) * 10;
            }
        }
        targetChara._affinity = -100;
        if (targetChara.IsPCFactionOrMinion && targetChara.IsBranchMember())
        {
            while (targetChara.things.Count > 0)
            {
                Thing thing = targetChara.things[0];
                targetChara.DropThing(thing, thing.Num);
            }
            targetChara.Die(null, null, AttackSource.DeathSentense);
            targetChara.homeBranch.BanishMember(targetChara, true);
        }
        else
        {
            targetChara.Die(null, null, AttackSource.DeathSentense);
        }
        
        CC.Pick(danYao);

        // Karma
        if (CC == EClass.pc)
        {
            if (targetChara.OriginalHostility == Hostility.Enemy)
            {
                EClass.player.ModKarma(-1);
            }
            else
            {
                EClass.player.ModKarma(-5);
            }
        }

        // Msg
        Msg.SetColor(Msg.colors.TalkGod);
        Msg.SayRaw("lianhua_success".lang(targetChara.Name, danYao.Name));
    }

    public override TargetType TargetType
	{
		get
		{
			return TargetType.SelfAndNeighbor;
		}
	}

    public override bool IsValidTC(Card c)
	{
		return !c.isThing && ((c.IsPCFactionOrMinion && !c.IsUnique) || (!c.IsPCFactionOrMinion)) && !banList.Contains(c.id);
	}

	public override int MaxRadius
	{
		get
		{
			return 2;
		}
	}

	public override bool CanPerform()
	{
		return Act.TC != null;
	}

    public override bool Perform()
    {
		this.target = Act.TC;
		return base.Perform();
    }

	public override bool IsHostileAct
	{
		get
		{
			return true;
		}
	}

    public override IEnumerable<AIAct.Status> Run()
	{
		Chara chara = this.target.Chara;
        Progress_Custom progress_Custom = new Progress_Custom();
        if (chara == null)
        {
            Msg.SetColor(Msg.colors.Negative);
            Msg.SayRaw("lianhua_notarget".lang());
            yield break;
        }
        if (chara == CC || chara == EClass.pc)
        {
            if (HasDanLu())
            {
                Msg.SetColor(Msg.colors.TalkGod);
                Msg.SayRaw("has_danlu_0".lang());
            }
            else
            {
                Msg.SetColor(Msg.colors.TalkGod);
                Msg.SayRaw("has_danlu_1".lang());
            }
            yield break;
        }

        // Qing Ke Lian Hua
        if (chara.LV * 10 < CC.STR || chara.hp < chara.MaxHP / 10 || chara.IsPCFactionOrMinion)
        {
            if (chara.IsPCFactionOrMinion)
            {
                Dialog.YesNo("lianhua_dialog".lang(TC.GetName(NameStyle.Full, 1), null, null, null, null), LianHua, null, "yes", "no");
            }
            else
            {
                LianHua();
            }
            yield break;
        }
        
        // Normal Lian Hua
        progress_Custom.canProgress = (() => (chara == null || chara.ExistsOnMap));
        progress_Custom.onProgressBegin = delegate()
        {
        };
        progress_Custom.onProgress = delegate(Progress_Custom p)
        {
            Point pos = this.owner.pos;
            if (chara != null && this.owner.Dist(chara) > 1)
            {
                EClass.pc.TryMoveTowards(chara.pos);
                if (this.owner == null)
                {
                    p.Cancel();
                    return;
                }
                if (chara != null && this.owner.Dist(chara) > 1)
                {
                    EClass.pc.Say("targetTooFar", null, null);
                    p.Cancel();
                    return;
                }
            }
            if (pos.TryWitnessCrime(this.owner, chara, 7, funcWitness) && chara.hostility != Hostility.Enemy)
            {
                p.Cancel();
                return;
            }
        };
        progress_Custom.onProgressComplete = delegate()
        {
            LianHua();
        };
        Progress_Custom seq = progress_Custom.SetDuration(40, 4);
        yield return base.Do(seq, null);
        yield break;
    }
}