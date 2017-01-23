using System;
using System.IO;
using System.IO.Pipes;

namespace DepthTracker.Connection
{
    public class PipeClient : IDisposable
    {
        private NamedPipeClientStream _client;
        private StreamWriter _writer;
        StreamReader _reader;

        public void StartPipeClient()
        {
            _client = new NamedPipeClientStream("DephTrackerPipe");
            _client.Connect();
            _reader = new StreamReader(_client);
            _writer = new StreamWriter(_client);
        }

        public void SendJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;
            _writer.WriteLine(json);
            _writer.Flush();
            Console.WriteLine(_reader.ReadLine());
        }

        public void Dispose()
        {
            if(_client != null)
            {
                _client.Dispose();
                _client = null;
            }
            if(_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
            if(_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }
    }
}
