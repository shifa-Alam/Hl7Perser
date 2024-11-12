using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Efferent.HL7.V2;


namespace Hl7Perser
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the listener in a separate thread to receive HL7 messages
            Thread listenerThread = new Thread(StartListener);  
            listenerThread.Start();

            // Simulate sending an HL7 message after a delay
            Thread.Sleep(5000);
            SendHL7Message("127.0.0.1", 2575);
        }

        static void StartListener()
        {
            TcpListener server = null;
            try
            {
                // Set the TCP listener on port 2575.
                Int32 port = 2575;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // Create TCP/IP socket and start listening for incoming connections.
                server = new TcpListener(localAddr, port);
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        var message = new Message(data);
                        if (message.ParseMessage())
                        {
                            Console.WriteLine("HL7 Message Parsed Successfully.");
                        }

                        // Send a response back to the client.
                        byte[] msg = Encoding.ASCII.GetBytes("Message Received");
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: Message Received");
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        static void SendHL7Message(string ipAddress, int port)
        {
            try
            {
                TcpClient client = new TcpClient(ipAddress, port);

                // HL7 message to send
                string hl7Message = "MSH|^~\\&|LAB|1234|5678|9012|202103051200||ORU^R01|1|P|2.3\rPID|||123456||DOE^JOHN";

                // Convert the message to bytes
                Byte[] data = Encoding.ASCII.GetBytes(hl7Message);

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", hl7Message);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}
