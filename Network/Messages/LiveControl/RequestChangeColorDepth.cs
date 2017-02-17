using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Network.Messages.LiveControl
{
    public class RequestChangeColorDepth :NovaMessage
    {
        public int Bpp { get; set; } //Message transfer Units
        public RequestChangeColorDepth(int bpp)
            : base((ushort) CustomMessageType.RequestScreenshotMessage)
        {
            Bpp = bpp;
        }

        public RequestChangeColorDepth()
            : base((ushort)CustomMessageType.RequestScreenshotMessage)
        { }
        public override void WritePayload(NetOutgoingMessage message)
        {
            base.WritePayload(message);
            message.Write(Bpp);
        }

        public override void ReadPayload(NetIncomingMessage message)
        {
            base.ReadPayload(message);
            Bpp = message.ReadInt32();
        }
    }
}
