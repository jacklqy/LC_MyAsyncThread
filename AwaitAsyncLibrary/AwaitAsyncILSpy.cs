using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AwaitAsyncLibrary
{
    public class AwaitAsyncILSpy
    {
        /// <summary>
        /// 执行顺序：aaa->bbb->ccc->ddd->eee
        /// </summary>
        public static void Show()
        {
            Console.WriteLine($"aaa {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            Async();
            Console.WriteLine($"ccc {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            Console.Read();
        }

        public static async void Async()
        {
            Console.WriteLine($"bbb {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            await Task.Run(() =>
            {
                Thread.Sleep(500);
                Console.WriteLine($"ddd {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            });
            Console.WriteLine($"eee {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
        }
    }
}
