// -----------------------------------------------------------------------
// <copyright file="GuardChaseDRevenge.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using HarmonyLib;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
#pragma warning disable SA1402
    internal class GuardChaseDRevenge : IEMEventClass
    {
        public override string Id => "GCDR";

        public override string Description => "Guard needs to find Class Ds and kill them. Class D needs to stay alive until 8 minute. Then they get a revolver with which they must take revenge on the guard";

        public override string Name => "Guard Chase D Revenge";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "G_Info", "Jesteś <color=gray>Ochroniarzem</color>. Posiadasz nieskończoną ilość amunicji. Twoim zadaniem jest znalezienie i zabicie każdej <color=orange>Klasy D</color>." },
            { "D_Info", "Jesteś <color=orange>Klasą D</color>. Twoim zadaniem jest schowanie się/ucieczka przed <color=gray>Ochroniarzem</color>. Po 6 minutach dostaniecie Rewolwer z pomocą którego dokanacie odwetu." },
        };

        public override void OnIni()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Map.Pickups.ToList().ForEach(x => x.Destroy());
            Exiled.Events.Events.DisabledPatchesHashSet.Add(typeof(PlayerInteract).GetMethod(nameof(PlayerInteract.OnInteract), BindingFlags.NonPublic | BindingFlags.Instance));
            Exiled.Events.Events.Instance.ReloadDisabledPatches();
            PluginHandler.Harmony.Patch(typeof(PlayerInteract).GetMethod(nameof(PlayerInteract.OnInteract), BindingFlags.NonPublic | BindingFlags.Instance), transpiler: new HarmonyMethod(typeof(EventPatches).GetMethod("Transpiler1", BindingFlags.NonPublic | BindingFlags.Static)));
            PluginHandler.Harmony.Patch(typeof(BreakableWindow).GetMethod(nameof(BreakableWindow.Damage), BindingFlags.Public | BindingFlags.Instance), transpiler: new HarmonyMethod(typeof(EventPatches).GetMethod("Transpiler2", BindingFlags.NonPublic | BindingFlags.Static)));
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.Shooting += this.Player_Shooting;
            foreach (var e in Exiled.API.Features.Lift.List)
                e.IsLocked = true;

            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.CheckpointEntrance)
                {
                    door.ChangeLock(DoorLockType.DecontEvacuate);
                    door.IsOpen = true;
                }
                else if (door.Type == DoorType.GateA || door.Type == DoorType.GateB)
                {
                    door.ChangeLock(DoorLockType.AdminCommand);
                    door.IsOpen = false;
                }
                else
                {
                    door.ChangeLock(DoorLockType.AdminCommand);
                    door.IsOpen = true;
                }
            }
        }

        public override void OnDeIni()
        {
            PluginHandler.Harmony.Unpatch(typeof(PlayerInteract).GetMethod(nameof(PlayerInteract.OnInteract), BindingFlags.NonPublic | BindingFlags.Instance), HarmonyPatchType.Prefix);
            PluginHandler.Harmony.Unpatch(typeof(BreakableWindow).GetMethod(nameof(BreakableWindow.Damage), BindingFlags.Public | BindingFlags.Instance), HarmonyPatchType.Prefix);
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.Shooting -= this.Player_Shooting;
        }

        private void Server_RoundStarted()
        {
            Map.TurnOffAllLights(float.MaxValue);
            var players = RealPlayers.List.ToList();
            var guard = players[UnityEngine.Random.Range(0, players.Count)];
            players.Remove(guard);
            guard.SlowChangeRole(RoleType.FacilityGuard, Room.List.First(x => x.Type == RoomType.Surface).Position + Vector3.up);
            guard.Broadcast(8, EventManager.EMLB + this.Translations["G_Info"]);
            guard.SetGUI("gcdr_guard", PseudoGUIPosition.MIDDLE, "Za minutę zostaniesz przeniesiony do <color=yellow>przejścia HCZ-EZ</color>", 10);
            Vector3 checkpoint = Room.List.First(x => x.Type == RoomType.HczEzCheckpoint).Position + Vector3.up;
            foreach (var player in players)
            {
                player.SlowChangeRole(RoleType.ClassD, checkpoint);
                player.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"]);
                MEC.Timing.CallDelayed(0.2f, () => player.AddItem(ItemType.Flashlight));
            }

            MEC.Timing.CallDelayed(0.2f, () =>
            {
                guard.RemoveItem(guard.Items.First(x => x.Type == ItemType.GunFSP9));
                var weapon = (InventorySystem.Items.Firearms.Firearm)Item.Create(ItemType.GunFSP9).Base;
                weapon.Status = new InventorySystem.Items.Firearms.FirearmStatus(30, weapon.Status.Flags, 10770);
                weapon._status = weapon.Status;
                guard.AddItem(weapon);
                weapon._sendStatusNextFrame = true;
                guard.RemoveItem(guard.Items.First(x => x.Type == ItemType.KeycardGuard));
            });

            MEC.Timing.CallDelayed(60, () =>
            {
                if (!this.Active)
                    return;
                guard.Position = checkpoint;
                MEC.Timing.CallDelayed(300, () =>
                {
                    if (!this.Active)
                        return;
                    foreach (var player in RealPlayers.List)
                    {
                        if (player.Role != RoleType.ClassD)
                            continue;
                        var weapon = (InventorySystem.Items.Firearms.Firearm)Item.Create(ItemType.GunRevolver).Base;
                        weapon.Status = new InventorySystem.Items.Firearms.FirearmStatus(4, weapon.Status.Flags, 588);
                        weapon._status = weapon.Status;
                        player.AddItem(weapon);
                        weapon._sendStatusNextFrame = true;
                        player.SetGUI("gcdr_classd", PseudoGUIPosition.MIDDLE, "Dostałeś <color=yellow>Rewolwer</color>", 5);
                        player.EnableEffect<CustomPlayerEffects.Visuals939>();
                    }

                    guard.EnableEffect<CustomPlayerEffects.Invisible>();
                    EventManager.Instance.RunCoroutine(this.UpdateGuardVisibility(), "GCDR_UpdateGuardVisibility");
                });
            });
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (RealPlayers.List.Count(x => x.Role.Team == Team.CDP) == 0)
                this.OnEnd("<color=gray>Ochrona</color> wygrywa!");
            else if (RealPlayers.List.FirstOrDefault(x => x.Role == RoleType.FacilityGuard) == default)
                this.OnEnd("<color=orange>Klasa D</color> wygrywa!");
        }

        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            if (ev.Shooter.Role.Type == RoleType.FacilityGuard)
            {
                var firearm = (InventorySystem.Items.Firearms.Firearm)ev.Shooter.CurrentItem.Base;
                firearm.Status = new InventorySystem.Items.Firearms.FirearmStatus(30, firearm.Status.Flags, firearm.Status.Attachments);
                firearm._sendStatusNextFrame = true;
            }
            else if (ev.Shooter.Role.Type == RoleType.ClassD)
            {
                var firearm = (InventorySystem.Items.Firearms.Firearm)ev.Shooter.CurrentItem.Base;
                firearm.Status = new InventorySystem.Items.Firearms.FirearmStatus(4, firearm.Status.Flags, firearm.Status.Attachments);
                firearm._sendStatusNextFrame = true;
            }
        }

        private IEnumerator<float> UpdateGuardVisibility()
        {
            while (this.Active)
            {
                foreach (var guard in RealPlayers.List)
                {
                    if (guard.Role.Type != RoleType.FacilityGuard)
                        continue;
                    if (RealPlayers.List.Any(x => x.Role == RoleType.ClassD && Physics.Linecast(x.Position + Vector3.up, guard.Position + Vector3.up, out var hit, ~(1 << LayerMask.NameToLayer("Player"))) && hit.collider.name == "Player"))
                    {
                        guard.ChangeEffectIntensity(EffectType.Invisible, 0);
                        yield return MEC.Timing.WaitForSeconds(1.5f);
                    }
                    else
                        guard.ChangeEffectIntensity(EffectType.Invisible, 1);
                }

                yield return MEC.Timing.WaitForSeconds(0.2f);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.OnInteract))]
    [HarmonyPatch(typeof(BreakableWindow), nameof(BreakableWindow.Damage))]
    internal class EventPatches
    {
        private static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            yield return new CodeInstruction(OpCodes.Ret);
            yield break;
        }

        private static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
            yield return new CodeInstruction(OpCodes.Ret);
            yield break;
        }
    }
}
