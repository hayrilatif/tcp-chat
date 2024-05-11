using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

//client


Console.Write("IP: ");
IPAddress address = IPAddress.Parse(Console.ReadLine());

Console.Write("Port: ");
int port = Int32.Parse(Console.ReadLine());
IPEndPoint endPoint = new IPEndPoint(address, port);

Console.WriteLine($"Connecting to {endPoint}");
Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
socket.Connect(endPoint);

Console.Write("Name: ");
string clientName = Console.ReadLine();

SendMessage(socket, clientName);

void MessagingLoop()
{
    Console.WriteLine("Press 'n' for send message.");
    while (true)
    {
        if (Console.ReadKey().Key != ConsoleKey.N) continue;

        Console.Write("To Whom: ");
        string? toWhom = Console.ReadLine();

        Console.Write("Message: ");
        string? message = Console.ReadLine();

        SendMessage(socket, toWhom);
        SendMessage(socket, message);
    }
}
Thread threadMessagingLoop = new Thread(new ThreadStart(MessagingLoop));
threadMessagingLoop.Start();

void ReadLoop(Object threadMessagingLoopO)
{
    Thread thread = (Thread)threadMessagingLoopO;

    while (true)
    {
        string? fromWho = ReadMessage(socket);
        if (fromWho == null) continue;

        string? message = ReadMessage(socket);
        if (message == null) continue;

        Console.Write($"New message '{message}' from client-'{fromWho}'");
        Console.Write(Environment.NewLine);
    }
}
Thread threadReadLoop = new Thread(new ParameterizedThreadStart(ReadLoop));
threadReadLoop.Start();


void SendMessage(Socket socket, string message)
{
    byte[] buffer = Encoding.UTF8.GetBytes(message);
    socket.Send(buffer);
}
string ReadMessage(Socket socket)
{
    byte[] buffer = new byte[1024];
    int s = socket.Receive(buffer);
    string message = Encoding.UTF8.GetString(buffer, 0, s);
    return message;
}