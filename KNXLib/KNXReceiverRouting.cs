﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace KNXLib
{
    internal class KNXReceiverRouting : KnxReceiver
    {
        #region constructor
        internal KNXReceiverRouting(KnxConnectionRouting connection, IList<UdpClient> udpClients, IPEndPoint localEndpoint)
            : base(connection)
        {
            this.LocalEndpoint = localEndpoint;
            this.UdpClients = udpClients;
        }
        #endregion

        #region variables
        private IPEndPoint _localEndpoint;
        private IPEndPoint LocalEndpoint
        {
            get
            {
                return this._localEndpoint;
            }
            set
            {
                this._localEndpoint = value;
            }
        }

        private IList<UdpClient> _udpClients;
        private IList<UdpClient> UdpClients
        {
            get
            {
                return this._udpClients;
            }
            set
            {
                this._udpClients = value;
            }
        }
        #endregion

        #region thread
        public override void ReceiverThreadFlow()
        {
            try
            {
                foreach (UdpClient client in this.UdpClients)
                {
                    client.BeginReceive(OnReceive, new object[] {client, client.Client.LocalEndPoint});
                }
                //byte[] dgram;
                while (true)
                {
                    // just wait to be aborted
                    Thread.Sleep(60000);
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception)
            {
                // ignore exception and exit
            }
        }
        #endregion

        #region Async receive

        private void OnReceive(IAsyncResult result)
        {
            IPEndPoint ep = null;
            var args = (object[])result.AsyncState;
            var session = (UdpClient)args[0];
            var local = (IPEndPoint)args[1];

            try
            {
                byte[] dgram = session.EndReceive(result, ref ep);
                ProcessDatagram(dgram);

                //We make the next call to the begin receive
                session.BeginReceive(OnReceive, args);
            }
            catch (ObjectDisposedException)
            {
                // ignore and exit, session was disposed
            }
        }

        #endregion

        #region datagram processing
        public override void ProcessDatagram(byte[] datagram)
        {
            try
            {
                ProcessDatagramHeaders(datagram);
            }
            catch (Exception)
            {
                // ignore, missing warning information
            }
        }

        private void ProcessDatagramHeaders(byte[] dgram)
        {
            // HEADER
            KnxDatagram datagram = new KnxDatagram();
            datagram.header_length = (int)dgram[0];
            datagram.protocol_version = dgram[1];
            datagram.service_type = new byte[] { dgram[2], dgram[3] };
            datagram.total_length = (int)dgram[4] + (int)dgram[5];

            byte[] cemi = new byte[dgram.Length - 6];
            Array.Copy(dgram, 6, cemi, 0, dgram.Length - 6);

            base.ProcessCEMI(datagram, cemi);
        }
        #endregion
    }
}
