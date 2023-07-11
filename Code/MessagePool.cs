using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    public class MessagePool
    {
        private Queue<DataMessage> messagePool;

        // Initializes Message Pool With a Certain Message Type and Capacity
        public MessagePool(int maxCapacity, MsgType msgType)
        {
            messagePool = new Queue<DataMessage>(maxCapacity);

            for(int i = 0; i < maxCapacity; i++)
            {
                messagePool.Enqueue(createDataMessage(msgType));
            }
        }

        // Returns Any Available DatMessage Object in MesssagePool
        public DataMessage Get(MsgType msgType)
        {
            if (messagePool.Count == 0)
                return createDataMessage(msgType);

            return messagePool.Dequeue();
        }

        // Recycles DataMessage Object
        public void Recycle(DataMessage message)
        {
            messagePool.Enqueue(message);
        }

        // Creates and Adds DataMessage of Specific Type to MessagePool 
        private DataMessage createDataMessage(MsgType msgType)
        {
            switch (msgType)
            {
                case MsgType.EVENT:
                    return new EventMessage();

                case MsgType.INPUT:
                    return new InputMessage();

                case MsgType.OBJCT:
                    return new ObjectMessage();

                case MsgType.TIME:
                    return new TimeMessage();
            }
            return null;
        }
    }
}
