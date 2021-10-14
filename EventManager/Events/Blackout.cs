// -----------------------------------------------------------------------
// <copyright file="Blackout.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Features;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class Blackout : IEMEventClass
    {
        public override string Id => "blackout";

        public override string Description => "Blackout event";

        public override string Name => "Blackout";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundEnded += this.Server_RoundEnded;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Map.GeneratorActivated += this.Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.ActivatingGenerator += this.Player_ActivatingGenerator;
        }

        public override void OnDeIni()
        {
            Mistaken.API.Utilities.Map.Blackout.Enabled = false;
            Exiled.Events.Handlers.Server.RoundEnded -= this.Server_RoundEnded;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Map.GeneratorActivated -= this.Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.ActivatingGenerator -= this.Player_ActivatingGenerator;
        }

        private void Server_RoundStarted()
        {
            Cassie.Message("LIGHT SYSTEM ERRROR . LIGHTS OUT", false, true);
            Mistaken.API.Utilities.Map.Blackout.Delay = 10;
            Mistaken.API.Utilities.Map.Blackout.Length = 10;
            Mistaken.API.Utilities.Map.Blackout.Enabled = true;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            MEC.Timing.CallDelayed(2, () =>
            {
                ev.Player.AddItem(ItemType.Flashlight);
            });
        }

        private void Player_ActivatingGenerator(Exiled.Events.EventArgs.ActivatingGeneratorEventArgs ev)
        {
            ev.Generator._totalActivationTime = 180;
        }

        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            if (Map.ActivatedGenerators > 2)
            {
                MEC.Timing.CallDelayed(70, () =>
                {
                    Mistaken.API.Utilities.Map.Blackout.Enabled = false;
                });
            }
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            this.DeInitiate();
        }
    }
}
