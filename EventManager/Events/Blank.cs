// -----------------------------------------------------------------------
// <copyright file="Blank.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class Blank : IEMEventClass
    {
        public override string Id => "blank";

        public override string Description => "Blank event";

        public override string Name => "Blank";

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
