using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace MineBot
{
    public static class Globals
    {
        public const int DefaultPort = 25565;

        public static IPEndPoint ParseEndPoint(string arg)
        {
            IPAddress address;
            int port;
            if (arg.Contains(":"))
            {
                // Both IP and port are specified
                var parts = arg.Split(':');
                if (!IPAddress.TryParse(parts[0], out address))
                    address = Resolve(parts[0]);
                return new IPEndPoint(address, Int32.Parse(parts[1]));
            }
            if (IPAddress.TryParse(arg, out address))
                return new IPEndPoint(address, DefaultPort);
            if (Int32.TryParse(arg, out port))
                return new IPEndPoint(IPAddress.Loopback, port);
            return new IPEndPoint(Resolve(arg), DefaultPort);
        }

        private static IPAddress Resolve(string arg)
        {
            return Dns.GetHostEntry(arg).AddressList.FirstOrDefault(item => item.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}