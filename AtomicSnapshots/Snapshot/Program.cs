using System;
using System.Threading.Tasks;

namespace Snapshot
{
    
    internal class Program
    {
        public const int RegistersAmount = 2;
        private static readonly Random Rand = new Random();
        private static readonly Logger Logger = new Logger("/home/yas/RiderProjects/Snapshot/Log.txt");
        private static readonly SingleWriterRegisters Cells = new SingleWriterRegisters(RegistersAmount, Logger);
        
        public static void Main(string[] args)
        {
            var tasks = new Task[RegistersAmount];

            for (var i = 0; i < RegistersAmount; i++) {
                var id = i;
                tasks[i] = Task.Run((() => ThreadLogic(id)));
            }

            Task.WaitAll(tasks);
            Logger.StopLog();
        }

        private static void ThreadLogic(int id) {
            for (var i = 0; i < Rand.Next() % 1000; i++) {
                Cells.Update(id, Rand.Next());
            }
        }
    }
}