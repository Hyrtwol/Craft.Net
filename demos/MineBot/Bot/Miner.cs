using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Craft.Net.Client;
using Craft.Net.Common;

namespace MineBot.Bot
{
    public class Miner : MinecraftClient
    {
        public int Fatigue = 0;
        public int Hunger = 0;
        public int Idle = 0;

        //private readonly MinecraftClient _client;
        private readonly IPEndPoint _endPoint;
        public Vector3 Target;

        //public Miner(MinecraftClient client, IPEndPoint endPoint)
        //{
        //    _client = client;
        //    _endPoint = endPoint;
        //}

        public Miner(Session session, IPEndPoint endPoint)
            : base(session)
        {
            _endPoint = endPoint;
        }

        public string Name
        {
            get { return Session.UserName; }
        }

        public bool IsConnected
        {
            get { return Client != null && Client.Connected; }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} F:{1} H:{2} ID:{3}",
                Session.SelectedProfile.Name,
                Food, Health, EntityId);
        }
        
        public void Connect()
        {
            Connect(_endPoint);
        }

        public override void Disconnect(string reason)
        {
            if (Client != null && Client.Connected)
            {
                base.Disconnect(reason);
            }
        }

        public void SendChatFormat(string format, params object[] args)
        {
            SendChat(string.Format(format, args));
        }
    }
}
