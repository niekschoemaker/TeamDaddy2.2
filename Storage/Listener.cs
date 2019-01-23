﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Console = System.Console;
using Google.Protobuf;

namespace unwdmi.Storage
{

    class ListenerParser
    {

        public ListenerParser(Controller instance)
        {
            _controller = instance;
        }

        private readonly Controller _controller;

        private Socket _listener;

        public void StartListening()
        {
            var minecraft = 25565;

            IPEndPoint ip = new IPEndPoint(IPAddress.Any, minecraft);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Blocking = false;
            _listener.Bind(ip);
            _listener.Listen(200);
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);
        }


        void AcceptCallBack(IAsyncResult ar)
        {
            // Program can act weird if all 800 connections are opened at once.
            // In real-life applications this shouldn't be a problem.
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject stateObject = new StateObject
            {
                workSocket = handler
            };
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0, ReceiveCallback, stateObject);
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            var so = (StateObject)ar.AsyncState;
            TcpListener server = (TcpListener)ar.AsyncState;

            using (TcpClient client = server.EndAcceptTcpClient(ar))
            using (NetworkStream stream = client.GetStream())
            {
                try
                {
                    //TODO
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }
    }

    class ListenerWeb
    {
        public ListenerWeb(Controller instance)
        {
            _controller = instance;
        }

        private readonly Controller _controller;

        private Socket _listener;

        public void StartListening()
        {
            var minecraft = 25566;

            IPEndPoint ip = new IPEndPoint(IPAddress.Any, minecraft);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Blocking = false;
            _listener.Bind(ip);
            _listener.Listen(200);
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);
        }


        void AcceptCallBack(IAsyncResult ar)
        {
            // Program can act weird if all 800 connections are opened at once.
            // In real-life applications this shouldn't be a problem.
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject stateObject = new StateObject
            {
                workSocket = handler
            };
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0, ReceiveCallback, stateObject);
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            var so = (StateObject)ar.AsyncState;
            var workSocket = so.workSocket;

            /*
             *  TODO: Functie die de ProtoBuf-stream van web ontvangt en opslaat
             */

            try
            {
                var read = workSocket.EndReceive(ar);

                if (read > 0)
                {
                    var _str = Encoding.ASCII.GetString(so.buffer, 0, read);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


        }


    }

    public class StateObject
    {
        public Socket workSocket;
        // 4096 fits all the XML files, so the checks don't have to be done as often, saves a bit of CPU, costs a bit more ram.
        public const int BUFFER_SIZE = 4096;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
    }
}
