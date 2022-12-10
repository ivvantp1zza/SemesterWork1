// See https://aka.ms/new-console-template for more information
using SemWork1;

var server = new HttpServer();
server.Start();
while (true)
{
    ReadCommand(Console.ReadLine(), server);
}

static void ReadCommand(string command, HttpServer server)
{
    switch (command)
    {
        case "start":
            server.Start();
            break;
        case "stop":
            server.Stop();
            break;
        case "restart":
            server.Stop();
            server.Start();
            break;
        default:
            Console.WriteLine("Unknown command!");
            break;
    }
}