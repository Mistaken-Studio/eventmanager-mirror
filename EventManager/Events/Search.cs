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
using MEC;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class Search : IEMEventClass, IWinOnLastAlive
    {
        public override string Id => "search";

        public override string Description => "Search event";

        public override string Name => "Search";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_Info", "Musisz znaleźć MicroHID'a w HCZ i uciec z nim." },
        };

        public override void OnIni()
        {
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Player.Escaping += this.Player_Escaping;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            foreach (var door in Door.List)
            {
                if (door.Nametag == string.Empty)
                    door.IsOpen = true;
                else if (door.Type != DoorType.HID)
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

            foreach (var e in Exiled.API.Features.Lift.List)
            {
                if (e.Type == ElevatorType.LczA || e.Type == ElevatorType.LczB)
                    e.IsLocked = true;
            }

            this.spawn = Room.List.First(x => x.Type == RoomType.EzGateB).transform.position + (Vector3.up * 2);
        }

        public override void OnDeIni()
        {
            Mistaken.API.Utilities.Map.Blackout.Enabled = false;
            Exiled.Events.Handlers.Player.Escaping -= this.Player_Escaping;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private Vector3 spawn;

        private void Server_RoundStarted()
        {
            Map.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"], shouldClearPrevious: true);
            switch (UnityEngine.Random.Range(0, 7))
            {
                case 0:
                    {
                        Item.Create(ItemType.MicroHID).Spawn(Door.List.First(x => x.Type == DoorType.Scp079First).Position);
                        break;
                    }

                case 1:
                    {
                        Item.Create(ItemType.MicroHID).Spawn(Door.List.First(x => x.Type == DoorType.Scp079Second).Position);
                        break;
                    }

                case 2:
                    {
                        Item.Create(ItemType.MicroHID).Spawn(Door.List.First(x => x.Type == DoorType.Scp049Armory).Position);
                        break;
                    }

                case 3:
                    {
                        Item.Create(ItemType.MicroHID).Spawn(Door.List.First(x => x.Type == DoorType.Scp096).Position);
                        break;
                    }

                case 4:
                    {
                        Item.Create(ItemType.MicroHID).Spawn(Door.List.First(x => x.Type == DoorType.Scp106Primary).Position);
                        break;
                    }

                case 5:
                    {
                        Item.Create(ItemType.MicroHID).Spawn(Door.List.First(x => x.Type == DoorType.Scp106Secondary).Position);
                        break;
                    }

                case 6:
                    {
                        Item.Create(ItemType.MicroHID).Spawn(Door.List.First(x => x.Type == DoorType.NukeArmory).Position);
                        break;
                    }
            }

            Mistaken.API.Utilities.Map.Blackout.Delay = 60;
            Mistaken.API.Utilities.Map.Blackout.Length = 30;
            Mistaken.API.Utilities.Map.Blackout.Enabled = true;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Spectator)
                return;
            ev.Player.SlowChangeRole(RoleType.ClassD, this.spawn);
            Timing.CallDelayed(1f, () => ev.Player.AddItem(ItemType.Flashlight));
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (ev.Player.Items.Any(i => i.Type == ItemType.MicroHID))
                this.OnEnd(ev.Player);
            else
                ev.IsAllowed = false;
        }
    }
}
