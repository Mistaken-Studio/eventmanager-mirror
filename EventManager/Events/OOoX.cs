// -----------------------------------------------------------------------
// <copyright file="OOoX.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    /*internal class OOoX : IEMEventClass
    {
        public override string Id => "ooox";

        public override string Description => "One Out of X";

        public override string Name => "OOoX";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.ChangingItem += this.Player_ChangingItem;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
            Exiled.Events.Handlers.Player.Joined += this.Player_Joined;
            Exiled.Events.Handlers.Player.Left += this.Player_Left;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.ChangingItem -= this.Player_ChangingItem;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
            Exiled.Events.Handlers.Player.Joined -= this.Player_Joined;
            Exiled.Events.Handlers.Player.Left -= this.Player_Left;
        }

        private readonly Dictionary<Player, (Vector3, int)> winners = new Dictionary<Player, (Vector3, int)>();

        private readonly Vector3 center = new Vector3(135, 980, 115);

        private string[] eventWinners;

        private void Server_RoundStarted()
        {
            this.winners.Clear();
            this.eventWinners = File.ReadAllLines(EventManager.BasePath + @"\winners.txt");
            var players = RealPlayers.List.Where(x => this.eventWinners.Contains(x.UserId)).ToList();
            Vector3[] spawnPositions = new Vector3[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                double theta = (1 / players.Count) * i;
                spawnPositions[i] = new Vector3((float)(Math.Cos(theta) * 8 * this.center.x), this.center.y, (float)(Math.Sin(theta) * 8 * this.center.z));
            }

            for (int i = 0; i < players.Count; i++)
            {
                this.winners[players[i]] = (spawnPositions[i], i + 1);
                players[i].SlowChangeRole(RoleType.ClassD, spawnPositions[i]);
            }

            foreach (var admin in RealPlayers.List.Where(x => x.RemoteAdminAccess))
            {
                admin.SlowChangeRole(RoleType.Tutorial, this.center);
                admin.NoClipEnabled = true;
                admin.IsBypassModeEnabled = true;
                admin.IsGodModeEnabled = true;
                admin.AddItem(new Firearm(ItemType.GunFSP9));
                admin.AddItem(new Firearm(ItemType.GunCrossvec));
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.ClassD && !ev.Player.RemoteAdminAccess)
                Timing.CallDelayed(0.1f, () => ev.Player.GameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ);
        }

        private void Player_ChangingItem(Exiled.Events.EventArgs.ChangingItemEventArgs ev)
        {
            switch (ev.NewItem.Type)
            {
                case ItemType.GunFSP9:
                    ev.Player.SetGUI("ooox_guns", PseudoGUIPosition.BOTTOM, "Trzymasz broń do <color=yellow>eliminacji</color> graczy!");
                    break;

                case ItemType.GunCrossvec:
                    ev.Player.SetGUI("ooox_guns", PseudoGUIPosition.BOTTOM, "Trzymasz broń do <color=yellow>poprawnych odpowiedzi</color> graczy!");
                    break;

                default:
                    ev.Player.SetGUI("ooox_guns", PseudoGUIPosition.BOTTOM, null);
                    break;
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Attacker.CurrentItem?.Type == ItemType.GunFSP9)
            {
                var grenade = new Throwable(ItemType.GrenadeHE, ev.Attacker);
                grenade.Base.ThrowSettings.RandomTorqueA = Vector3.zero;
                grenade.Base.ThrowSettings.RandomTorqueB = Vector3.zero;
                var pickup = grenade.Spawn(ev.Target.Position);
                pickup.Locked = true;
                pickup.Scale = Vector3.zero;
            }
            else if (ev.Attacker.CurrentItem?.Type == ItemType.GunCrossvec)
            {
            }
        }

        private void Player_Joined(Exiled.Events.EventArgs.JoinedEventArgs ev)
        {
            if (this.eventWinners.Contains(ev.Player.UserId) && Round.IsStarted)
            {
                var players = RealPlayers.List.Where(x => this.eventWinners.Contains(x.UserId)).ToList();
                Vector3[] spawnPositions = new Vector3[players.Count];
                for (int i = 0; i < players.Count; i++)
                {
                    double theta = (1 / players.Count) * i;
                    spawnPositions[i] = new Vector3((float)(Math.Cos(theta) * 8 * this.center.x), this.center.y, (float)(Math.Sin(theta) * 8 * this.center.z));
                }

                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].Role != RoleType.ClassD)
                    {
                        this.winners[players[i]] = (spawnPositions[i], this.winners.Count + 1);
                        players[i].SlowChangeRole(RoleType.ClassD, spawnPositions[i]);
                    }
                    else
                    {
                        this.winners[players[i]] = (spawnPositions[i], this.winners[players[i]].Item2);
                        players[i].Position = spawnPositions[i];
                    }
                }
            }
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (this.eventWinners.Contains(ev.Player.UserId))
            {
                this.winners.Remove(ev.Player);
            }
        }
    }*/
}
