using System;
using System.IO;
using System.IO.Pipes;

namespace MappingTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new NamedPipeClientStream("DephTrackerPipe");
            client.Connect();
            StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);

            while (true)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                writer.WriteLine(input);
                writer.Flush();
            }
        }
    }
}