using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using Lidgren.Network;

namespace OmegaRace
{
    public abstract class DataMessage
    {
        // Message Type Indentifier
        public abstract MsgType MsgID { get; set; }
        // Network Delivery Method
        public abstract NetDeliveryMethod Method { get; set; }
        // Network Sequence Channel
        public abstract int SeqChannel { get; set; }

        // Data Serializer
        public abstract void Serialize(ref BinaryWriter writer);
        // Data Deserializer
        public abstract DataMessage Deserialize(ref BinaryReader reader);
        // Print Content of Message
        public abstract void printMe();

        // Executes Message Instructions
        public abstract void Execute();
    }
}
