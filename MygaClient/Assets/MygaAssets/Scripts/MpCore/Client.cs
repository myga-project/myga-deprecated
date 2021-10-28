﻿using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using MygaCross;
using System.Text;

namespace MygaClient 
{
    public static class Client
    {

        private static Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static int bufSize = 8 * 1024;
        private static State state = new State();
        private static EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private static AsyncCallback recv = null;

        public static string ip = "127.0.0.1";
        public static int port = 7777;
        public static int myId = 0;

        private static bool connected = false;

        public static void Connect(string _ip, int _port)
        {
            ip = _ip;
            port = _port;

            _socket.Connect(IPAddress.Parse(ip), port);
            Receive();

            Handler.ConnectEvents();
        }

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public static void Send(Package _package)
        {
            _socket.BeginSend(_package.ToBytes(), 0, _package.ToBytes().Length, SocketFlags.None, (ar) =>
            {
                _socket.EndSend(ar);
            }, state);
        }

        private static void Receive()
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                ClientEventSystem.PackageRecieved(so.buffer);
            }, state);
        }
    }
}