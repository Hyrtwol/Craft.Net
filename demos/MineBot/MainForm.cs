using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Craft.Net;
using Craft.Net.Client;
using Craft.Net.Client.Events;
using Craft.Net.Common;

namespace MineBot
{
    public partial class MainForm : Form
    {
        delegate void SetTextCallback(string text);

        private MinecraftClient _client;
        private Vector3 _seek;
        private bool _seekDirty;
        private bool _doSeek;

        public MainForm()
        {
            InitializeComponent();
            Text = Application.ProductName + " " + Application.ProductVersion;
            UpdateButtons();
        }

        private void buttonPing_Click(object sender, EventArgs e)
        {
            var endPoint = GetEndPoint();
            if (endPoint == null) return;
            var ping = ServerPing.DoPing(endPoint);

            if (ping == null)
            {
                textBoxLog.Text = null;
                MessageBox.Show(string.Format("Unable to ping {0}", endPoint));
                return;
            }
            var sb = new StringBuilder();
            sb.AppendFormat("Players: {0}/{1}", ping.Players.OnlinePlayers, ping.Players.MaxPlayers);
            sb.AppendLine();
            sb.AppendFormat("Version: {0}", ping.Version.Name);
            sb.AppendLine();
            sb.AppendFormat("ProtocolVersion: {0}", ping.Version.ProtocolVersion);
            sb.AppendLine();
            sb.AppendFormat("Description: {0}", ping.Description);
            sb.AppendLine();
            sb.AppendFormat("Latency: {0}", ping.Latency.TotalMilliseconds);
            Log(sb.ToString());
        }

        private IPEndPoint GetEndPoint()
        {
            var endPoint = Globals.ParseEndPoint(comboBox1.Text);
            if (endPoint == null) MessageBox.Show(string.Format("Unable to parse connect string {0}", comboBox1.Text));
            return endPoint;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_client != null) return;
            var endPoint = GetEndPoint();
            if (endPoint == null) return;
            var session = new Session(textBoxUserName.Text);
            _client = new MinecraftClient(session);
            _client.Connect(endPoint);
            _client.ChatMessage += ChatMessage; // (s, ea) => Log(ea.RawMessage);
            _client.Disconnected += ClientDisconnected;
            //_client.
            //_client.LoggedIn
            UpdateButtons();
            _seekDirty = true;
            timer1.Enabled = true;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (_client == null) return;
            timer1.Enabled = false;
            _client.Disconnect("Done for today");
            _client = null;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            var isConnected = _client != null;
            buttonConnect.Enabled = !isConnected;
            buttonDisconnect.Enabled = isConnected;
        }

        private void ChatMessage(object sender, ChatMessageEventArgs e)
        {
            Log(e.RawMessage);
        }

        private void ClientDisconnected(object sender, DisconnectEventArgs e)
        {
            Log("Client disconnected " +e.Reason);
        }

        private void Log(string message)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.textBoxLog.InvokeRequired)
            {
                var d = new SetTextCallback(Log);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                this.textBoxLog.Text = message;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_client != null)
            {
                var position = _client.Position;
                if (_seekDirty)
                {
                    _seek = position;
                    numericUpDown1.Value = (decimal) _seek.X;
                    numericUpDown2.Value = (decimal) _seek.Z;
                    _seekDirty = false;
                }

                if (_doSeek)
                {
                    var dif = _seek - position;
                    //var dist = _seek.DistanceTo(position);
                    var dist = dif.Distance;
                    if (dist > 0.5)
                    {
                        //_client.Move()
                        var vel = dif;
                        vel /= dist;
                        _client.Velocity = vel;
                    }
                    else
                    {
                        _doSeek = false;
                    }
                }
                label1.Text = string.Format(
                    "Pos {0} {1} {2}",
                    position.X, position.Y, position.Z);
                label2.Text = string.Format(
                    "Seek {0} {1} {2}",
                    _seek.X, _seek.Y, _seek.Z);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_client != null)
            {
                Vector3 position = _client.Position;
                position.X = (double) numericUpDown1.Value;
                position.Z = (double) numericUpDown2.Value;
                _seek = position;
                _doSeek = true;
            }
        }
    }
}
