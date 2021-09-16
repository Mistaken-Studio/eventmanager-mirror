// -----------------------------------------------------------------------
// <copyright file="SCP575.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using Mistaken.API;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class SCP575 : IEMEventClass
    {
        public override string Id => "575";

        public override string Description => "SCP-575 breach";

        public override string Name => "SCP-575";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += this.Server_RoundEnded;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= this.Server_RoundEnded;
        }

        // private bool attack = false;
        private void Server_RoundStarted()
        {
            // this.Scp575AttackScript();
            Timing.CallDelayed(UnityEngine.Random.Range(30, 90), () =>
            {
                this.Scp575LigtsScript();
            });
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            this.DeInitiate();
        }

        private void Scp575LigtsScript()
        {
            if (!this.Active)
                return;
            int time = UnityEngine.Random.Range(180, 300);
            Map.TurnOffAllLights(time);

            // this.attack = true;
            Timing.CallDelayed(time, () =>
            {
                // this.attack = false;
                Timing.CallDelayed(60, () =>
                {
                    this.Scp575LigtsScript();
                });
            });
        }

        /*private void Scp575AttackScript()
        {
            if (!this.Active)
                return;
            if (this.attack)
            {
                foreach (Player player in RealPlayers.List)
                {
                    if (player.CurrentItem.Type != ItemType.Flashlight ||
                        player.CurrentItem.Type == ItemType.GunE11SR && player.CurrentItem.modOther != 4 ||
                        player.CurrentItem.Type == ItemType.GunCOM15 && player.CurrentItem.modOther != 1 ||
                        player.CurrentItem.Type == ItemType.GunCOM18 && player.CurrentItem.modOther != 1 ||
                        player.CurrentItem.Type == ItemType.GunCrossvec && player.CurrentItem.modOther != 1)
                        player.Health -= UnityEngine.Random.Range(10, 30);
                }
            }

            Timing.CallDelayed(UnityEngine.Random.Range(10, 20), () =>
            {
                this.Scp575AttackScript();
            });
        }*/
    }
}
