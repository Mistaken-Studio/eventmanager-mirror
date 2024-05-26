// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using Mistaken.Updater.Config;

namespace Mistaken.EventManager
{
    internal class Config : IAutoUpdatableConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        [Description("If true then all Events that require at least some amout of players will get forced")]
        public bool DoNotCountPlayers { get; set; } = false;

        [Description("Sets the amount of days after which a new winners file is created")]
        public ushort NewWinnersFileDays { get; set; } = 14;

        [Description("If true then Events will be initiated automatically once a while")]
        public bool AutoEventsEnabled { get; set; } = false;

        [Description("Sets the amount of rounds after which AutoEvents will initialize an event")]
        public ushort AutoEventsRounds { get; set; } = 5;

        [Description("Sets the list of events, which are automatically initiated once a while (use event's id for this)")]
        public List<string> AutoEventsList { get; set; } = new List<string>()
        {
            "dbbr",
            "bdeath",
            "hp",
            "dmt",
            "whr",
            "tsl",
            "search",
            "tntb",
            "achtung",
            "ctf",

            // "cowar",
            // "titan",
        };

        [Description("Auto Update Settings")]
        public Dictionary<string, string> AutoUpdateConfig { get; set; }
    }
}
