// -----------------------------------------------------------------------
// <copyright file="EventHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.Events.Extensions;
using Mistaken.EventManager.EventArgs;
using static Exiled.Events.Events;

namespace Mistaken.EventManager
{
    /// <summary>
    /// EventManager related events.
    /// </summary>
    public static class EventHandler
    {
        /// <summary>
        /// Invoked when Event is invoked.
        /// </summary>
        public static event CustomEventHandler<AdminInvokingEventEventArgs> AdminInvokingEvent;

        /// <summary>
        /// Invoked when player wins Event.
        /// </summary>
        public static event CustomEventHandler<PlayerWinningEventEventArgs> PlayerWinningEvent;

        /// <summary>
        /// Called when Event is invoked.
        /// </summary>
        /// <param name="ev">The <see cref="AdminInvokingEventEventArgs"/> instance.</param>
        public static void OnAdminInvokingEvent(AdminInvokingEventEventArgs ev)
            => AdminInvokingEvent.InvokeSafely(ev);

        /// <summary>
        /// Called when player wins Event.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerWinningEventEventArgs"/> instance.</param>
        public static void OnPlayerWinningEvent(PlayerWinningEventEventArgs ev)
            => PlayerWinningEvent.InvokeSafely(ev);
    }
}
