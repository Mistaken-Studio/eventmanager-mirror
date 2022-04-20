// -----------------------------------------------------------------------
// <copyright file="WDB.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Mistaken.API;
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
            Exiled.Events.Handlers.Player.UsedItem += this.Player_UsedItem;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Server_RespawningTeam;
            Exiled.Events.Handlers.Player.UsedItem -= this.Player_UsedItem;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
        }

        private readonly Dictionary<Player, bool> infected = new Dictionary<Player, bool>();

        private byte respawnCounter = 0;

        private void Server_RoundStarted()
        {
            this.infected.Clear();

            foreach (var door in Door.List.Where(x => x.Type == DoorType.GateA || x.Type == DoorType.GateB))
            {
                door.IsOpen = true;
                door.ChangeLock(DoorLockType.NoPower);
            }

            foreach (var player in RealPlayers.Get(Team.SCP))
                player.SlowChangeRole(RoleType.Scp0492, RoleType.FacilityGuard.GetRandomSpawnProperties().Item1);

            EventManager.Instance.RunCoroutine(this.UpdateInfected(), "wdb_updateinfected");

            Timing.CallDelayed(60 * 15, () =>
            {
                if (!this.Active)
                    return;
                foreach (var door in Door.List)
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

            switch (this.respawnCounter)
            {
                case 0:
                    {
                        foreach (var player in ev.Players)
                        {
                            if (converter % 1 == 0)
                                player.Position = Room.List.First(x => x.Type == RoomType.EzGateA).Position + (Vector3.up * 2);
                            else
                                player.SlowChangeRole(RoleType.Scp0492, RoleType.NtfCaptain.GetRandomSpawnProperties().Item1);

                            converter += 0.2f;
                        }
                    }

                    break;
                case 1:
                    {
                        foreach (var player in ev.Players)
                        {
                            if (converter % 1 == 0)
                                player.Position = Room.List.First(x => x.Type == RoomType.EzGateA).Position + (Vector3.up * 2);
                            else
                                player.SlowChangeRole(RoleType.Scp0492, RoleType.NtfCaptain.GetRandomSpawnProperties().Item1);

                            converter += 0.1f;
                        }
                    }

                    break;
                default:
                    {
                        ev.Players[0].Position = Room.List.First(x => x.Type == RoomType.EzGateA).Position + (Vector3.up * 2);
                        for (int i = 1; i < ev.Players.Count; i++)
                            ev.Players[i].SlowChangeRole(RoleType.Scp0492, RoleType.NtfCaptain.GetRandomSpawnProperties().Item1);
                    }

                    break;
            }

            this.respawnCounter++;
        }

        private void Player_UsedItem(Exiled.Events.EventArgs.UsedItemEventArgs ev)
        {
            if (ev.Item.Type == ItemType.SCP500)
                this.infected[ev.Player] = false;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.FacilityGuard)
                Timing.CallDelayed(1f, () => ev.Player.Position = RoleType.Scp93953.GetRandomSpawnProperties().Item1);
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Attacker.Role == RoleType.Scp0492)
            {
                ev.Amount = 1;
                this.infected[ev.Target] = true;
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (this.infected[ev.Target])
            {
                Timing.CallDelayed(1f, () => ev.Target.Role.Type = RoleType.Scp0492);
                this.infected[ev.Target] = false;
            }
        }

        private IEnumerator<float> UpdateInfected()
        {
            while (this.Active)
            {
                foreach (var player in RealPlayers.List)
                {
                    if (player.Position.y > 900 && player.Role != RoleType.Scp0492)
                            this.infected[player] = true;

                    if (this.infected[player] && !(player.ActiveEffects.Contains(new CustomPlayerEffects.Poisoned()) && player.ActiveEffects.Contains(new CustomPlayerEffects.Concussed())))
                    {
                        player.EnableEffect<CustomPlayerEffects.Poisoned>();
                        player.EnableEffect<CustomPlayerEffects.Concussed>();
                    }
                }

                yield return Timing.WaitForSeconds(2f);
            }
        }
    }
}
