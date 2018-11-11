﻿using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps
{
    class CompHibernatable_MechanoidBeacon : CompHibernatable
    {
        public int extraStartUpDays = 0;
        public int coolDownTicks = 0;

        public new CompProperties_Hibernatable_MechanoidBeacon Props
        {
            get
            {
                return (CompProperties_Hibernatable_MechanoidBeacon)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref extraStartUpDays, "extraStartUpDays");
            Scribe_Values.Look(ref coolDownTicks, "coolDownTicks");
        }

        public override void CompTick()
        {
            if (this.State == HibernatableStateDefOf.Starting && Find.TickManager.TicksGame > Traverse.Create(this).Field("endStartupTick").GetValue<int>() )
            {
                FinishWarmup();
            }
            if (coolDownTicks > 0)
            {
                coolDownTicks--;
            }
        }

        private void FinishWarmup()
        {
            this.State = HibernatableStateDefOf.Running;
            Traverse.Create(this).Field("endStartupTick").SetValue(0);
            if (this.parent.Map.listerBuildings.allBuildingsColonist.FirstOrDefault((Building b) => b is Building_RogueAI) is Building_RogueAI rogueAI)
            {
                Find.LetterStack.ReceiveLetter("WTH_MechanoidBeaconComplete_Label".Translate(), "WTH_MechanoidBeaconComplete_Description".Translate(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(this.parent), null, null);
                rogueAI.IsConscious = true;
            }
            extraStartUpDays += 2;
            coolDownTicks += Props.coolDownDaysAfterSuccess * GenDate.TicksPerDay;
        }

        public override string CompInspectStringExtra()
        {
            string text = base.CompInspectStringExtra();
            if(!text.NullOrEmpty())
            {
                text += "\n";
            }
            text += "WTH_CompHibernatable_MechanoidBeacon_StartUpDays".Translate(Props.startupDays + extraStartUpDays);
            if(coolDownTicks > 0)
            {
                text += "\n" + "WTH_CompHibernatable_MechanoidBeacon_Cooldown".Translate((coolDownTicks/(float)GenDate.TicksPerDay).ToStringDecimalIfSmall());
            }
            return text; 
        }
    }
}
