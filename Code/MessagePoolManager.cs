using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    public static class MessagePoolManager
    {
        // Message Pool For InputMessages
        public static MessagePool InputPool { get; private set; }

        // Message Pool For ObjectMessages
        public static MessagePool ObjectPool { get; private set; }

        // Message Pool For EventMessages
        public static MessagePool EventPool { get; private set; }

        // Initializes Message Pools
        public static void InitializePools()
        {
            InputPool = new MessagePool(4, MsgType.INPUT);
            ObjectPool = new MessagePool(8, MsgType.OBJCT);
            EventPool = new MessagePool(2, MsgType.EVENT);
        }
    }
}
