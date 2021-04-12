using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AwaitAsyncLibrary
{
    /// <summary>
    /// await/async关键字
    /// 任何一个方法都可以增加async
    /// await放在task前面
    /// 一般成对出现 只有async是没有意义的，有警告
    ///              只有await是报错的，编译直接报错
    /// await/async要么不用，要么用到底             
    /// </summary>
    public class AwaitAsyncClass
    {
        public static void TestShow()
        {
            Test();
        }

        private async static Task Test()
        {
            Console.WriteLine($"当前主线程-Task-》start ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            {
                //NoReturnNoAwait();
            }
            {
                //NoReturn();
                //for (int i = 0; i < 10; i++)
                //{
                //    Thread.Sleep(300);
                //    Console.WriteLine($"当前主线程-Task ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")} i={i}");
                //}
            }
            {
                //Task t = NoReturnTask();
                //Console.WriteLine($"当前主线程-Task ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                //t.Wait();//主线程等待Task的完成，阻塞的
                //await t;//主线程到这里就返回了，await后的代码会由线程池的线程执行。非阻塞的
            }
            {
                {
                    //Task<long> t = SumAsync();
                    //Console.WriteLine($"当前主线程-Task ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    //long iResult = t.Result;//访问result  主线程等待Task的完成
                    //t.Wait();//等价于上一行
                }
                {
                    Task<long> t = SumAsync2();
                    Console.WriteLine($"当前主线程-Task ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    long iResult = t.Result;//访问result  主线程等待Task的完成
                    t.Wait();//等价于上一行
                }
            }
            Console.WriteLine($"当前主线程-Task-》end ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            Console.Read();
        }

        /// <summary>
        /// 只有async没有await，会有个warn警告
        /// 跟普通方法没有区别
        /// </summary>
        private static async void NoReturnNoAwait()
        {
            //主线程执行
            Console.WriteLine($"NoReturnNoAwait Sleep before Task，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            
            Task task = Task.Run(() =>//启动新线程完成任务
            {
                Console.WriteLine($"NoReturnNoAwait Sleep before Task，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                Thread.Sleep(3000);
                Console.WriteLine($"NoReturnNoAwait Sleep after Task，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            });

            //主线程执行
            Console.WriteLine($"NoReturnNoAwait Sleep after Task，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
        }

        /// <summary>
        /// async/await
        /// 不能单独await，await只能放在task前面
        /// 不推荐void返回值，使用Task来代替
        /// Task和Task<T>能够使用await，Task.When
        /// </summary>
        private static async void NoReturn()
        {
            //主线程执行
            Console.WriteLine($"NoReturn Sleep before await，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            
            TaskFactory taskFactory = new TaskFactory();
            Task task = taskFactory.StartNew(() =>//启动新线程完成任务
            {
                Console.WriteLine($"NoReturn Sleep before，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                Thread.Sleep(3000);
                Console.WriteLine($"NoReturn Sleep after，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            });

            {
                //{//1回调方式
                //    task.ContinueWith(t =>
                //    {
                //        Console.WriteLine($"NoReturn Sleep after await，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                //    });
                //}

                {//2其实就像把await后面的代码包装成一个ContinueWith的回调动作，和上面的1相同效果。
                    await task;//主线程到这里就返回了，执行主线程任务。（谁调用我就返回去干自己的活）
                    //这个回调的线程是不确定的：可能是主线程，也可能是子线程 也可能是其它线程
                    Console.WriteLine($"NoReturn Sleep after await，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                }
                //说明：1和2其实效果相同
            }
        }

        /// <summary>
        /// 无返回值 async Task == async void
        /// Task和Task<T>能够使用await,Task.WhenAny,Task.WhenAll等方式组合使用。Async void不行
        /// </summary>
        /// <returns></returns>
        private static async Task NoReturnTask()
        {
            //这里还是主线程执行
            Console.WriteLine($"NoReturnTask Sleep before await，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            Task task = Task.Run(() =>
            {
                Console.WriteLine($"NoReturnTask Sleep before，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                Thread.Sleep(3000);
                Console.WriteLine($"NoReturnTask Sleep after，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            });
            await task;
            Console.WriteLine($"NoReturnTask Sleep after await，ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");

            //return new TaskFactory().StartNew(() => { });//不能return，没有async才行
        }

        /// <summary>
        /// 带返回值的Task
        /// 要使用返回值就一定要等子线程计算完毕
        /// </summary>
        /// <returns>async 就只返回long</returns>
        private static async Task<long> SumAsync()
        {
            long result = 0;

            Console.WriteLine($"SumAsync 111 start ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            await Task.Run(() =>
            {
                for (int k = 0; k < 10; k++)
                {
                    Console.WriteLine($"SumAsync {k} await Task.Run ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    Thread.Sleep(1000);
                }

                for (int i = 0; i < 999999999; i++)
                {
                    result += i;
                }
            });

            Console.WriteLine($"SumAsync 111 end ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            return result;
        }

        /// <summary>
        /// 带返回值的Task--加强版（可以控制线程的执行顺序）
        /// 要使用返回值就一定要等子线程计算完毕
        /// </summary>
        /// <returns>async 就只返回long</returns>
        private static async Task<long> SumAsync2()
        {
            long result = 0;

            Console.WriteLine($"SumAsync 111 start ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            await Task.Run(() =>
            {
                for (int k = 0; k < 10; k++)
                {
                    Console.WriteLine($"SumAsync {k} await Task.Run ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    Thread.Sleep(1000);
                }

                for (int i = 0; i < 999999999; i++)
                {
                    result += i;
                }
            });

            Console.WriteLine($"SumAsync 111 end ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            await Task.Run(() =>
            {
                for (int k = 0; k < 10; k++)
                {
                    Console.WriteLine($"SumAsync {k} await Task.Run ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    Thread.Sleep(1000);
                }

                for (int i = 0; i < 999999999; i++)
                {
                    result += i;
                }
            });

            Console.WriteLine($"SumAsync 111 end ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            await Task.Run(() =>
            {
                for (int k = 0; k < 10; k++)
                {
                    Console.WriteLine($"SumAsync {k} await Task.Run ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    Thread.Sleep(1000);
                }

                for (int i = 0; i < 999999999; i++)
                {
                    result += i;
                }
            });

            Console.WriteLine($"SumAsync 111 end ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            return result;//会是SumAsync方法值的三部
        }

        /// <summary>
        /// 真的返回Task 不是async
        /// 要使用返回值就一定要等子线程计算完毕
        /// </summary>
        /// <returns>没有async Task</returns>
        private static Task<int> SumFactory()
        {
            Console.WriteLine($"SumFactory 111 start ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            TaskFactory taskFactory = new TaskFactory();
            Task<int> iResult = taskFactory.StartNew<int>(() =>
            {
                Thread.Sleep(3000);
                Console.WriteLine($"SumFactory 123 Task.Run ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                return 123;
            });
            Console.WriteLine($"SumFactory 111 end ManagedThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            return iResult;
        }
    }
}
