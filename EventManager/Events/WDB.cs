// -----------------------------------------------------------------------
// <copyright file="WDB.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.EventManager.EventCreator;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class WDB : IEMEventClass
    {
        public override string Id => "wdb";

        public override string Description => "When Day Breaks";

        public override string Name => "WDB";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam += this.Server_RespawningTeam;
            Exiled.Events.Handlers.Player.ItemUsed += this.Player_ItemUsed;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Server_RespawningTeam;
            Exiled.Events.Handlers.Player.ItemUsed -= this.Player_ItemUsed;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
        }

        private readonly Dictionary<Player, bool> infected = new Dictionary<Player, bool>();

        private byte respawnCounter = 0;

        private void Server_RoundStarted()
        {
            this.infected.Clear();

            foreach (var door in Map.Doors.Where(x => x.Type == DoorType.GateA || x.Type == DoorType.GateB))
            {
                door.IsOpen = true;
                door.ChangeLock(DoorLockType.NoPower);
            }

            foreach (var player in RealPlayers.Get(Team.SCP))
            {
                player.SlowChangeRole(RoleType.Scp0492, RoleType.FacilityGuard.GetRandomSpawnProperties().Item1);
            }

            Timing.CallDelayed(60 * 20, () =>
            {
                if (!this.Active)
                    return;
                foreach (var door in Map.Doors)
                {
                    if (door.RequiredPermissions.RequiredPermissions != Interactables.Interobjects.DoorUtils.KeycardPermissions.None)
                    {
                        door.IsOpen = true;
                        door.ChangeLock(DoorLockType.NoPower);
                    }
                }
            });
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            float converter = 1f;
            API.Extensions.Extensions.Shuffle(ev.Players);

            if (this.respawnCounter == 0)
            {
                foreach (var player in ev.Players)
                {
                    if (converter % 1 == 0)
                    {
                        player.Position = Map.Rooms.First(x => x.Type == RoomType.EzGateA).Position + (Vector3.up * 2);
                    }
                    else
                    {
                        player.SlowChangeRole(RoleType.Scp0492, RoleType.NtfCaptain.GetRandomSpawnProperties().Item1);
                    }

                    converter += 0.2f;
                }
            }
            else if (this.respawnCounter == 1)
            {
                foreach (var player in ev.Players)
                {
                    if (converter % 1 == 0)
                    {
                        player.Position = Map.Rooms.First(x => x.Type == RoomType.EzGateA).Position + (Vector3.up * 2);
                    }
                    else
                    {
                        player.SlowChangeRole(RoleType.Scp0492, RoleType.NtfCaptain.GetRandomSpawnProperties().Item1);
                    }

                    converter += 0.1f;
                }
            }
            else
            {
                ev.Players[0].Position = Map.Rooms.First(x => x.Type == RoomType.EzGateA).Position + (Vector3.up * 2);
                for (int i = 1; i < ev.Players.Count; i++)
                {
                    ev.Players[i].SlowChangeRole(RoleType.Scp0492, RoleType.NtfCaptain.GetRandomSpawnProperties().Item1);
                }
            }

            this.respawnCounter++;
        }

        private void Player_ItemUsed(Exiled.Events.EventArgs.UsedItemEventArgs ev)
        {
            if (ev.Item.Type == ItemType.SCP500)
                this.infected[ev.Player] = false;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.FacilityGuard)
            {
                ev.Player.Position = RoleType.Scp93953.GetRandomSpawnProperties().Item1;
            }
        }

        private void Player_Hurting(HurtingEventArgs ev)
        {
            if (ev.Attacker.Role == RoleType.Scp0492 && !this.infected[ev.Target])
            {
                this.infected[ev.Target] = true;
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (this.infected[ev.Target])
            {
                Timing.CallDelayed(0.1f, () => { ev.Target.Role = RoleType.Scp0492; });
            }
        }
    }
}
