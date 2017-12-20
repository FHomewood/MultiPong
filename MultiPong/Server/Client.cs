using System.Net.Sockets;

namespace Server
{
    class Client
    {
        public Socket socket;
        public byte edge;
        public byte location;
        public byte score;

        public Client(Socket socket, byte edge, byte location, byte score)
        {
            this.socket = socket;
            this.edge = edge;
            this.location = location;
            this.score = score;
        }
    }
}