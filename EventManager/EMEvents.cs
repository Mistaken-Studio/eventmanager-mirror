// -----------------------------------------------------------------------
// <copyright file="EMEvents.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Mistaken.EventManager.EventArgs;
using static Exiled.Events.Events;

namespace Mistaken.EventManager
{
    /// <summary>
    /// EventManager related events.
    /// </summary>
    public static class EMEvents
    {
        /// <summary>
        /// Called when Event is invoked.
        /// </summary>
        public static event CustomEventHandler<AdminInvokingEventEventArgs> AdminInvokingEvent;

        /// <summary>
        /// Called when player wins  Event.
        /// </summary>
        public static event CustomEventHandler<PlayerWinningEventEventArgs> PlayerWinningEvent;
    }
}
