using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace wServer.networking
{
    //hackish code
    internal class NetworkHandler
    {
        public const int BUFFER_SIZE = 0x10000;
        private static readonly ILog log = LogManager.GetLogger(typeof (NetworkHandler));
        private readonly Client parent;
        private readonly ConcurrentQueue<Packet> pendingPackets = new ConcurrentQueue<Packet>();
        private readonly object sendLock = new object();
        private readonly Socket skt;

        private SocketAsyncEventArgs receive;
        private byte[] receiveBuff;
        private ReceiveState receiveState = ReceiveState.Awaiting;

        private SocketAsyncEventArgs send;
        private byte[] sendBuff;
        private SendState sendState = SendState.Awaiting;

        public NetworkHandler(Client parent, Socket skt)
        {
            this.parent = parent;
            this.skt = skt;
        }

        public void BeginHandling()
        {
            skt.NoDelay = true;
            skt.UseOnlyOverlappedIO = true;

            send = new SocketAsyncEventArgs();
            send.Completed += SendCompleted;
            send.UserToken = new SendToken();
            send.SetBuffer(sendBuff = new byte[BUFFER_SIZE], 0, BUFFER_SIZE);

            receive = new SocketAsyncEventArgs();
            receive.Completed += ReceiveCompleted;
            receive.UserToken = new ReceiveToken();
            receive.SetBuffer(receiveBuff = new byte[BUFFER_SIZE], 0, BUFFER_SIZE);

            receiveState = ReceiveState.ReceivingHdr;
            receive.SetBuffer(0, 5);
            if (!skt.ReceiveAsync(receive))
                ReceiveCompleted(this, receive);
        }

        private void ProcessPolicyFile() //WUT.
        {
            var s = new NetworkStream(skt);
            var wtr = new NWriter(s);
            wtr.WriteNullTerminatedString(@"<cross-domain-policy>
     <allow-access-from domain=""*"" to-ports=""*"" />
</cross-domain-policy>");
            wtr.Write((byte) '\r');
            wtr.Write((byte) '\n');
            parent.Disconnect();
        }

        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (!skt.Connected)
                {
                    parent.Disconnect();
                    return;
                }

                if (e.SocketError != SocketError.Success)
                    throw new SocketException((int) e.SocketError);

                switch (receiveState)
                {
                    case ReceiveState.ReceivingHdr:
                        if (e.BytesTransferred < 5)
                        {
                            parent.Disconnect();
                            return;
                        }

                        if (e.Buffer[0] == 0x3c && e.Buffer[1] == 0x70 &&
                            e.Buffer[2] == 0x6f && e.Buffer[3] == 0x6c && e.Buffer[4] == 0x69)
                        {
                            ProcessPolicyFile();
                            return;
                        }

                        int len = (e.UserToken as ReceiveToken).Length =
                            IPAddress.NetworkToHostOrder(BitConverter.ToInt32(e.Buffer, 0)) - 5;
                        if (len < 0 || len > BUFFER_SIZE)
                            log.ErrorFormat("Buffer not large enough! (requested size={0})", len);
                        (e.UserToken as ReceiveToken).PacketBody = new byte[len];
                        (e.UserToken as ReceiveToken).ID = (PacketID) e.Buffer[4];

                        receiveState = ReceiveState.ReceivingBody;
                        e.SetBuffer(0, len);
                        skt.ReceiveAsync(e);

                        break;
                    case ReceiveState.ReceivingBody:
                        if (e.BytesTransferred < (e.UserToken as ReceiveToken).Length)
                        {
                            parent.Disconnect();
                            return;
                        }

                        byte[] body = (e.UserToken as ReceiveToken).PacketBody;
                        PacketID id = (e.UserToken as ReceiveToken).ID;
                        Buffer.BlockCopy(e.Buffer, 0, body, 0, body.Length);

                        receiveState = ReceiveState.Processing;
                        bool cont = OnPacketReceived(id, body);

                        if (cont && skt.Connected)
                        {
                            receiveState = ReceiveState.ReceivingHdr;
                            e.SetBuffer(0, 5);
                            skt.ReceiveAsync(e);
                        }
                        break;
                    default:
                        throw new InvalidOperationException(e.LastOperation.ToString());
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (!skt.Connected) return;

                int len;
                switch (sendState)
                {
                    case SendState.Ready:
                        len = (e.UserToken as SendToken).Packet.Write(parent, sendBuff, 0);

                        sendState = SendState.Sending;
                        e.SetBuffer(0, len);
                        skt.SendAsync(e);
                        break;
                    case SendState.Sending:
                        (e.UserToken as SendToken).Packet = null;

                        if (CanSendPacket(e, true))
                        {
                            len = (e.UserToken as SendToken).Packet.Write(parent, sendBuff, 0);

                            sendState = SendState.Sending;
                            e.SetBuffer(0, len);
                            skt.SendAsync(e);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }


        private void OnError(Exception ex)
        {
            log.Error("Socket error.", ex);
            parent.Disconnect();
        }

        private bool OnPacketReceived(PacketID id, byte[] pkt)
        {
            if (parent.IsReady())
            {
                parent.Manager.Network.AddPendingPacket(parent, id, pkt);
                return true;
            }
            return false;
        }

        private bool CanSendPacket(SocketAsyncEventArgs e, bool ignoreSending)
        {
            lock (sendLock)
            {
                if (sendState == SendState.Ready ||
                    (!ignoreSending && sendState == SendState.Sending))
                    return false;
                Packet packet;
                if (pendingPackets.TryDequeue(out packet))
                {
                    (e.UserToken as SendToken).Packet = packet;
                    sendState = SendState.Ready;
                    return true;
                }
                sendState = SendState.Awaiting;
                return false;
            }
        }

        public void SendPacket(Packet pkt)
        {
            if (!skt.Connected) return;
            pendingPackets.Enqueue(pkt);
            if (CanSendPacket(send, false))
            {
                int len = (send.UserToken as SendToken).Packet.Write(parent, sendBuff, 0);

                sendState = SendState.Sending;
                send.SetBuffer(sendBuff, 0, len);
                if (!skt.SendAsync(send))
                    SendCompleted(this, send);
            }
        }

        public void SendPackets(IEnumerable<Packet> pkts)
        {
            if (!skt.Connected) return;
            foreach (Packet i in pkts)
                pendingPackets.Enqueue(i);
            if (CanSendPacket(send, false))
            {
                int len = (send.UserToken as SendToken).Packet.Write(parent, sendBuff, 0);

                sendState = SendState.Sending;
                send.SetBuffer(sendBuff, 0, len);
                if (!skt.SendAsync(send))
                    SendCompleted(this, send);
            }
        }

        private enum ReceiveState
        {
            Awaiting,
            ReceivingHdr,
            ReceivingBody,
            Processing
        }

        private class ReceiveToken
        {
            public PacketID ID;
            public int Length;
            public byte[] PacketBody;
        }

        private enum SendState
        {
            Awaiting,
            Ready,
            Sending
        }

        private class SendToken
        {
            public Packet Packet;
        }
    }
}