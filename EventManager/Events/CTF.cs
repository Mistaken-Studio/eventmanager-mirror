// -----------------------------------------------------------------------
// <copyright file="CTF.cs" company="Mistaken">
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
using Mistaken.EventManager.EventCreator;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    /*internal class CTF : IEMEventClass
    {
        public override string Id => "ctf";

        public override string Description => "Capture The Flag";

        public override string Name => "CTF";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "TaskMTF", "Jesteś członkiem <color=blue>MFO</color>. Waszym zadaniem jest przejęcie flagi drużyny przeciwnej (<color=green>CI</color>). Bazy odznaczają się obecnością <color=yellow>latarki</color> w pomieszczeniu." },
            { "FlagMTF", "Gracz <color=green>$player</color> przejął flagę drużyny <color=blue>MFO</color>" },
            { "TaskCI", "Jesteś członkiem <color=green>CI</color>. Waszym zadaniem jest przejęcie flagi drużyny przeciwnej (<color=blue>MFO</color>). Bazy odznaczają się obecnością <color=yellow>latarki</color> w pomieszczeniu." },
            { "FlagCI", "Gracz <color=blue>$player</color> przejął flagę drużyny <color=green>CI</color>" },
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.PickingUpItem += this.Player_PickingUpItem;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.DroppingItem += this.Player_DroppingItem;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.PickingUpItem -= this.Player_PickingUpItem;
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

        private void Server_RoundStarted()
        {
            this.tickets["MTF"] = 0;
            this.tickets["CI"] = 0;
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var e in Map.Lifts)
            {
                if (e.elevatorName.StartsWith("El") || e.elevatorName.StartsWith("SCP") || e.elevatorName == string.Empty) e.Network_locked = true;
            }

            var door = Map.Doors.First(x => x.Type == DoorType.CheckpointEntrance);
            door.ChangeLock(DoorLockType.DecontEvacuate);
            door.IsOpen = true;
            int i = 0;
            var rooms = Map.Rooms.ToList();
            var randomroom = rooms.Where(r => r.Type == RoomType.Hcz079 || r.Type == RoomType.Hcz106 || r.Type == RoomType.HczChkpA || r.Type == RoomType.HczChkpB).ToList();
            this.mtfRoom = randomroom[UnityEngine.Random.Range(0, randomroom.Count)];
            foreach (var room in rooms)
            {
                if (Vector3.Distance(room.Position, this.mtfRoom.Position) >= 90 && randomroom.Contains(room) && this.ciRoom == null)
                    this.ciRoom = room;
            }

            if (this.ciRoom == null) this.ciRoom = rooms.First(r => Vector3.Distance(r.Position, this.mtfRoom.Position) >= 70 && r.Zone == ZoneType.HeavyContainment);

            ItemType.Flashlight.Spawn(1, this.mtfRoom.Position);
            ItemType.Flashlight.Spawn(2, this.ciRoom.Position);
            foreach (var player in RealPlayers.RandomList)
            {
                player.SessionVariables["NO_SPAWN_PROTECT"] = true;
                if (i % 2 != 0)
                {
                    player.SlowChangeRole(RoleType.NtfSergeant, this.mtfRoom.Position + (Vector3.up * 2));
                    player.Broadcast(10, this.Translations["TaskMTF"]);
                }
                else
                {
                    player.SlowChangeRole(RoleType.ChaosRifleman, this.ciRoom.Position + (Vector3.up * 2));
                    player.Broadcast(10, this.Translations["TaskCI"]);
                }

                i++;
            }

            MEC.Timing.RunCoroutine(this.UpdateWinner());
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                var role = ev.Target.Role;
                if (role == RoleType.ChaosRifleman)
                    this.tickets["MTF"] += 1;
                else
                    this.tickets["CI"] += 1;
                if (this.tickets["MTF"] >= 35)
                    this.OnEnd("<color=blue>MFO</color> wygrywa!", true);
                else if (this.tickets["CI"] >= 35)
                    this.OnEnd("<color=green>CI</color> wygrywa!", true);

                ev.Target.Broadcast(5, "Za chwilę się odrodzisz!");
                MEC.Timing.CallDelayed(5f, () =>
                {
                    ev.Target.SlowChangeRole(role, (role == RoleType.ChaosRifleman ? this.ciRoom.Position : this.mtfRoom.Position) + Vector3.up);
                });
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player.Role == RoleType.NtfSergeant)
                MEC.Timing.CallDelayed(1f, () => ev.Player.RemoveItem(ev.Player.Items.First(x => x.Type == ItemType.GrenadeHE)));
        }

        private void Player_PickingUpItem(Exiled.Events.EventArgs.PickingUpItemEventArgs ev)
        {
            if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 1 && player.Role == RoleType.NtfLieutenant) { ev.IsAllowed = false; return; }
            else if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 2 && player.Role == RoleType.ChaosInsurgency) { ev.IsAllowed = false; return; }
            if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 1) Map.Broadcast(5, Translations["FlagMTF"].Replace("$player", player.Nickname));
            else if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 2) Map.Broadcast(5, Translations["FlagCI"].Replace("$player", player.Nickname)); ;
            if (ev.Pickup.Type != ItemType.Flashlight)
                return;
            switch (ev.Pickup.durability)
            {
                case 1:
                    if (ev.Player.Role == RoleType.NtfSergeant)
                    {
                        ev.IsAllowed = false;
                        return;
                    }

                    Map.Broadcast(5, this.Translations["FlagMTF"].Replace("$player", ev.Player.Nickname));
                    break;
                case 2:
                    if (ev.Player.Role == RoleType.ChaosRifleman)
                    {
                        ev.IsAllowed = false;
                        return;
                    }

                    Map.Broadcast(5, this.Translations["FlagCI"].Replace("$player", ev.Player.Nickname));
                    break;
            }
        }

        private void Player_DroppingItem(Exiled.Events.EventArgs.DroppingItemEventArgs ev)
        {
            if (ev.Item.Type == ItemType.Flashlight && ev.Item.durability == 1)
            {
                Map.ClearBroadcasts();
                Map.Broadcast(4, this.Translations["FlagMTF"].Replace("$player", ev.Player.Nickname));
                if (ev.Player.CurrentRoom == this.ciRoom) this.OnEnd("<color=green>CI</color> wygrywa!", true);
            }
            else if (ev.Item.Type == ItemType.Flashlight && ev.Item.durability == 2)
            {
                Map.ClearBroadcasts();
                Map.Broadcast(4, this.Translations["FlagCI"].Replace("$player", ev.Player.Nickname));
                if (ev.Player.CurrentRoom == this.mtfRoom) this.OnEnd("<color=blue>MFO</color> wygrywa!", true);
            }
        }

        private IEnumerator<float> UpdateWinner()
        {
            while (this.Active)
            {
                yield return Timing.WaitForSeconds(5f);
                foreach (var player in RealPlayers.List)
                {
                    foreach (var item in player.Items)
                    {
                        if (item.Type == ItemType.Flashlight)
                        {
                            switch (item.durability)
                            {
                                case 1:
                                    Map.ClearBroadcasts();
                                    Map.Broadcast(5, this.Translations["FlagMTF"].Replace("$player", player.Nickname));
                                    if (player.CurrentRoom == this.ciRoom) this.OnEnd("<color=green>CI</color> wygrywa!", true);
                                    break;
                                case 2:
                                    Map.ClearBroadcasts();
                                    Map.Broadcast(5, this.Translations["FlagCI"].Replace("$player", player.Nickname));
                                    if (player.CurrentRoom == this.mtfRoom) this.OnEnd("<color=blue>MFO</color> wygrywa!", true);
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }*/
}
