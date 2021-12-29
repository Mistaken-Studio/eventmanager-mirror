// -----------------------------------------------------------------------
// <copyright file="AdminInvokingEventEventArgs.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;

namespace Mistaken.EventManager.EventArgs
{
    /// <summary>
    /// Contains all information after invoking the Event.
    /// </summary>
    public class AdminInvokingEventEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminInvokingEventEventArgs"/> class.
        /// </summary>
        /// <param name="sender"><inheritdoc cref="Sender"/></param>
        /// <param name="emEvent"><inheritdoc cref="EventName"/></param>
        public AdminInvokingEventEventArgs(Player sender, IEMEventClass emEvent)
        {
            this.Sender = sender;
            this.EventName = emEvent.Name;
        }

        /// <summary>
        /// Gets the admin who invoked an Event.
        /// </summary>
        public Player Sender { get; }

        /// <summary>
        /// Gets the name of the Event.
        /// </summary>
        public string EventName { get; }
    }
}
