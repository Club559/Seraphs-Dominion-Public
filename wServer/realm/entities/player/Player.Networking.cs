using System;
using System.Collections.Generic;
using wServer.networking;

namespace wServer.realm.entities
{
    partial class Player
    {
        private readonly Queue<Tuple<Packet, Predicate<Player>>> pendingPackets =
            new Queue<Tuple<Packet, Predicate<Player>>>();

        internal void Flush()
        {
            foreach (Player i in Owner.Players.Values)
                foreach (var j in pendingPackets)
                    if (j.Item2(i))
                        i.client.SendPacket(j.Item1);
            pendingPackets.Clear();
        }

        public void BroadcastSync(Packet packet) //sync at Move
        {
            BroadcastSync(packet, _ => true);
        }

        public void BroadcastSync(Packet packet, Predicate<Player> cond)
        {
            pendingPackets.Enqueue(Tuple.Create(packet, cond));
        }

        public void BroadcastSync(IEnumerable<Packet> packets)
        {
            foreach (Packet i in packets)
                BroadcastSync(i, _ => true);
        }

        public void BroadcastSync(IEnumerable<Packet> packets, Predicate<Player> cond)
        {
            foreach (Packet i in packets)
                BroadcastSync(i, cond);
        }
    }
}