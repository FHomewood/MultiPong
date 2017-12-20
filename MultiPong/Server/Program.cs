using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        private static byte[] _buffer = new byte[1024];
        private static byte clientNo = 0;
        private const int dedicatedVarbytes = 5;
        private static List<Client> _clientSockets = new List<Client>();
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte pukLocX256, pukLocY256, pukLocX, pukLocY;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            SetupServer();
            Console.Read();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(1);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSockets.Add(new Client(socket, clientNo, 0, 0));
            Console.WriteLine("Client {0} Connected", clientNo);
            byte[] data = new byte[] { (byte)clientNo };
            clientNo++;
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received = 0;
            received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);
            foreach (Client client in _clientSockets)
            {
                if (dataBuf[0] == client.edge)
                {
                    pukLocX256 = dataBuf[1];
                    pukLocX = dataBuf[2];
                    pukLocY256 = dataBuf[3];
                    pukLocY = dataBuf[4];
                    client.location = dataBuf[1];
                    client.score = dataBuf[2];
                }
            }
            //command list//
            byte[] data = new byte[3 * clientNo + 5];
            data[0] = clientNo;
            data[1] = (byte)(pukLocX256);
            data[2] = (byte)(pukLocX);
            data[3] = (byte)(pukLocY256);
            data[4] = (byte)(pukLocY);
            foreach (Client client in _clientSockets)
            {
                data[3 * client.edge + 5] = client.edge;
                data[3 * client.edge + 6] = client.location;
                data[3 * client.edge + 7] = client.score;
            }
            //send response//
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        private static byte[] SubArray(byte[] array, int startVal, int endval)
        {
            byte[] O = new byte[endval - startVal];
            for (int i = startVal; i < endval; i++)
            {
                O[i - startVal] = array[i];
            }


            return O;
        }
    }
}
