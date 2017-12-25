using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Snapshot
{
    public class Logger
    {
        private readonly ConcurrentDictionary<int, string> _messagesQueue;
        private readonly string _fileName;

        public Logger(string fileName) {
            _fileName = fileName;
            _messagesQueue = new ConcurrentDictionary<int, string>();
        }

        public void WriteToLog(int index, string message) {
            if (!_messagesQueue.TryAdd(index, message)) {
                throw new ArgumentException("Number of operation collision");
            }
        }

        public void StopLog() {
            using (var writer = File.CreateText(_fileName)) {
                foreach (var pair in _messagesQueue) {
                    writer.WriteLine($"{pair.Key} {pair.Value}");
                }
            }
        }
    }
}