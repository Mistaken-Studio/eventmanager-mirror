﻿// -----------------------------------------------------------------------
// <copyright file="DBoiBattleRoyalMicroHid.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using Mistaken.API;
using PlayerStatsSystem;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class DBoiBattleRoyalMicroHid : EventBase, IAnnouncePlayersAlive, IWinOnLastAlive
    {
        public override string Id => "dbbrmhid";

        public override string Description => "D Boi Battle Royale Micro Hid";

        public override string Name => "DBoiBattleRoyaleMicroHid";

        public Dictionary<string, string> Translations => new ()
        {
            { "D_Kill", "Zostałeś zdekontamionowany" },
        };

        public bool ClearPrevious => true;

        public override void Initialize()
        {
            this.decontaminated = 0;
            API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            API.Utilities.Map.TeslaMode = API.Utilities.TeslaMode.DISABLED_FOR_ALL;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Map.Pickups.ToList().ForEach(x => x.Destroy());
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            foreach (var e in Exiled.API.Features.Lift.List)
            {
                if (!e.Name.StartsWith("El"))
                    e.IsLocked = true;
            }

            foreach (var door in Door.List)
            {
                var doorType = door.Type;
                if (doorType == DoorType.CheckpointEntrance || doorType == DoorType.CheckpointLczA || doorType == DoorType.CheckpointLczB)
                {
                    door.ChangeLock(DoorLockType.DecontEvacuate);
                    door.IsOpen = true;
                }
                else if (doorType == DoorType.Scp914Gate)
                    door.ChangeLock(DoorLockType.AdminCommand);
                else
                    door.IsOpen = true;
            }
        }

        public override void Deinitialize()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
        }

        private int decontaminated = 0;

        private void Server_RoundStarted()
        {
            var rooms = Room.List.Where(x => x.Type != RoomType.EzShelter && x.Type != RoomType.HczTesla && x.Type != RoomType.HczNuke && x.Type != RoomType.Surface && x.Type != RoomType.Hcz049 && x.Type != RoomType.Pocket && x.Type != RoomType.Hcz106 && x.Type != RoomType.HczHid && x.Type != RoomType.Lcz914 && x.Type != RoomType.Lcz173 && x.Type != RoomType.EzCollapsedTunnel && x.Type != RoomType.Lcz330).ToList();
            foreach (var player in RealPlayers.List)
            {
                int random = Random.Range(0, rooms.Count());
                var room = rooms[random];
                player.SlowChangeRole(RoleType.ClassD, room.Position + (Vector3.up * 2));
            }

            this.Decontaminate();
            EventManager.Instance.RunCoroutine(this.GiveHid(), "dbbrmhid_givehid");
        }

        private void Decontaminate()
        {
            if (!this.Active)
                return;
            if (this.decontaminated < 2)
            {
                var rand = Random.Range(0, 3);
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
                Timing.CallDelayed(300, () =>
                {
                    if (!Round.IsStarted)
                        return;
                    this.Decontaminate();
                });
            }
            else
            {
                var rand = Random.Range(0, 3);
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

                Timing.CallDelayed(300, () =>
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
            Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone Decontamination in t minus 1 minute", false, true);
                Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone is under lockdown . All alive in it are now terminated", false, true);
                    Timing.CallDelayed(38, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var e in Exiled.API.Features.Lift.List)
                            e.IsLocked = true;
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.LightContainment)
                                player.Hurt(new CustomReasonDamageHandler(this.Translations["D_Kill"]));
                        }

                        Timing.CallDelayed(60, () =>
                        {
                            foreach (var e in Exiled.API.Features.Lift.List)
                            {
                                if (e.Name.StartsWith("El"))
                                    e.IsLocked = false;
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
            Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Heavy Containment Zone Decontamination in t minus 1 minute", false, true);
                Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Heavy Containment Zone Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Heavy Containment Zone is under lockdown . All alive in it are now terminated", false, true);
                    Timing.CallDelayed(38, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var e in Exiled.API.Features.Lift.List)
                            e.IsLocked = true;
                        foreach (var door in Door.List)
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

                        Timing.CallDelayed(60, () =>
                        {
                            foreach (var e in Exiled.API.Features.Lift.List)
                            {
                                if (e.Name.StartsWith("El"))
                                    e.IsLocked = false;
                            }

                            foreach (var door in Door.List)
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
            Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Entrance Zone Decontamination in t minus 1 minute", false, true);
                Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Entrance Zone Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Entrance Zone is under lockdown . All alive in it are now terminated", false, true);
                    Timing.CallDelayed(38, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var door in Door.List)
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

                        Timing.CallDelayed(60, () =>
                        {
                            foreach (var door in Door.List)
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
            Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE Decontamination in t minus 1 minute", false, true);
                Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE are under lockdown . All alive in them are now terminated", false, true);
                    Timing.CallDelayed(40, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var e in Exiled.API.Features.Lift.List)
                            e.IsLocked = true;
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
            Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE Decontamination in t minus 1 minute", false, true);
                Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE are under lockdown . All alive in them are now terminated", false, true);
                    Timing.CallDelayed(40, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var e in Exiled.API.Features.Lift.List)
                            e.IsLocked = true;
                        foreach (var door in Door.List)
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
            Timing.CallDelayed(224, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE Decontamination in t minus 1 minute", false, true);
                Timing.CallDelayed(38, () =>
                {
                    if (!this.Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE Decontamination in t minus 30 seconds PITCH_1.0 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE are under lockdown . All alive in them are now terminated", false, true);
                    Timing.CallDelayed(40, () =>
                    {
                        if (!this.Active)
                            return;
                        foreach (var door in Door.List)
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
            Timing.CallDelayed(202, () =>
            {
                if (!this.Active)
                    return;
                Cassie.Message("Pitch_0.9 Initiating nato_a warhead in 3 . 2 . 1 . ", false, true);
                Timing.CallDelayed(8, () =>
                {
                    if (!this.Active)
                        return;
                    Warhead.Start();
                    Warhead.IsLocked = true;
                });
            });
        }

        private IEnumerator<float> GiveHid()
        {
            yield return Timing.WaitForSeconds(5f);
            while (this.Active)
            {
                foreach (var player in RealPlayers.List)
                    player.AddItem(ItemType.MicroHID);
                yield return Timing.WaitForSeconds(5f);
            }
        }
    }
}
