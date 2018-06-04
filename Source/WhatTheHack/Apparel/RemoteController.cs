﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Duties;
using WhatTheHack.Storage;

namespace WhatTheHack
{
    class RemoteController : Apparel
    {
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == base.Wearer)
            {
                Pawn linkedPawn = base.Wearer.RemoteControlLink();
                if (linkedPawn == null)
                {
                    yield return GetRemoteControlActivateGizmo(base.Wearer);

                }
                else
                {
                    yield return GetRemoteControlDeActivateGizmo(base.Wearer);
                }
            }
        }

        private static Gizmo GetRemoteControlDeActivateGizmo(Pawn pawn)
        {
            Command_Action command = new Command_Action();
            command.defaultLabel = "WTH_Gizmo_RemoteControlDeactivate_Label".Translate();
            command.defaultDesc = "WTH_Gizmo_RemoteControlDeactivate_Description".Translate();
            command.icon = TexCommand.CannotShoot;
            command.action = delegate ()
            {
                Pawn mech = pawn.RemoteControlLink();
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                pawnData.remoteControlLink = null;
                mechData.remoteControlLink = null;
                mech.drafter.Drafted = false;
            };
            return command;
        }


        private static Gizmo GetRemoteControlActivateGizmo(Pawn pawn)
        {
            Command_Target command_Target = new Command_Target();
            command_Target.defaultLabel = "WTH_Gizmo_RemoteControlActivate_Label".Translate();
            command_Target.defaultDesc = "WTH_Gizmo_RemoteControlActivate_Description".Translate();
            command_Target.targetingParams = GetTargetingParametersForHacking();
            command_Target.hotKey = KeyBindingDefOf.Misc5;
            command_Target.icon = TexCommand.Install;
            command_Target.action = delegate (Thing target)
            {
                if(target is Pawn)
                {
                    Pawn mech = (Pawn)target;
                    ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                    ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                    pawnData.remoteControlLink = mech;
                    mechData.remoteControlLink = pawn;
                    mechData.isActive = true;
                    mech.drafter.Drafted = true;
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    if (pawn.GetLord() == null || pawn.GetLord().LordJob == null)
                    {
                        LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_ControlMechanoid(), pawn.Map, new List<Pawn> { pawn });
                        pawn.mindState.duty.focus = mech;
                    }
                }
            };
            return command_Target;
        }
        private static TargetingParameters GetTargetingParametersForHacking()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }
                    Pawn pawn = targ.Thing as Pawn;
                    return pawn != null && !pawn.Downed && pawn.IsHacked() && pawn.HasHackedLocomotion();
                }
            };
        }
    }
}