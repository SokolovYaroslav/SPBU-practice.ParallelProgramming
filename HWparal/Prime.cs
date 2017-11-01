using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HWparal
{
    public static class Prime
    {
        private static int maxThreadsAmount = 4;

        public static bool CheckResult() {
            List<int> truePrimes = PrimesInRangeThread(0, 10000);
            List<int> threadPrimes = PrimesInRangeThread(0, 10000);
            List<int> taskPrimes = PrimesInRangeTask(0, 10000);
            List<int> threadPoolPrimes = PrimesInRangeThreadPool(0, 10000);
            List<int> bestPrimes = PrimesInRangeBest(0, 10000);

            if (truePrimes.Count != threadPrimes.Count) {
                Console.WriteLine("Thread primes gives wrong answer!");
                return false;
            }
            if (truePrimes.Count != taskPrimes.Count) {
                Console.WriteLine("Task primes gives wrong answer!");
                return false;
            }
            if (truePrimes.Count != threadPoolPrimes.Count) {
                Console.WriteLine("Thread pool primes gives wrong answer!");
                return false;
            }
            if (truePrimes.Count != bestPrimes.Count) {
                Console.WriteLine("Best primes gives wrong answer!");
                return false;
            }
            
            for (int i = 0; i < truePrimes.Count; i++) {
                if (truePrimes[i] != threadPrimes[i]) {
                    Console.WriteLine("Thread primes gives wrong answer!");
                    return false;
                }
                if (truePrimes[i] != taskPrimes[i]) {
                    Console.WriteLine("Task primes gives wrong answer!");
                    return false;
                }
                if (truePrimes[i] != threadPoolPrimes[i]) {
                    Console.WriteLine("Thread pool primes gives wrong answer!");
                    return false;
                }
                if (truePrimes[i] != bestPrimes[i]) {
                    Console.WriteLine("Best primes gives wrong answer!");
                    return false;
                }
            }
            return true;
        }
        
        public static void ComparePrimes(int start, int end) {
            Console.WriteLine("Threads: " + maxThreadsAmount.ToString());
            if (!CheckResult()) {
                return;
            }
            
            var sw = new Stopwatch();
            
            sw.Start();
            for (int i = 0; i < 15; i++) {
                PrimesInRange(start, end);   
            }
            sw.Stop();
            Console.WriteLine("Synchronous: " + sw.ElapsedMilliseconds / (double) (1000 * 15));
            sw.Reset();
            
            sw.Start();
            for (int i = 0; i < 15; i++) {
                PrimesInRangeThread(start, end);   
            }
            sw.Stop();
            Console.WriteLine("Threads: " + sw.ElapsedMilliseconds / (double) (1000 * 15));
            sw.Reset();
            
            sw.Start();
            for (int i = 0; i < 15; i++) {
                PrimesInRangeTask(start, end);   
            }
            sw.Stop();
            Console.WriteLine("Tasks: " + sw.ElapsedMilliseconds / (double) (1000 * 15));
            sw.Reset();
            
            sw.Start();
            for (int i = 0; i < 15; i++) {
                PrimesInRangeThreadPool(start, end);   
            }
            sw.Stop();
            Console.WriteLine("Thread pool: " + sw.ElapsedMilliseconds / (double) (1000 * 15));
            sw.Reset();
            
            sw.Start();
            for (int i = 0; i < 15; i++) {
                PrimesInRangeBest(start, end);   
            }
            sw.Stop();
            Console.WriteLine("Best: " + sw.ElapsedMilliseconds / (double) (1000 * 15));
        }
        
        public static List<int> PrimesInRangeThread(int start, int end) {
            int[] splits = GetSplitting(start, end);
            
            List<Thread> threads = new List<Thread>();
            List<int>[] results = new List<int>[maxThreadsAmount];

            for (int i = 0; i < maxThreadsAmount; ++i) {
                int index = i;
                int startNumber = splits[i];
                int endNumber = splits[i + 1];
                if (startNumber >= endNumber) {
                    break;
                }
                threads.Add(new Thread(() => 
                    results[index] = PrimesInRange(startNumber, endNumber)));
            }

            foreach (var thread in threads) {
                thread.Start();
            }
            foreach (var thread in threads) {
                thread.Join();
            }

            var res = new List<int>();
            Array.ForEach(results, primes => { res.AddRange(primes); });
            
            return results.SelectMany(primes => primes).ToList();
        }

        public static List<int> PrimesInRangeTask(int start, int end) {
            int[] splits = GetSplitting(start, end);
            
            List<Task<List<int>>> tasks = new List<Task<List<int>>>();

            for (int i = 0; i < maxThreadsAmount; ++i) {
                int startNumber = splits[i];
                int endNumber = splits[i + 1];
                if (startNumber >= endNumber) {
                    break;
                }
                
                tasks.Add(Task.Run(() => 
                    PrimesInRange(startNumber, endNumber)));
            }

            try {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException e) {
                Console.WriteLine(e);
                throw;
            }
            
            return tasks.SelectMany(task => task.Result).ToList();
        }

        public static List<int> PrimesInRangeThreadPool(int start, int end) {
            int[] splits = GetSplitting(start, end);
            List<int>[] results = new List<int>[maxThreadsAmount];
            
            int completed = 0; 
            int toComplete = maxThreadsAmount;

            ManualResetEvent allDone = new ManualResetEvent(initialState: false);
            
            for (int i = 0; i < maxThreadsAmount; i++) {
                int index = i;
                int startNumber = splits[i];
                int endNumber = splits[i + 1];
                if (startNumber >= endNumber) {
                    break;
                }
                
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    results[index] = PrimesInRange(startNumber, endNumber);
                    if (Interlocked.Increment(ref completed) == toComplete) {
                        allDone.Set();
                    }
                });
            }
            
            allDone.WaitOne();

            return results.SelectMany(primes => primes).ToList();
        }

        public static List<int> PrimesInRangeBest(int start, int end) {
            List<int> primes =
                (from n in Enumerable.Range(start, end).AsParallel()
                    where IsPrime(n) select n).ToList();
            primes.Sort();
            return primes;
        }

        private static int[] GetSplitting(int start, int end) {
            /*TODO implement smarter splitting*/
            int[] splits = new int[maxThreadsAmount + 1];
            int step = (end - start) / maxThreadsAmount + 1;
            int currentBorder = start;
            
            splits[0] = start;
            for (int i = 0; i < maxThreadsAmount; i++) {
                if (currentBorder + step < end) {
                    splits[i + 1] = currentBorder + step;
                }
                else {
                    splits[i + 1] = end;
                }
                currentBorder += step;
            }
            
            return splits;
        }

        private static List<int> PrimesInRange(int start, int end) {
            List<int> primes = new List<int>();
            
            for (int number = start; number < end; number++) {
                if (IsPrime(number)) {
                    primes.Add(number);
                }
            }

            return primes;
        }

        public static bool IsPrime(int number) {
            if (number < 2) {
                return false;
            }
            if (number == 2) {
                return true;
            }
            if (number % 2 == 0) {
                return false;
            }
            for (int i = 3; i * i <= number; i += 2) {
                if (number % i == 0) {
                    return false;
                }
            }
            return true;
        }
    }
}