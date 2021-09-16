// -----------------------------------------------------------------------
// <copyright file="Search.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
using Interactables.Interobjects.DoorUtils;
using Mistaken.API;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class Search : IEMEventClass,
        ISpawnRandomItems,
        IWinOnLastAlive
    {
        public override string Id => "search";

        public override string Description => "Search event";

        public override string Name => "Search";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D", "Musisz znaleźć MicroHID'a w HCZ i uciec z nim." },
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Player.Escaping += this.Player_Escaping;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
        }

        public override void OnDeIni()
        {
            Mistaken.API.Utilities.Map.Blackout.Enabled = false;
            Exiled.Events.Handlers.Player.Escaping -= this.Player_Escaping;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private void Server_RoundStarted()
        {
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            var doors = Map.Doors;
            foreach (var door in doors)
            {
                var doorType = door.Type;
                if (door.Nametag == string.Empty) door.IsOpen = true;
                else if (doorType != DoorType.HID)
                {
                    door.IsOpen = true;
                    door.ChangeLock(DoorLockType.Warhead);
                }
                else
                {
                    door.IsOpen = false;
                    door.ChangeLock(DoorLockType.Warhead);
                }
            }

            foreach (var e in Map.Lifts)
            {
                var elevatorType = e.Type();
                if (elevatorType == ElevatorType.LczA || elevatorType == ElevatorType.LczB) e.Network_locked = true;
            }

            Map.Broadcast(10, EventManager.EMLB + this.Translations["D"]);
            foreach (var player in RealPlayers.List)
                player.Role = RoleType.ClassD;
            int rand = UnityEngine.Random.Range(0, 7);
            switch (rand)
            {
                case 0:
                    {
                        new Item(ItemType.MicroHID).Spawn(doors.First(d => d.Type == DoorType.Scp079First).Position);
                        break;
                    }

                case 1:
                    {
                        new Item(ItemType.MicroHID).Spawn(doors.First(d => d.Type == DoorType.Scp079Second).Position);
                        break;
                    }

                case 2:
                    {
                        new Item(ItemType.MicroHID).Spawn(doors.First(d => d.Type == DoorType.Scp049Armory).Position);
                        break;
                    }

                case 3:
                    {
                        new Item(ItemType.MicroHID).Spawn(doors.First(d => d.Type == DoorType.Scp096).Position);
                        break;
                    }

                case 4:
                    {
                        new Item(ItemType.MicroHID).Spawn(doors.First(d => d.Type == DoorType.Scp106Primary).Position);
                        break;
                    }

                case 5:
                    {
                        new Item(ItemType.MicroHID).Spawn(doors.First(d => d.Type == DoorType.Scp106Secondary).Position);
                        break;
                    }

                case 6:
                    {
                        new Item(ItemType.MicroHID).Spawn(doors.First(d => d.Type == DoorType.NukeArmory).Position);
                        break;
                    }
            }

            Mistaken.API.Utilities.Map.Blackout.Delay = 60;
            Mistaken.API.Utilities.Map.Blackout.Length = 30;
            Mistaken.API.Utilities.Map.Blackout.Enabled = true;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            MEC.Timing.CallDelayed(1f, () => ev.Player.Position = RoleType.NtfCaptain.GetRandomSpawnProperties().Item1);
        }

        private void Player_Escaping(EscapingEventArgs ev)
        {
            Log.Debug(ev.Player.Id);
            foreach (var item in ev.Player.Items)
            {
                Log.Debug(item.Type);
            }

            Log.Debug(ev.Player.Items.Any(i => i.Type == ItemType.MicroHID).ToString());
            if (ev.Player.Items.Any(i => i.Type == ItemType.MicroHID))
                this.OnEnd(ev.Player.Nickname);
            else
                ev.IsAllowed = false;
        }
    }
}
