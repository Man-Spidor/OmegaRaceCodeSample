using System;
using System.Diagnostics;
using Lidgren.Network;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;

namespace OmegaRace
{
    public static class NetworkManager
    {
        //  ********************************************************************************************************************
        //  * 3 Different Modes (Mostly For Testing Purposes)                                                                  *
        //  *                                                                                                                  *
        //  * 1.) Local: Searches for server on localhost.                                                                     *
        //  *                                                                                                                  *
        //  * 2.) Networked: Searches for server on given IP Address.                                                          *
        //  *                                                                                                                  *
        //  * 3.) Playback: Does not search for server, only used when using program's "Playback" feature. A feature which     *
        //  *     allows you to replay a previously played game to check for errors.                                           *
        //  ********************************************************************************************************************

        public enum Mode
        {
            Local,
            Networked,
            Playback
        }

        private const int serverPort = 14240;
        private const string serverAddress = "192.168.0.1";
        private static NetClient client;
        private static Mode mode;

        public static void setMode(Mode _mode)
        {
            mode = _mode;
        }

        // Initializes Server If Not In Playback Mode

        public static void InitServer()
        {
            if (mode != Mode.Playback)
            {
                NetPeerConfiguration config = new NetPeerConfiguration("Connection Test");
                config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);

                client = new NetClient(config);
                client.Start();

                IPEndPoint ep = null;

                // Connects to proper server
                if (mode == Mode.Local)
                    ep = NetUtility.Resolve("localhost", serverPort);
                else
                    ep = NetUtility.Resolve(serverAddress, serverPort);

                client.DiscoverKnownPeer(ep);

                // Console.WriteLine("Client Connected!");
            }
        }

        //  ********************************************************************************************************************
        //  * Sends a DataMessage to the client.                                                                               *
        //  *                                                                                                                  *
        //  * It starts by initializing a byte array, MemoryStream, and a BinaryWriter.                                        *
        //  *                                                                                                                  *
        //  * Next, it serializes the message's data into the MemoryStream using the BinaryWriter.                             *
        //  *                                                                                                                  *
        //  * Then, the data is stored in the byte array and written into an outgoing network message and sent using the       *
        //  * message's specific instructions.                                                                                 *
        //  *                                                                                                                  *
        //  * Finally, it sends the NetOutgoingMessage to the client.                                                          *
        //  ********************************************************************************************************************

        public static void SendMessage(DataMessage message)
        {
            byte[] data;

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            message.Serialize(ref writer);

            data = stream.ToArray();
            NetOutgoingMessage om = client.CreateMessage(data.Length);
            om.Write(data);

            client.SendMessage(om, message.Method, message.SeqChannel);

            // Console.Write("Sent Message: ");
            // message.printMe();
        }

        //  ********************************************************************************************************************
        //  * Processes incoming network messages and returns a DataMessage object.                                            *                                        
        //  *                                                                                                                  *
        //  * Within the loop, it switches on the "MessageType" of the incoming message, "im".                                 *
        //  *                                                                                                                  *
        //  * The different cases in the switch statement represent different message types and perform specific actions:      *
        //  *     - NetIncomingMessageType.DiscoveryResponse: Handles the response when the client discovers a server.         *
        //  *                                                                                                                  *
        //  *     - NetIncomingMessageType.StatusChanged: Handles changes in the client's connection status.                   *                                                        
        //  *                                                                                                                  *
        //  *     - NetIncomingMessageType.Data: Handles data messages received by the client.                                 *
        //  *                                                                                                                  *
        //  * Once the loop ends, the method returns the deserialized message received by the client or null depending on the  *
        //  * incoming message.                                                                                                *
        //  ********************************************************************************************************************

        public static DataMessage ProcessIncoming()
        {
            DataMessage returnMessage = null;
            if (mode != Mode.Playback)
            {
                NetIncomingMessage im;

                while ((im = client.ReadMessage()) != null)
                {
                    // Console.WriteLine("Recieved Message!");

                    switch (im.MessageType)
                    {
                        //**********************************************************************************

                        // The client has found a server 
                        case NetIncomingMessageType.DiscoveryResponse:
                            // Console.WriteLine("Found server at " + im.SenderEndPoint + " name: " + im.ReadString());
                            client.Connect(im.SenderEndPoint);
                            break;

                        //**********************************************************************************

                        // The client's connection status changed
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                            // Console.WriteLine("Connection status changed: " + status.ToString() + ": " + im.ReadString());
                            if (status == NetConnectionStatus.Connected)
                            {
                                MessageFactory.CreateTimeMessage(MsgType_Time.TIME_REQUEST, 0, 10);
                            }
                            break;

                        //**********************************************************************************

                        // The client recieved a Datamessage packet
                        case NetIncomingMessageType.Data:
                            returnMessage = ProcessDataMessage(im);
                            // Console.WriteLine("Data message received");
                            break;

                        //**********************************************************************************
                    }

                    client.Recycle(im);
                }
            }
            return returnMessage;
        }

        //  ********************************************************************************************************************
        //  * Reads the bytes from the NetIncomingMessage into a byte array. Then, uses a BinaryReader using a MemoryStream    *
        //  * initialized with the byte array.                                                                                 *
        //  *                                                                                                                  *
        //  * Next, it initializes a DataMessage object as an ObjectMessage.                                                   *
        //  *                                                                                                                  *
        //  * The method then retrieves the message ID (msgID) from the first byte of the byte array and converts it to the    *
        //  * MsgType enumeration.                                                                                             *
        //  *                                                                                                                  *
        //  * Checks the MsgType value and performs the following actions based on the message type:                           *
        //  *     - MsgType.INPUT: Creates an InputMessage object and deserializes the message, updating the data variable.    *
        //  *                                                                                                                  *
        //  *     - MsgType.OBJCT: Creates an ObjectMessage object and deserializes the message, updating the data variable.   *
        //  *                                                                                                                  *
        //  *     - MsgType.EVENT: Creates an EventMessage object and deserializes the message, updating the data variable.    *
        //  *                                                                                                                  *
        //  *     - MsgType.TIME: Creates a TimeMessage object and deserializes the message, updating the data variable.       *
        //  *                                                                                                                  *
        //  * Finally, it returns the updated data variable, which holds the deserialized message based on its type.           *
        //  ********************************************************************************************************************

        private static DataMessage ProcessDataMessage(NetIncomingMessage im)
        {
            byte[] bytes = im.ReadBytes(im.LengthBytes);
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            DataMessage data = new ObjectMessage();

            byte msgID = bytes[0];
            MsgType MsgID = (MsgType)msgID;

            switch (MsgID)
            {
                case MsgType.INPUT:
                    InputMessage inputMessage = (InputMessage)MessagePoolManager.InputPool.Get(MsgType.INPUT);
                    data = inputMessage.Deserialize(ref reader);
                    break;
                case MsgType.OBJCT:
                    ObjectMessage objectMessage = (ObjectMessage)MessagePoolManager.ObjectPool.Get(MsgType.OBJCT);
                    data = objectMessage.Deserialize(ref reader);
                    break;
                case MsgType.EVENT:
                    EventMessage collisionMessage = (EventMessage)MessagePoolManager.EventPool.Get(MsgType.EVENT);
                    data = collisionMessage.Deserialize(ref reader);
                    break;
                case MsgType.TIME:
                    TimeMessage timeMessage = new TimeMessage();
                    data = timeMessage.Deserialize(ref reader);
                    break;
            }
            return data;
        }
    }
}
