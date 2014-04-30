using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Craft.Net.Client;

namespace MineBot.Bot
{
    public class MinerBotManager
    {
        private readonly List<EnumStateMachine<Miner>> _miners = new List<EnumStateMachine<Miner>>();
        private readonly KeyValuePair<Enum, IEnumState<Miner>>[] _states;

        public event EventHandler<string> LogMessage;

        public MinerBotManager()
        {
            _states = MinerStates.GetStates()
                                 .ToDictionary(enumState => enumState.EnumValue)
                                 .ToArray();
        }

        public int Count
        {
            get { return _miners.Count; }
        }

        public bool IsConnected
        {
            get { return _miners.Count > 0; }
        }

        public void Update()
        {
            foreach (var machine in _miners)
            {
                machine.Update();
            }
        }

        private void Log(string message)
        {
            if (LogMessage != null) LogMessage(this, message);
        }

        public void AddMiner(IPEndPoint endPoint, string name)
        {
            var session = new Session(name);
            
            var miner = new Miner(session, endPoint);
            miner.ChatMessage += (s, e) => Log(e.RawMessage);
            miner.Disconnected += (s, e) => Log("Client disconnected " + e.Reason);

            Log("Adding miner " + miner);
            var machine = new EnumStateMachine<Miner>(_states, miner);
            machine.ChangeState(ManState.Connect);
            _miners.Add(machine);
        }

        public void DisconnectAll(string reason)
        {
            foreach (var enumStateMachine in _miners)
            {
                enumStateMachine.Entity.Disconnect(reason);
            }
            _miners.Clear();
        }
    }
}