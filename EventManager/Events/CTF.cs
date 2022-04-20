// -----------------------------------------------------------------------
// <copyright file="CTF.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using Mistaken.API;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class CTF : IEMEventClass
    {
        public override string Id => "ctf";

        public override string Description => "Capture The Flag";

        public override string Name => "CTF";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "MTF_Task", "Jesteś członkiem <color=blue>MFO</color>. Waszym zadaniem jest przejęcie flagi drużyny przeciwnej (<color=green>CI</color>). Bazy odznaczają się obecnością <color=yellow>latarki</color> w pomieszczeniu." },
            { "MTF_Flag", "Gracz <color=green>$player</color> przejął flagę drużyny <color=blue>MFO</color>" },
            { "CI_Task", "Jesteś członkiem <color=green>CI</color>. Waszym zadaniem jest przejęcie flagi drużyny przeciwnej (<color=blue>MFO</color>). Bazy odznaczają się obecnością <color=yellow>latarki</color> w pomieszczeniu." },
            { "CI_Flag", "Gracz <color=blue>$player</color> przejął flagę drużyny <color=green>CI</color>" },
        };

        public override void OnIni()
        {
            this.tickets["MTF"] = 0;
            this.tickets["CI"] = 0;
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            var door = Door.List.First(x => x.Type == DoorType.CheckpointEntrance);
            door.ChangeLock(DoorLockType.DecontEvacuate);
            door.IsOpen = true;
            var randomroom = Room.List.Where(r => r.Type == RoomType.Hcz079 || r.Type == RoomType.Hcz106 || r.Type == RoomType.HczChkpA || r.Type == RoomType.HczChkpB).ToList();
            this.mtfRoom = randomroom[UnityEngine.Random.Range(0, randomroom.Count)];
            foreach (var room in Room.List)
            {
                if (Vector3.Distance(room.Position, this.mtfRoom.Position) >= 90 && randomroom.Contains(room) && this.ciRoom == null)
                    this.ciRoom = room;
            }

            if (this.ciRoom == null)
                this.ciRoom = Room.List.First(x => Vector3.Distance(x.Position, this.mtfRoom.Position) >= 70 && x.Zone == ZoneType.HeavyContainment);
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.PickingUpItem += this.Player_PickingUpItem;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.DroppingItem += this.Player_DroppingItem;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            this.flagCI = Item.Create(ItemType.Flashlight).Spawn(this.ciRoom.Position + (Vector3.up * 2));
            this.flagMTF = Item.Create(ItemType.Flashlight).Spawn(this.mtfRoom.Position + (Vector3.up * 2));
            foreach (var e in Exiled.API.Features.Lift.List)
            {
                if (e.Name.StartsWith("El") || e.Name.StartsWith("SCP") || e.Name == string.Empty)
                    e.IsLocked = true;
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.PickingUpItem -= this.Player_PickingUpItem;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.DroppingItem -= this.Player_DroppingItem;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private readonly Dictionary<string, int> tickets = new Dictionary<string, int>()
        {
            { "CI", 0 },
            { "MTF", 0 },
        };

        private Room ciRoom = null;

        private Room mtfRoom = null;

        private Pickup flagCI = null;

        private Pickup flagMTF = null;

        private void Server_RoundStarted()
        {
            int i = 0;
            foreach (var player in RealPlayers.RandomList)
            {
                player.SessionVariables["NO_SPAWN_PROTECT"] = true;
                if (i % 2 != 0)
                {
                    player.SlowChangeRole(this.RandomTeamRole(Team.MTF), this.mtfRoom.Position + (Vector3.up * 2));
                    player.Broadcast(8, EventManager.EMLB + this.Translations["MTF_Task"], shouldClearPrevious: true);
                }
                else
                {
                    player.SlowChangeRole(this.RandomTeamRole(Team.CHI), this.ciRoom.Position + (Vector3.up * 2));
                    player.Broadcast(8, EventManager.EMLB + this.Translations["CI_Task"], shouldClearPrevious: true);
                }

                i++;
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (this.tickets["MTF"] >= 25)
                this.OnEnd("<color=blue>MFO</color> wygrywa!");
            else if (this.tickets["CI"] >= 25)
                this.OnEnd("<color=green>CI</color> wygrywa!");
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                var team = ev.Target.Role.Team;
                if (team == Team.CHI)
                    this.tickets["MTF"] += 1;
                else
                    this.tickets["CI"] += 1;

                ev.Target.Broadcast(5, "Za chwilę się odrodzisz!");
                Timing.CallDelayed(5f, () => ev.Target.SlowChangeRole(this.RandomTeamRole(team), (team == Team.CHI ? this.ciRoom.Position : this.mtfRoom.Position) + (Vector3.up * 2)));
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(1f, () =>
            {
                if (ev.Player.Role.Team == Team.MTF)
                    ev.Player.RemoveItem(ev.Player.Items.FirstOrDefault(x => x.Type == ItemType.GrenadeHE));
            });
        }

        private void Player_PickingUpItem(Exiled.Events.EventArgs.PickingUpItemEventArgs ev)
        {
            if (ev.Pickup.Serial == this.flagMTF.Serial)
            {
                if (ev.Player.Role.Team == Team.MTF)
                {
                    ev.IsAllowed = false;
                    return;
                }

                Map.Broadcast(5, this.Translations["MTF_Flag"].Replace("$player", ev.Player.Nickname), shouldClearPrevious: true);
            }
            else if (ev.Pickup.Serial == this.flagCI.Serial)
            {
                if (ev.Player.Role.Team == Team.CHI)
                {
                    ev.IsAllowed = false;
                    return;
                }

                Map.Broadcast(5, this.Translations["CI_Flag"].Replace("$player", ev.Player.Nickname), shouldClearPrevious: true);
            }
        }

        private void Player_DroppingItem(Exiled.Events.EventArgs.DroppingItemEventArgs ev)
        {
            if (ev.Item.Serial == this.flagMTF.Serial)
            {
                if (ev.Player.CurrentRoom == this.ciRoom)
                    this.OnEnd("<color=green>CI</color> wygrywa!");
            }
            else if (ev.Item.Serial == this.flagCI.Serial)
            {
                if (ev.Player.CurrentRoom == this.mtfRoom)
                    this.OnEnd("<color=blue>MFO</color> wygrywa!");
            }
        }
    }
}
