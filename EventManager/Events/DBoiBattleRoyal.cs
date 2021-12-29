﻿// -----------------------------------------------------------------------
// <copyright file="DBoiBattleRoyal.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using PlayerStatsSystem;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class DBoiBattleRoyal : IEMEventClass, IAnnouncePlayersAlive, ISpawnRandomItems, IWinOnLastAlive
    {
        public override string Id => "dbbr";

        public override string Description => "D Boi Battle Royale";

        public override string Name => "DBoiBattleRoyale";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_Kill", "Zostałeś zdekontamionowany" },
        };

        public bool ClearPrevious => true;

        public override void OnIni()
        {
            this.stats.Clear();
            this.decontaminated = 0;
            Mistaken.API.Utilities.Map.TeslaMode = Mistaken.API.Utilities.TeslaMode.DISABLED_FOR_ALL;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            MapGeneration.InitiallySpawnedItems.Singleton.ClearAll();
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            foreach (var e in Map.Lifts)
            {
                if (!e.elevatorName.StartsWith("El"))
                    e.Network_locked = true;
            }

            foreach (var door in Map.Doors)
            {
                if (door.Type == DoorType.CheckpointEntrance || door.Type == DoorType.CheckpointLczA || door.Type == DoorType.CheckpointLczB)
                {
                    door.ChangeLock(DoorLockType.DecontEvacuate);
                    door.IsOpen = true;
                }
                else if (door.Type == DoorType.Scp914)
                    door.ChangeLock(DoorLockType.AdminCommand);
                else
                    door.IsOpen = true;
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
        }

        private readonly Dictionary<Player, int> stats = new Dictionary<Player, int>();

        private int decontaminated = 0;

        private void Server_RoundStarted()
        {
            var rooms = Map.Rooms.Where(room => room.Type != RoomType.EzShelter && room.Type != RoomType.HczTesla && room.Type != RoomType.HczNuke && room.Type != RoomType.Surface && room.Type != RoomType.Hcz049 && room.Type != RoomType.Pocket && room.Type != RoomType.Hcz106 && room.Type != RoomType.HczHid && room.Type != RoomType.Lcz914 && room.Type != RoomType.Lcz173 && room.Type != RoomType.EzCollapsedTunnel).ToList();
            foreach (var player in RealPlayers.List)
            {
                int random = UnityEngine.Random.Range(0, rooms.Count());
                var room = rooms[random];
                player.SlowChangeRole(RoleType.ClassD, room.Position + (Vector3.up * 2));
            }

            this.Decontaminate();
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!this.stats.ContainsKey(ev.Killer))
                this.stats.Add(ev.Killer, 0);
            this.stats[ev.Killer] += 1;
            ev.Target.SetGUI("em_dbbr_stats", PseudoGUIPosition.MIDDLE, $"Tym razem Ci się nie udało\nKończysz z następującą ilością zabójstw:\n<color=yellow><size=200%>{(this.stats.ContainsKey(ev.Target) ? this.stats[ev.Target] : 0)}</size></color>", 15f);
            if (RealPlayers.Get(RoleType.ClassD).Count() == 1)
                ev.Killer.SetGUI("em_dbbr_stats", PseudoGUIPosition.MIDDLE, $"Gratuluję zwycięstwa!\nKończysz z następującą ilością zabójstw:\n<color=yellow><size=200%>{(this.stats.ContainsKey(ev.Killer) ? this.stats[ev.Killer] : 0)}</size></color>", 15f);
        }

        private void Decontaminate()
        {
            if (!this.Active)
                return;
            if (this.decontaminated < 2)
            {
                var rand = UnityEngine.Random.Range(0, 3);
                if (rand == 0)
                {
                    if (!Round.IsStarted)
                        return;
                    this.DecontaminateLCZ();
                }
                else if (rand == 1)
                {
                    if (!Round.IsStarted)
                        return;
                    this.DecontaminateHCZ();
                }
                else if (rand == 2)
                {
                    if (!Round.IsStarted)
                        return;
                    this.DecontaminateEZ();
                }

                this.decontaminated++;
                MEC.Timing.CallDelayed(300, () =>
                {
                    if (!Round.IsStarted)
                        return;
                    this.Decontaminate();
                });
            }
            else
            {
                var rand = UnityEngine.Random.Range(0, 3);
                if (rand == 0)
                {
                    if (!Round.IsStarted)
                        return;
                    this.DecontaminateEZ_LCZ();
                }
                else if (rand == 1)
                {
                    if (!Round.IsStarted)
                        return;
                    this.DecontaminateEZ_HCZ();
                }
                else if (rand == 2)
                {
                    if (!Round.IsStarted)
                        return;
                    this.DecontaminateLCZ_HCZ();
                }

                MEC.Timing.CallDelayed(300, () =>
                {
                    if (!Round.IsStarted)
                        return;
                    this.DecontaminateFacility();
                });
            }
        }

        private void DecontaminateLCZ()
        {
            if (!this.Active)
                return;
            MEC.Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone Decontamination in t minus 1 minute", false, true);
                MEC.Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone is under lockdown . All alive in it are now terminated", false, true);
                    MEC.Timing.CallDelayed(38, () =>
                    {
                        if (!this.Active)
                            return;
                        var elevators = Map.Lifts;
                        foreach (var elevator in elevators)
                            elevator.Network_locked = true;
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.LightContainment)
                                player.Hurt(new CustomReasonDamageHandler(this.Translations["D_Kill"]));
                        }

                        MEC.Timing.CallDelayed(60, () =>
                        {
                            foreach (var e in elevators)
                            {
                                if (e.elevatorName.StartsWith("El"))
                                    e.Network_locked = false;
                            }
                        });
                    });
                });
            });
        }

        private void DecontaminateHCZ()
        {
            if (!this.Active)
                return;
            MEC.Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Heavy Containment Zone Decontamination in t minus 1 minute", false, true);
                MEC.Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Heavy Containment Zone Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Heavy Containment Zone is under lockdown . All alive in it are now terminated", false, true);
                    MEC.Timing.CallDelayed(38, () =>
                    {
                        if (!this.Active)
                            return;
                        var elevators = Map.Lifts;
                        var doors = Map.Doors;
                        foreach (var elevator in elevators)
                            elevator.Network_locked = true;
                        foreach (var door in doors)
                        {
                            if (door.Type == DoorType.CheckpointEntrance)
                            {
                                door.IsOpen = false;
                                door.ChangeLock(DoorLockType.DecontEvacuate);
                                door.ChangeLock(DoorLockType.DecontLockdown);
                            }
                        }

                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.HeavyContainment)
                                player.Hurt(new CustomReasonDamageHandler(this.Translations["D_Kill"]));
                        }

                        MEC.Timing.CallDelayed(60, () =>
                        {
                            foreach (var e in elevators)
                            {
                                if (e.elevatorName.StartsWith("El"))
                                    e.Network_locked = false;
                            }

                            foreach (var door in doors)
                            {
                                if (door.Type == DoorType.CheckpointEntrance)
                                {
                                    door.IsOpen = true;
                                    door.ChangeLock(DoorLockType.DecontEvacuate);
                                    door.ChangeLock(DoorLockType.DecontLockdown);
                                }
                            }
                        });
                    });
                });
            });
        }

        private void DecontaminateEZ()
        {
            if (!this.Active)
                return;
            MEC.Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Entrance Zone Decontamination in t minus 1 minute", false, true);
                MEC.Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Entrance Zone Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Entrance Zone is under lockdown . All alive in it are now terminated", false, true);
                    MEC.Timing.CallDelayed(38, () =>
                    {
                        if (!this.Active)
                            return;
                        var doors = Map.Doors;
                        foreach (var door in doors)
                        {
                            if (door.Type == DoorType.CheckpointEntrance)
                            {
                                door.IsOpen = false;
                                door.ChangeLock(DoorLockType.DecontEvacuate);
                                door.ChangeLock(DoorLockType.DecontLockdown);
                            }
                        }

                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.Entrance)
                                player.Hurt(new CustomReasonDamageHandler(this.Translations["D_Kill"]));
                        }

                        MEC.Timing.CallDelayed(60, () =>
                        {
                            foreach (var door in doors)
                            {
                                if (door.Type == DoorType.CheckpointEntrance)
                                {
                                    door.IsOpen = true;
                                    door.ChangeLock(DoorLockType.DecontEvacuate);
                                    door.ChangeLock(DoorLockType.DecontLockdown);
                                }
                            }
                        });
                    });
                });
            });
        }

        private void DecontaminateEZ_HCZ()
        {
            if (!this.Active)
                return;
            MEC.Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE Decontamination in t minus 1 minute", false, true);
                MEC.Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE are under lockdown . All alive in them are now terminated", false, true);
                    MEC.Timing.CallDelayed(40, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var elevator in Map.Lifts)
                            elevator.Network_locked = true;
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.HeavyContainment || player.CurrentRoom?.Zone == ZoneType.Entrance)
                                player.Hurt(new CustomReasonDamageHandler(this.Translations["D_Kill"]));
                        }
                    });
                });
            });
        }

        private void DecontaminateEZ_LCZ()
        {
            if (!this.Active)
                return;
            MEC.Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE Decontamination in t minus 1 minute", false, true);
                MEC.Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE are under lockdown . All alive in them are now terminated", false, true);
                    MEC.Timing.CallDelayed(40, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var elevator in Map.Lifts)
                            elevator.Network_locked = true;
                        foreach (var door in Map.Doors)
                        {
                            if (door.Type == DoorType.CheckpointEntrance)
                            {
                                door.IsOpen = false;
                                door.ChangeLock(DoorLockType.DecontEvacuate);
                                door.ChangeLock(DoorLockType.DecontLockdown);
                            }
                        }

                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.LightContainment || player.CurrentRoom?.Zone == ZoneType.Entrance)
                                player.Hurt(new CustomReasonDamageHandler(this.Translations["D_Kill"]));
                        }
                    });
                });
            });
        }

        private void DecontaminateLCZ_HCZ()
        {
            if (!this.Active)
                return;
            MEC.Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE Decontamination in t minus 1 minute", false, true);
                MEC.Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE are under lockdown . All alive in them are now terminated", false, true);
                    MEC.Timing.CallDelayed(40, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var door in Map.Doors)
                        {
                            if (door.Type == DoorType.CheckpointEntrance)
                            {
                                door.IsOpen = false;
                                door.ChangeLock(DoorLockType.DecontEvacuate);
                                door.ChangeLock(DoorLockType.DecontLockdown);
                            }
                        }

                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.LightContainment || player.CurrentRoom?.Zone == ZoneType.HeavyContainment)
                                player.Hurt(new CustomReasonDamageHandler(this.Translations["D_Kill"]));
                        }
                    });
                });
            });
        }

        private void DecontaminateFacility()
        {
            if (!this.Active)
                return;
            MEC.Timing.CallDelayed(202, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Initiating nato_a warhead in 3 . 2 . 1 . ", false, true);
                MEC.Timing.CallDelayed(8, () =>
                {
                    if (!this.Active)
                        return;
                    Warhead.Start();
                    Warhead.IsLocked = true;
                });
            });
        }
    }
}
