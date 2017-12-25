using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinaryTree
{
    internal class Program
    {
        public static void Main(string[] args) {
            var tree = new BinarySearchTreeConcurrent<int, int?>();
            var queue = new Queue<Task>();

            var rand = new Random(788);
            for (int i = 0; i < 1000; i++) {
                int next;
                switch (rand.Next(3)) {
                    case 0:
                        next = rand.Next(100);
                        queue.Enqueue(new Task(() => {
                            tree.Insert(next, next);
                            Console.WriteLine("Insert" + next);
                        }));
                        break;
                    case 1:
                        next = rand.Next(100);
                        queue.Enqueue(new Task(() => {
                            tree.Delete(next);
                            Console.WriteLine("Delete" + next);
                        }));
                        break;
                    case 2:
                        next = rand.Next(100);
                        queue.Enqueue(new Task(() => {
                            var actual = tree.Search(next);
                            if (actual != null && next != actual) {
                                Console.WriteLine("Something went wrong!");
                            }
                            Console.WriteLine("Search" + next);
                        }));
                        break;       
                }
            }
            foreach (var task in queue) {
                task.Start();
            }
            Task.WaitAll(queue.ToArray());
        }
    }
}