// -----------------------------------------------------------------------
// <copyright file="OOoX.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Mistaken.EventManager.EventCreator;

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
        }

        public override void OnDeIni()
        {
        }
    }
}
