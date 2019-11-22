﻿using System;
namespace FaceOff
{
    public class AlertMessageEventArgs : EventArgs
    {
        public AlertMessageEventArgs(string title, string message, PlayerModel player) =>
            (Message, Title, Player) = (message, title, player);

        public string Message { get; }
        public string Title { get; }
        public PlayerModel Player { get; }
    }
}