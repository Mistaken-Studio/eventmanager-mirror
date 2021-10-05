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
using MEC;
using Mistaken.API;
using Mistaken.EventManager.EventCreator;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class OOoX : IEMEventClass
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
            Exiled.Events.Handlers.Player.Joined += this.Player_Joined;
            Exiled.Events.Handlers.Player.Left += this.Player_Left;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
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
                players[i].SlowChangeRole(RoleType.ClassD, spawnPositions[i]);
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.ClassD)
                Timing.CallDelayed(0.1f, () => ev.Player.GameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ);
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
                        players[i].SlowChangeRole(RoleType.ClassD, spawnPositions[i]);
                    else
                        players[i].Position = spawnPositions[i];
                }
            }
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (this.eventWinners.Contains(ev.Player.UserId))
            {
            }
        }
    }
}
