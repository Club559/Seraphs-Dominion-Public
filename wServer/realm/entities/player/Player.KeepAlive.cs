#region

using db;
using wServer.networking.cliPackets;
using wServer.networking;

#endregion

namespace wServer.realm.entities
{
    public partial class Player
    {
        private const int PING_PERIOD = 1000;
        private const int DC_THRESOLD = 600000;

        private int updateLastSeen;

        public WorldTimer PongDCTimer { get; private set; }

        private bool KeepAlive(RealmTime time)
        {
            return true;
        }

        internal void Pong(int time, PongPacket pkt)
        {
            updateLastSeen++;

            if (updateLastSeen == 60)
            {
                if (Owner.Timers.Contains(PongDCTimer))
                    Owner.Timers.Remove(PongDCTimer);

                Owner.Timers.Add(PongDCTimer = new WorldTimer(DC_THRESOLD, (w, t) =>
                {
                    SendError("Lost connection to server.");
                    Client.Disconnect();
                }));
            }
        }
    }
}