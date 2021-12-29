﻿// -----------------------------------------------------------------------
// <copyright file="Hide.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mistaken.API;
using Mistaken.API.Extensions;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class Hide : IEMEventClass, IAnnouncePlayersAlive
    {
        public override string Id => "hide";

        public override string Description => "Hide event";

        public override string Name => "Hide";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_Info", "Twoim zadaniem jest ucieczka do <color=grey>HCZ</color>. <color=lime>Checkpointy</color> otwierają się za 5 minut, w międzyczasie ukryj się przed <color=red>SCP-939</color>." },
            { "SCP_Info", "Twoim zadaniem jest znalezienie <color=orange>klas D</color> i ich zabicie." },
        };

        public bool ClearPrevious => true;

        public override void OnIni()
        {
            this.escapeDoors.Clear();
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            MapGeneration.InitiallySpawnedItems.Singleton.ClearAll();
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            foreach (var door in Map.Doors)
            {
                if (door.Type == DoorType.CheckpointLczA || door.Type == DoorType.CheckpointLczB)
                {
                    this.checkpointdoors.Add(door);
                    door.ChangeLock(DoorLockType.DecontLockdown);
                }
                else if (door.Type == DoorType.Scp012 || door.Type == DoorType.Scp914)
                    door.ChangeLock(DoorLockType.AdminCommand);
            }

            foreach (var room in Map.Rooms.Where(x => x.Type == RoomType.LczChkpA || x.Type == RoomType.LczChkpB))
            {
                var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.LCZ_BREAKABLE, room, new Vector3(1.5f, 0f, -1f), Vector3.up * 90f, new Vector3(3.5f, 1f, 1f), false);
                door.gameObject.SetActive(false);
                this.escapeDoors.Add(door);
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
        }

        private readonly List<Door> checkpointdoors = new List<Door>();

        private readonly List<DoorVariant> escapeDoors = new List<DoorVariant>();

        private void Server_RoundStarted()
        {
            var scpSpawn = Map.Doors.First(x => x.Type == DoorType.Scp173Gate);
            scpSpawn.ChangeLock(DoorLockType.AdminCommand);

            foreach (var player in RealPlayers.List)
            {
                if (player.Team == Team.SCP)
                {
                    player.SlowChangeRole(RoleType.Scp93953, RoleType.Scp173.GetRandomSpawnProperties().Item1);
                    player.Broadcast(8, EventManager.EMLB + this.Translations["SCP_Info"]);
                    foreach (var door in this.escapeDoors)
                    {
                        if (Server.SendSpawnMessage != null)
                        {
                            if (player.ReferenceHub.networkIdentity.connectionToClient == null)
                                continue;
                            Server.SendSpawnMessage.Invoke(null, new object[] { door.netIdentity, player.Connection });
                        }
                    }
                }
                else
                {
                    player.SlowChangeRole(RoleType.ClassD);
                    player.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"]);
                }
            }

            Timing.CallDelayed(25f, () =>
            {
                scpSpawn.ChangeLock(DoorLockType.AdminCommand);
                scpSpawn.IsOpen = true;
            });

            Timing.CallDelayed(300f, () =>
            {
                foreach (var checkpoint in this.checkpointdoors)
                {
                    checkpoint.ChangeLock(DoorLockType.DecontLockdown);
                    checkpoint.ChangeLock(DoorLockType.DecontEvacuate);
                    checkpoint.IsOpen = true;
                }
            });

            EventManager.Instance.RunCoroutine(this.UpdateWinner(), "hide_updatewinner");
        }

        private IEnumerator<float> UpdateWinner()
        {
            while (this.Active)
            {
                List<Player> winners = new List<Player>();
                foreach (var player in RealPlayers.List)
                {
                    if (player.CurrentRoom.Zone == ZoneType.HeavyContainment)
                        winners.Add(player);
                }

                if (winners.Count > 1)
                    this.OnEnd($"<color=orange>Klasa D</color> wygrywa! ({winners.Count} <color=orange>Klas D</color> uciekło)");
                else if (winners.Count != 0)
                    this.OnEnd($"<color=orange>Klasa D</color> wygrywa! ({winners[0].Nickname} uciekł)", winners[0]);
                else if (!RealPlayers.Any(Team.CDP))
                    this.OnEnd("<color=red>SCP</color> wygrywa!");
                yield return Timing.WaitForSeconds(5f);
            }
        }
    }
}
