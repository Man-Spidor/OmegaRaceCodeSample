# Omega Race Code Sample

Omega Race is a traditionally locally played game; however, for a class project, I refactored it to make it a networked game. Provided are a few classes I created to accomplish this. Originally the game was only a simple main game loop that took keyboard inputs and updated the game's physics after processing the inputs. I began refactoring the code by changing how the game processes user inputs to ensure it did so in a data-driven manner. This is when **DataMessage** was created to allow for easily expandable subtypes with common properties necessary for networking purposes. Next, to implement the networking into the game, I created a **NetworkManager**, one for the clients and one for the server. Finally, when I noticed the creation of new DataMessages in every frame of the game was beginning to slow down my code and affect performance drastically, I created the **MessagePool** class and the **MessagePoolManager** class responsible for recycling used DataMessages.

I chose the samples I did because they demonstrate both my fundamental ability to code and my ability to write effective code and adapt to issues as they arise.  

**Omega Race Code Sample Includes:** 
  - 4 Classes, NetworkManager, MessagePoolManager, MessagePool, DataMessage.
  - UML Diagram of the classes.

**NetworkManager:**
The NetworkManager class is responsible for managing network connections and communication. It provides methods for initializing the server, sending and receiving data, and processing incoming network messages.

**MessagePool & MessagePoolMaanger:**
The MessagePool and MessagePoolManager classes provide object-pooling functionality for different types of data messages. The MessagePool class manages a pool of DataMessage objects with a specific message type, allowing efficient reuse and recycling of objects. The MessagePoolManager class initializes and provides access to different message pools for InputMessages, ObjectMessages, and EventMessages. 

**DataMessage:**
The DataMessage class is an abstract base class that is a foundation for different types of messages in the OmegaRace game. It defines common properties and methods for data serialization, deserialization, printing, and execution. 
