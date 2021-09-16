// -----------------------------------------------------------------------
// <copyright file="Hide.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.EventManager.EventCreator;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class Hide : IEMEventClass,
        IAnnouncePlayersAlive
    {
        public override string Id => "hide";

        public override string Description => "Hide event";

        public override string Name => "Hide";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_Info", "Twoim zadaniem jest ucieczka do <color=grey>HCZ</color>. <color=lime>Checkpointy</color> otwierają się za 8 minut, w międzyczasie ukryj się przed <color=red>SCP-939</color>." },
            { "S_Info", "Twoim zadaniem jest znalezienie <color=orange>klas D</color> i ich zabicie." },
        };

        public bool ClearPrevious => true;

        public override void OnIni()
        {
            MapGeneration.InitiallySpawnedItems.Singleton.ClearAll();
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
        }

        private readonly List<Door> checkpointdoors = new List<Door>();

        private readonly List<DoorVariant> escapeDoors = new List<DoorVariant>();

        private void Server_RoundStarted()
        {
            this.escapeDoors.Clear();
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var door in Map.Doors)
            {
                var type = door.Type;
                if (type == DoorType.CheckpointLczA || type == DoorType.CheckpointLczB)
                    this.checkpointdoors.Add(door);
                else if (type == DoorType.Scp012 || type == DoorType.Scp914)
                    door.ChangeLock(DoorLockType.AdminCommand);
            }

            foreach (var checkpoint in this.checkpointdoors)
                checkpoint.ChangeLock(DoorLockType.DecontLockdown);
            var scpSpawn = Map.Doors.First(x => x.Type == DoorType.Scp173Gate);
            scpSpawn.ChangeLock(DoorLockType.AdminCommand);
            foreach (var room in Map.Rooms.Where(x => x.Type == RoomType.LczChkpA || x.Type == RoomType.LczChkpB))
            {
                var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.LCZ_BREAKABLE, room, new Vector3(1.5f, 0f, -1f), Vector3.up * 90f, new Vector3(3.5f, 1f, 1f), false);
                door.gameObject.SetActive(false);
                this.escapeDoors.Add(door);
            }

            foreach (var player in RealPlayers.List)
            {
                if (player.Side == Side.Scp)
                {
                    player.SlowChangeRole(RoleType.Scp93953, RoleType.Scp173.GetRandomSpawnProperties().Item1);
                    player.Broadcast(8, EventManager.EMLB + this.Translations["S_Info"]);
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

            MEC.Timing.CallDelayed(25f, () =>
            {
                scpSpawn.ChangeLock(DoorLockType.AdminCommand);
                scpSpawn.IsOpen = true;
            });
            MEC.Timing.CallDelayed(455f, () =>
            {
                foreach (var checkpoint in this.checkpointdoors)
                {
                    checkpoint.ChangeLock(DoorLockType.DecontLockdown);
                    checkpoint.ChangeLock(DoorLockType.DecontEvacuate);
                    checkpoint.IsOpen = true;
                }
            });
            MEC.Timing.RunCoroutine(this.UpdateWinner());
        }

        private IEnumerator<float> UpdateWinner()
        {
            while (this.Active)
            {
                yield return MEC.Timing.WaitForSeconds(7.5f);
                List<Player> winners = new List<Player>();
                foreach (var player in RealPlayers.List)
                {
                    if (player.CurrentRoom.Zone == ZoneType.HeavyContainment) winners.Add(player);
                }

                if (winners.Count > 1)
                    this.OnEnd($"<color=orange>Klasa D</color> wygrywa! ({winners.Count} <color=orange>Klas D</color> uciekło)", true);
                else if (winners.Count != 0)
                    this.OnEnd($"<color=orange>Klasa D</color> wygrywa! ({winners[0].Nickname} uciekł)", true);
                else if (!RealPlayers.Any(Team.CDP))
                    this.OnEnd("SCP");
            }
        }
    }
}
