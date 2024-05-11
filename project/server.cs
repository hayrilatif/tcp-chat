using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

//server

List<Tuple<string, string, string>> entries = new List<Tuple<string, string, string>>();
Dictionary<string, Socket> clientDict = new Dictionary<string, Socket>();

string hostName = Dns.GetHostName();
IPHostEntry entry = Dns.GetHostEntry(hostName);

for (int i = 0; i < entry.AddressList.Length; i ++)
{
    Console.WriteLine($"[{i}]-{entry.AddressList[i].ToString()}");
}
Console.WriteLine("Write the selected index of the address from the list.");
IPAddress address = IPAddress.Parse(entry.AddressList[Int32.Parse(Console.ReadLine())].ToString());
Console.WriteLine($"Selected address: {address.ToString()}");

Console.Write("Port: ");
int port = Int32.Parse(Console.ReadLine());

IPEndPoint endPoint = new IPEndPoint(address, port);

Console.WriteLine($"Listening for new clients on {endPoint}");
Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
socket.Bind(endPoint);
socket.Listen();

Thread threadClientAcceptor = new Thread(new ThreadStart(ClientAcceptor));
threadClientAcceptor.Start();

Thread threadWriteLoop= new Thread(new ThreadStart(WriteLoop));
threadWriteLoop.Start();

void ClientAcceptor()
{
    Console.WriteLine("ClientAcceptor started!");
    while (true)
    {
        Socket client = socket.Accept();
        string? clientName = ReadMessage(client);
        if (clientName == null) continue;
        
        Thread threadReadLoop = new Thread(new ParameterizedThreadStart(ReadLoop));
        threadReadLoop.Start(new Tuple<string, Socket>(clientName, client));

        Console.WriteLine($"New client accepted: {clientName}");
    }
}

void ReadLoop(Object TupleO)
{
    Tuple<string,Socket>tuple = (Tuple<string, Socket>)TupleO;
    string clientName = tuple.Item1;
    Socket client = tuple.Item2;
    clientDict[clientName] = client;

    while (true)
    {
        string? toWhom = ReadMessage(client);
        if (toWhom == null) continue;

        string? message = ReadMessage(client);
        if (message == null) continue;
        
        lock (entries)
        { 
            entries.Add(new Tuple<string, string, string>(clientName, toWhom, message));
        }
        Console.WriteLine($"New entry '{message}' from client-'{clientName}' to client-'{toWhom}'");
    }
}

void WriteLoop()
{
    Console.WriteLine("salfj");
    while (true)
    {
        lock (entries)
        {
            if (entries.Count == 0)
            {
                continue;
            }

            string fromClient = entries[0].Item1;
            string toClient = entries[0].Item2;
            string message = entries[0].Item3;
            entries.RemoveAt(0);

            Socket toSocket = clientDict[toClient];
            SendMessage(toSocket, fromClient);
            SendMessage(toSocket, message);

            Console.WriteLine($"'{message}' forwarded from client-'{fromClient}' to client-'{toClient}'");
        }
    }
}

void SendMessage(Socket socket, string message)
{
    byte[] buffer = Encoding.UTF8.GetBytes(message);
    socket.Send(buffer);
}

string? ReadMessage(Socket socket)
{
    try
    {
        byte[] buffer = new byte[1024];
        int s = socket.Receive(buffer);
        string message = Encoding.UTF8.GetString(buffer, 0, s);
        return message;
    }
    catch
    {
        return null;
    }
}