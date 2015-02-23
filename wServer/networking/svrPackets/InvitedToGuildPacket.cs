namespace wServer.networking.svrPackets
{
    public class InvitedToGuildPacket : ServerPacket
    {
        public string Guild;
        public string Name;

        public override PacketID ID
        {
            get { return PacketID.InvitedToGuild; }
        }

        public override Packet CreateInstance()
        {
            return new InvitedToGuildPacket();
        }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
            Guild = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
            wtr.WriteUTF(Guild);
        }
    }
}