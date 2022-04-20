// -----------------------------------------------------------------------
// <copyright file="OpositeDay.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class OpositeDay : IEMEventClass
    {
        public override string Id => "oday";

        public override string Description => "OpositeDay event";

        public override string Name => "OpositeDay";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Server.RoundEnded += this.Server_RoundEnded;
            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.GateA || door.Type == DoorType.GateB || door.Type == DoorType.Scp079First)
                {
                    door.IsOpen = true;
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Server.RoundEnded -= this.Server_RoundEnded;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            MEC.Timing.CallDelayed(1, () =>
            {
                switch (ev.NewRole)
                {
                    case RoleType.NtfPrivate:
                    case RoleType.NtfSergeant:
                    case RoleType.NtfCaptain:
                        ev.Player.Position = RoleType.Scp096.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.ChaosConscript:
                    case RoleType.ChaosMarauder:
                    case RoleType.ChaosRepressor:
                        ev.Player.Position = RoleType.Scp049.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.ChaosRifleman:
                    case RoleType.NtfSpecialist:
                        ev.Player.Position = RoleType.Scp93989.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.Scp0492:
                        ev.Player.Position = RoleType.ClassD.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.ClassD:
                        ev.Player.Position = RoleType.Scp049.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.Scientist:
                        ev.Player.Position = RoleType.Scp93953.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.FacilityGuard:
                        ev.Player.Position = Door.List.First(x => x.Type == DoorType.Scp079First).Base.transform.position + (Vector3.up * 2);
                        break;
                    case RoleType.Scp93953:
                    case RoleType.Scp93989:
                        ev.Player.Position = RoleType.Scientist.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.Scp173:
                        ev.Player.Position = RoleType.NtfPrivate.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.Scp049:
                        ev.Player.Position = RoleType.ChaosRifleman.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.Scp106:
                        ev.Player.Position = RoleType.NtfCaptain.GetRandomSpawnProperties().Item1;
                        break;
                    case RoleType.Scp096:
                        ev.Player.Position = RoleType.NtfSergeant.GetRandomSpawnProperties().Item1;
                        break;
                    default:
                        break;
                }
            });
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            this.DeInitiate();
        }
    }
}
