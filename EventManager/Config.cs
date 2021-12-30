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
    /// <inheritdoc/>
    public class Config : IAutoUpdatableConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug should be displayed.
        /// </summary>
        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether em force command should not count amount of players.
        /// </summary>
        [Description("If true then em force command won't count amount of players")]
        public bool Dnpn { get; set; }

        /// <summary>
        /// Gets or sets a path for the EventManager folder.
        /// </summary>
        [Description("Sets the path for the EventManager folder. If not set, it will be created in Plugins folder")]
        public string FolderPath { get; set; }

        /// <summary>
        /// Gets or sets a value after which a new winners file is created.
        /// </summary>
        [Description("Sets the amount of days after which a new winners file is created")]
        public ushort NewWinnersFileDays { get; set; } = 14;

        /// <summary>
        /// Gets or sets a value indicating whether AutoEvents should be enabled.
        /// </summary>
        [Description("If true then Events will be initiated automatically once a while")]
        public bool AutoEventsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a value after which how many rounds without events to start an event.
        /// </summary>
        [Description("Sets the amount of rounds after which AutoEvents will initialize an event")]
        public ushort AutoEventsRounds { get; set; } = 4;

        /// <summary>
        /// Gets or sets a list of automatic events.
        /// </summary>
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
            "cowar",
            "ctf",

            // "titan",
        };

        /// <inheritdoc/>
        [Description("Auto Update Settings")]
        public Dictionary<string, string> AutoUpdateConfig { get; set; }
    }
}
