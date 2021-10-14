// -----------------------------------------------------------------------
// <copyright file="PlayerWinningEventEventArgs.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.EventArgs
{
    /// <summary>
    /// Contains all information before Event gets deinitiated.
    /// </summary>
    public class PlayerWinningEventEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerWinningEventEventArgs"/> class.
        /// </summary>
        /// <param name="winner"><inheritdoc cref="Winner"/></param>
        /// <param name="emEvent"><inheritdoc cref="EventName"/></param>
        public PlayerWinningEventEventArgs(Player winner, IEMEventClass emEvent)
        {
            this.Winner = winner;
            this.EventName = emEvent.Name;
        }

        /// <summary>
        /// Gets the winner of the Event.
        /// </summary>
        public Player Winner { get; }

        /// <summary>
        /// Gets the name of the Event.
        /// </summary>
        public string EventName { get; }
    }
}
