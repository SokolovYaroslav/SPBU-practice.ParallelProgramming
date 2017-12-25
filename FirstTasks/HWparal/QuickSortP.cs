using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HWparal
{
    public class QuickSortP
    {
        private static int maxThreadsAmount = 16;

        public static void CompareQuickSorts() {
            Console.WriteLine("Threads: " + maxThreadsAmount.ToString());
            var sw = new Stopwatch();
            
            var array = CreateRandom2DIntArray(5, 10000000, 42);
            
            sw.Start();
            for (int i = 0; i < array.Length; i++) {
                MyBestQuickSort(array[i], 0, array[i].Length - 1);
            }
            sw.Stop();

            foreach (var row in array) {
                if (!IsSortedArray(row)) {
                    Console.WriteLine("Array from my quicksort is not sorted!");
                    break;
                }
            }
            
            Console.WriteLine("Time in MyBestQuickSort is "
                              + sw.ElapsedMilliseconds / (double) (5 * 1000) + " sec");
            
            sw.Reset();
            array = CreateRandom2DIntArray(5, 10000000, 42);
            
            sw.Start();
            for (int i = 0; i < array.Length; i++) {
                TheBestQuickSort(array[i], 0, array[i].Length - 1);
            }
            sw.Stop();

            foreach (var row in array) {
                if (!IsSortedArray(row)) {
                    Console.WriteLine("Array from the best quicksort is not sorted!");
                    break;
                }
            }
            
            Console.WriteLine("Time in TheBestQuickSort is "
                              + sw.ElapsedMilliseconds / (double) (5 * 1000) + " sec");
        }
        
        public static long QuickSort<T>(T[] items) where T : IComparable<T> {
            var sw = new Stopwatch();
            sw.Start();
            MyBestQuickSort(items, 0, items.Length - 1);
            sw.Stop();
            Console.WriteLine("QuickSort: " + maxThreadsAmount + " threads, " +
                              sw.ElapsedMilliseconds / (double) 1000 + " sec");
            return sw.ElapsedMilliseconds;
        }
        
        private static void MyBestQuickSort<T>(T[] items, int start, int end,
            bool parallel = true, int depth = 0) where T : IComparable<T> {
            
            int left = start;
            int right = end;
            var pivotal = items[(right + left) / 2];

            while(left <= right)
            {
                while(items[left].CompareTo(pivotal) < 0)
                {
                    left++;
                }
                while(items[right].CompareTo(pivotal) > 0)
                {
                    right--;
                }
                if(left <= right)
                {
                    Swap(ref items[left++],ref items[right--]);
                }
            }

            if (parallel && depth < Math.Log(maxThreadsAmount, 2)) {
                Thread thread1 = new Thread(() => 
                    { MyBestQuickSort(items, start, right, depth:++depth); });
                Thread thread2 = new Thread(() => 
                    { MyBestQuickSort(items, left, end, depth:++depth); });

                if (right > start) {
                    thread1.Start();
                }
                if (left < end) {
                    thread2.Start();
                }
                
                if (right > start) {
                    thread1.Join();    
                }
                if (left < end) {
                    thread2.Join();   
                }
            }
            else {
                if (right > start) {
                    MyBestQuickSort(items, start, right, false);
                }
                if (left < end) {
                    MyBestQuickSort(items, left, end, false);
                }
            }
        }

        private static int DoPartition<T>(T[] items, int left, int right) where T : IComparable<T>
        {
            int pivotPos = (left + right) / 2;
            T pivotValue = items[pivotPos];
            Swap(ref items[right - 1], ref items[pivotPos]);
            int store = left;
            for (int i = left; i < right - 1; ++i) {
                if (items[i].CompareTo(pivotValue) < 0) {
                    Swap(ref items[i], ref items[store]);
                    ++store;
                }
            }
            Swap(ref items[right - 1], ref items[store]);
            return store;
        }
        
        private static void TheBestQuickSort<T>(T[] items, int left, int right) 
            where T : IComparable<T>
        {
            if (left == right)
                return;
            int pivot = DoPartition(items, left, right);
            
            Parallel.Invoke(
                () => TheBestQuickSort(items, left, pivot),
                () => TheBestQuickSort(items, pivot + 1, right)
            );

        }
        
        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        public static bool IsSortedArray<T>(T[] array) where T : IComparable<T> {
            for (int i = 1; i < array.Length; i++) {
                if (array[i - 1].CompareTo(array[i]) > 0) {
                    return false;
                }
            }
            return true;
        }

        public static int[][] CreateRandom2DIntArray(int amount, int lenght, int seed) {
            Random random = new Random(Seed:seed);
            int[][] array;
            array = new int[amount][];
            for (int i = 0; i < array.Length; i++) {
                array[i] = new int[lenght];
            }
            for (int i = 0; i < array[0].Length; i++) {
                array[0][i] = random.Next();
                for (int j = 1; j < array.Length; j++) {
                    array[j][i] = array[0][i];
                }
            }
            return array;
        }
    }
}