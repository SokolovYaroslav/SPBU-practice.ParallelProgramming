using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HWparal
{
    internal class Program
    {
        public static void Main(string[] args) {
            Console.WriteLine("Quicksort test:");
            QuickSortP.CompareQuickSorts();
            Console.WriteLine("Primes test:");
            Prime.ComparePrimes(1000000, 10000000);
            Console.WriteLine("Hash test:");
            Console.WriteLine(FolderHash.FolderGetHash("./"));
        }
    }
}