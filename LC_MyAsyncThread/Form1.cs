using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// 任何一个多线程在启动的时候都离不开委托的。
/// </summary>
namespace LC_MyAsyncThread
{
    /// <summary>
    /// 1、进程-线程-多线程，同步和异步；
    /// 2、委托启动异步调用；
    /// 3、多线程特点：不卡主线程，速度快，无序性；
    /// 4、异步的回调和状态参数；
    /// 5、异步等待三种方式；
    /// 6、异步返回值；
    /// 
    /// 进程：计算机概念，程序在服务器运行时占据全部计算资源总和
    /// 线程：计算机概念，进程在响应操作时最小单位，也包括CPU 内存 网络 硬盘IO
    /// 一个进程会包含多个线程；线程是隶属于某个进程，进程销毁线程也就没有了
    /// 句柄：其实是个long数字，是操作系统标识应用程序
    /// 多线程：计算机概念，一个进程有多个线程在同时运行
    /// 
    /// C#里面的多线程：
    /// Thread类是C#语言对线程对象的一个封装
    /// 
    /// 为什么可以多线程呢？
    /// 1 多个CPU的核可以并行工作(4核8线程，这里的线程指的是模拟核)
    /// 2 ***CPU分片***，1s的处理能力分成1000份，***操作系统调度***去响应不同的任务，从宏观角度来说，感觉就是多个任务在并发执行，但是从微观角度来说，一个物理cpu同一时刻只能为一个任务服务
    /// 
    /// 并行：多核之间叫并行
    /// 并发：CPU分片的并发
    /// 
    /// 同步异步：
    ///     同步方法：发起调用，等待完成后才继续下一行，非常符号开发思维，有序执行；
    ///     异步方法：发起调用，不等待完成，直接进入下一行，启动一个新线程来完成方法计算；
    /// 
    /// 启动多少个线程才算合理上线，怎么判断？大概的参考值：CPU核数*4
    /// 
    /// 1 Thread:线程等待，回调，前台线程/后台线程
    /// 2 ThreadPool：线程池使用，设置线程池，ManualResetEvent
    /// 3 扩展封装Thread&ThreadPool回调/等待
    /// 4 Task(多线程的最佳实践)：Waitall WaitAny Delay
    /// 5 TaskFactory：ContinueWhenAny ContinueWhenAll
    /// 6 并行运算Parallel.Invoke/For/Foreach
    /// 
    /// 
    /// 1 多异常处理和线程取消
    /// 2 多线程的临时变量
    /// 3 线程安全和锁lock
    /// 4 async/await
    /// 
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Sync
        /// <summary>
        /// 同步方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSync_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnSync_Click Start" +
                $"{Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
                $"************************");
            int l = 3;
            int m = 4;
            int n = l + m;
            for (int i = 0; i < 5; i++)
            {
                string name = string.Format($"btnSync_Click_{i}");
                this.DoSomethingLong(name);
            }
            Console.WriteLine($"********************btnSync_Click End" +
                $"{Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
                $"************************");
        }
        #endregion

        #region Async
        /// <summary>
        /// 异步方法
        /// 1 同步方法卡界面：主线程(UI线程)忙于计算，无暇他顾。
        ///   异步多线程方法不卡界面：主线程闲置，计算任务交给子线程完成。
        /// 改善用户体验，winform点击个按钮不至于卡死；
        /// web开发，比如发短信/邮件，异步多线程去发；
        /// 
        /// 2 同步方法慢，只有一个线程计算
        ///   异步多线程快，因为有5个线程并发计算
        ///   12658ms    3636ms  不到4倍   CPU密集型计算（资源受限）
        ///   10126ms    2075ms  差不多5倍 Sleep（资源够用）
        ///   ***多线程其实是资源换性能*** 1 资源不是无限的  2资源调度损耗
        ///   
        ///   一个订单表统计查询很耗时，能不能多线程优化下性能？不能！这就是一个操作，没法并行；
        ///   查询数据库/调用接口/读取硬盘文件/做数据计算，能不能多线程优化下性能？可以，多个任务可以并行；
        ///   线程不是越多越好，因为资源有限，而且调度有损耗；
        /// 
        /// 3 同步方法有序进行，异步多线程无序进行
        ///   启动无序：线程资源是向操作系统申请的，由操作系统的调度策略决定，所以启动顺序随机
        ///   同一个任务同一个线程，执行时间也不确定，CPU分片
        ///   以上相加，得出的结果是 结束也无序
        ///   使用多线程请一定小心，很多事不是想当然的，尤其是多线程操作间有顺序要求的时候
        ///   能不能通过延迟一点启动来控制顺序？或者预计下结束顺序？这些都不靠谱！
        ///   
        /// 异步技术和多线程技术是同一个东西吗？不是(比如通过硬件加速实现的异步和通过线程实现的异步就不一样)。***但是在.net里面都是基于线程来完成的异步，.net里面没有区分，更多的体现出来的就是线程***
        /// 异步是一个效果，多线程可以实现异步
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAsync_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnAsync_Click Start" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");

            Action<string> action = this.DoSomethingLong;
            //action.Invoke("btnAsync_Click");
            //action("btnAsync_Click");
            //action.BeginInvoke("btnAsync_Click", null, null);//委托自身需要参数

            for (int i = 0; i < 5; i++)
            {
                string name = string.Format($"btnAsync_Click_{i}");
                action.BeginInvoke(name, null, null);//委托自身需要参数
            }

            Console.WriteLine($"********************btnAsync_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        } 
        #endregion

        #region AsyncAdvanced 委托异步调用
        private void btnAsyncAdvanced_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnAsyncAdvanced_Click Start" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");

            //Action<string> action = this.DoSomethingLong;

            ////1 回调：将后续动作通过回调参数传递进去，子线程完成计算后，去调用这个回调委托。
            //IAsyncResult asyncResult = null;//异步调用操作描述
            //AsyncCallback asyncCallback = ar =>
            //{
            //    Console.WriteLine($"{object.ReferenceEquals(ar, asyncResult)}");
            //    Console.WriteLine($"btnAsyncAdvanced_Click计算成功啦...{ar.AsyncState} {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            //};
            //asyncResult = action.BeginInvoke("btnAsyncAdvanced_Click", asyncCallback, "jack");//第一个参数为调用委托方法参数，第二参数为回调函数，第三个参数为回调传递参数

            ////2 通过IsCompleted等待，卡界面--主线程在等待，边等待边提示
            //int i = 0;
            //while (!asyncResult.IsCompleted)
            //{
            //    //Thread.Sleep(200);
            //    if (i < 9)
            //    {
            //        Console.WriteLine($"完成进度{++i * 10}%");
            //    }
            //    else
            //    {
            //        Console.WriteLine("完成99.99%");
            //    }
            //    Thread.Sleep(200);//sleep放的位置也是有技巧的
            //}
            //Console.WriteLine("计算完成啦。。。");

            ////3 WaitOne等待，一直等待/限时等待，卡界面--主线程在等待
            //asyncResult.AsyncWaitHandle.WaitOne();//一直等待任务完成
            //asyncResult.AsyncWaitHandle.WaitOne(-1);//一直等待任务完成
            //asyncResult.AsyncWaitHandle.WaitOne(1000);//最多等待1s，超时就不等了
            //Console.WriteLine("计算完成啦。。。");

            ////4 EndInvoke 一直等待
            //action.EndInvoke(asyncResult);//等待某次异步调用操作结束
            //Console.WriteLine("计算完成啦。。。");

            //5 EndInvoke一直等待，而且可以获取委托的返回值 
            Func<int> func = () =>
            {
                Thread.Sleep(2000);
                Console.WriteLine($"调用委托方法啦...");
                return DateTime.Now.Hour;
            };
            //int iResult = func.Invoke();
            IAsyncResult asyncResult =  func.BeginInvoke(ar =>
            {
                //int iEndResult = func.EndInvoke(asyncResult);
                Console.WriteLine($"回调结束啦...{ar.AsyncState} {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            },"jack");
            //一个异步操作只能调用EndInvoke一次
            int iEndResult = func.EndInvoke(asyncResult);
            Console.WriteLine($"异步调用返回值：{iEndResult}");

            Console.WriteLine($"********************btnAsyncAdvanced_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        #endregion

        #region Thread
        /// <summary>
        /// Thread .NetFramework1.0
        /// Thread：C#对线程对象的一个封装
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnThread_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnThread_Click Start" +
             $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
             $"************************");

            {
                //ThreadStart method = () =>
                //{
                //    this.DoSomethingLong("btnThread_Click");
                //};
                //Thread thread = new Thread(method);
                //thread.Start();//开启线程，执行委托

                //能不能把线程玩的很花哨？比如开启十个线程，一会让这个线程停止，一会让另一个线程恢复，一会让另一个线程终止等等这些操作？
                //不是不能，但是不一定控制的住。因为线程的执行先后顺序，谁先开始，谁先结束，谁花了多少时间，这些都是无法预测的，是由操作系统调度的
                //thread.Suspend();//暂停(弃用) 暂停并一定马上暂停
                //thread.Resume();//恢复(弃用)
                //thread.Abort();//终止(快被弃用了)
                //Thread.ResetAbort();//(弃用)
                //线程是计算机资源，程序想停下线程，只能向操作系统通知(线程抛异常)，会有延时，不一定真的能停下来。
                //比如向远程服务器传文件或下载文件或本地删除大型文件，当进行到中途要终止的时候，不一定真的能马上终止下来。数据传输已经有部分了，不会说退回去的。


                ////1 等待
                //while (thread.ThreadState != ThreadState.Stopped)
                //{
                //    Thread.Sleep(200);//当前线程休息200ms
                //}
                ////2 Join等待
                //thread.Join();//运行这句代码的线程，等待thread的完成
                //thread.Join(1000);//最多等待1000ms

                ////最高优先级：优先执行，但不代表优先完成，甚至说在极端情况下，还有意外发生，操作系统调度线程时，cpu时间分片，可能改低优先级的先执行了
                //thread.Priority = ThreadPriority.Highest;//不能通过这个设置来控制线程的执行先后顺序，可以自行测试
                ////设置前后台线程
                //thread.IsBackground = true;//关闭进程(关闭程序)，线程退出
                //thread.IsBackground = false;//关闭进程(关闭程序)，线程需要计算完后才退出。应用场景：比如关系应用程序后，需要做一些操作，比如做日志等...

            }
            {
                //ParameterizedThreadStart method = obj =>
                //{
                //    Console.WriteLine(obj);
                //    this.DoSomethingLong("btnThread_Click");
                //};
                //Thread thread = new Thread(method);
                //thread.Start("123");//开启线程，执行委托，传入的参数给ParameterizedThreadStart委托
            }
            {
                ////重点。。。
                ////Thread模拟委托的异步回调，无返回值；BeginInvoke
                //ThreadStart threadStart = () => this.DoSomethingLong("btnThread_Click");
                //Action actionCallback = () =>
                //{
                //    Thread.Sleep(2000);
                //    Console.WriteLine($"This is Action CallBack {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                //};
                //this.ThreadWithCallBack(threadStart, actionCallback);
            }
            {
                ////重点。。。
                //Thread模拟委托的异步回调，有返回值；BeginInvoke/EndInvoke
                Func<int> func = () =>
                {
                    Thread.Sleep(5000);
                    Console.WriteLine($"This is Func CallBack {Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    return DateTime.Now.Year;
                };
                Func<int> funcResult = this.ThreadWithReturn<int>(func);//非阻塞
                //...中间这里可以做其他的事
                Console.WriteLine($"do somthing ...");
                Console.WriteLine($"do somthing ...");
                Console.WriteLine($"do somthing ...");
                Console.WriteLine($"do somthing ...");
                Console.WriteLine($"do somthing ...");
                int iResult = funcResult.Invoke();//阻塞
                Console.WriteLine($"返回结果：{iResult}");
            }

            //线程嵌套：比如程序启动时启动了5个线程，然后当其中某个线程运行满足条件时，就又开启2个线程，以此类推...，如果最后要等待所有线程都执行完了，获取返回结果，向这种线程嵌套只能获取第一层的5个线程返回值，另外在开启的线程值获取不到。
            //委托嵌套

            Console.WriteLine($"********************btnThread_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }

        /// <summary>
        /// Thread模拟委托的异步回调
        /// 基于Thread封装一个回调
        /// 回调：启动子线程执行动作A--不阻塞--A执行完后子线程会执行动作B
        /// 
        /// 其实这种封装在我们实际开发中用的少，因为框架已经为我们提供了很多这种回调的API了，但是这种封装方式很锻炼一个人思维，以及对Thread和delegate的理解
        /// </summary>
        /// <param name="threadStart">多线程执行的操作</param>
        /// <param name="actionCallback">线程执行完后，回调的动作</param>
        private void ThreadWithCallBack(ThreadStart threadStart,Action actionCallback)
        {
            ////此方式不行，因为Join阻塞了主线程
            //Thread thread = new Thread(threadStart);
            //thread.Start();
            //thread.Join();//错了，因为方法被阻塞了。这不是回调，回调是非阻塞的。
            //actionCallback.Invoke();

            //包一层就能实现：启动子线程执行动作A--不阻塞--A执行完后子线程会执行动作B的效果
            ThreadStart method = new ThreadStart(() =>
            {
                threadStart.Invoke();
                actionCallback.Invoke();
            });
            new Thread(method).Start();
        }
        /// <summary>
        /// 1 异步，非阻塞
        /// 2 还能获取到返回值
        /// 既要不阻塞，又要计算后返回计算结果？不可能！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private Func<T> ThreadWithReturn<T>(Func<T> func)
        {
            T t = default(T);
            ThreadStart method = new ThreadStart(() =>
            {
                t = func.Invoke();
            });
            Thread thread = new Thread(method);
            thread.Start();
            return new Func<T>(() =>
            {
                //thread.ThreadState
                thread.Join();//等待子线程执行完
                return t;
            });
        }
        //获取异步调用的返回值？

        #endregion

        #region ThreadPool
        /// <summary>
        /// Thread--功能繁多，反而用不好--就像给4岁小孩一把热武器，用不好反而会造成更大的伤害。(频繁的创建n多线程，导致影响系统性能或崩溃)
        /// ThreadPool线程池 .NetFramework2.0
        /// 如果某个对象的创建和销毁代价比较高，同时这个对象还可以反复使用的，就需要一个池子
        /// 这个池子就保存多个这样的对象，需要的时候就从池子里面获取，用完之后不用销毁，放回池子(可以通过状态来控制)；(享元模式)
        /// 这样就可以节约资源提升性能，此外还能管控总数量，防止滥用；弥补了Thread的一些问题。
        /// ThreadPool的线程都是后台线程。thread.IsBackground = true;//关闭进程(关闭程序)，线程退出
        /// 
        /// 大家可以试试，基于ThreadPool去封装回调--带返回值和不带返回值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnThreadPool_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnThreadPool_Click Start" +
             $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
             $"************************");

            {
                //ThreadPool.QueueUserWorkItem(o => this.DoSomethingLong("btnThreadPool_Click1"));
                //ThreadPool.QueueUserWorkItem(o => {
                //    this.DoSomethingLong($"btnThreadPool_Click2 {o}");//btnThreadPool_Click2 jack
                //}, "jack");//jack值会赋值给委托方法参数o
            }

            {
                //ThreadPool.GetMaxThreads(out int workThreads, out int completionPortThreads);
                //Console.WriteLine($"当前电脑最大workThreads={workThreads} 最大completionPortThreads={completionPortThreads}");

                //ThreadPool.GetMinThreads(out int workThreadsMin, out int completionPortThreadsMin);
                //Console.WriteLine($"当前电脑最小workThreadsMin={workThreadsMin} 最小completionPortThreadsMin={completionPortThreadsMin}");

                ////设置的线程池数量是进程全局的
                ////委托异步调用--Task--Parrallel--async/await 全部都是线程池的线程
                ////直接new Thread不受这个数量限制的(但是会占用线程池的线程数量)
                //ThreadPool.SetMaxThreads(123, 234);//必须大于等于当前电脑cpu的核数，否则设置无效
                //ThreadPool.SetMinThreads(8, 8);
                //Console.WriteLine("&&&&&&&&&&&&设置后&&&&&&&&&&&");

                //ThreadPool.GetMaxThreads(out int workThreads1, out int completionPortThreads1);
                //Console.WriteLine($"当前电脑最大workThreads={workThreads1} 最大completionPortThreads={completionPortThreads1}");

                //ThreadPool.GetMinThreads(out int workThreadsMin1, out int completionPortThreadsMin1);
                //Console.WriteLine($"当前电脑最小workThreadsMin={workThreadsMin1} 最小completionPortThreadsMin={completionPortThreadsMin1}");
            }

            {
                ////等待
                //ManualResetEvent mre = new ManualResetEvent(false);
                ////fasle---关闭---Set打开---WaitOne就能通过
                ////true ---打开---Reset关闭---false---WaitOne就只能等待
                //ThreadPool.QueueUserWorkItem(o =>
                //{
                //    this.DoSomethingLong("btnThreadPool_Click1");
                //    mre.Set();
                //    //mre.Reset();
                //});
                ////...中间这里可以做其他的事
                //Console.WriteLine($"do somthing ...");
                //Console.WriteLine($"do somthing ...");
                //Console.WriteLine($"do somthing ...");
                //mre.WaitOne();
                //Console.WriteLine("任务已经完成啦...");
            }

            {
                //死锁，不要阻塞线程池里面的线程。请小心死锁问题。
                ThreadPool.SetMaxThreads(12, 12);
                ManualResetEvent mre = new ManualResetEvent(false);
                for (int i = 0; i < 14; i++)
                {
                    int k = i;
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId.ToString("00")} show k={k}");
                        if (k == 13)
                        {
                            mre.Set();
                        }
                        else
                        {
                            mre.WaitOne();
                        }
                    });
                }
                //这里会出现死锁问题：因为我们设置了最多只有12个线程，当到13的时候，就没有线程了，就会一直等
                if (mre.WaitOne())
                {
                    Console.WriteLine("任务全部执行完成啦...");
                }
            }

            Console.WriteLine($"********************btnThreadPool_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        #endregion

        #region Task
        /// <summary>
        /// 1 Task：Waitall WaitAny Delay
        /// 2 TaskFactory：ContinueWhenAny ContinueWhenAll</summary>
        /// 3 并行运算Parallel.Invoke/For/Foreach
        /// 
        /// Task是.Net Framerwork3.0出现的。线程是基于线程池的，然后提供了丰富的API
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTask_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnTask_Click Start" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");

            ////task线程启动方式
            //{
            //    Task task = new Task(() => this.DoSomethingLong("btnTask_Click_1"));
            //    task.Start();
            //}
            //{
            //    Task task = Task.Run(() => this.DoSomethingLong("btnTask_Click_2"));
            //}
            //{
            //    TaskFactory taskFactory = Task.Factory;
            //    Task task = taskFactory.StartNew(() => this.DoSomethingLong("btnTask_Click_3"));
            //}

            //{
            //    ThreadPool.SetMaxThreads(12,12);//线程池是单列的，全局唯一的，是对整个项目都生效的
            //    //设置后，同时并发的task只有12个，而且是复用的，这就可以证明Task的线程是源于线程池的。
            //    //全局的，请不要这样设置！！！
            //    for (int i = 0; i < 100; i++)
            //    {
            //        int k = i;
            //        Task.Run(() =>
            //        {
            //            Console.WriteLine($"This is {k} Runing. ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            //            Thread.Sleep(200);
            //        });
            //    }
            //    //假如说我想控制下Task的并发数量，该怎么做？
            //}

            //{
            //    //Thread.Sleep和Task.Delay区别
            //    {
            //        Stopwatch stopwatch = new Stopwatch();
            //        stopwatch.Start();
            //        Console.WriteLine("在Sleep之前");
            //        Thread.Sleep(2000);//同步等待--当前线程等待2s，然后继续，阻塞
            //        Console.WriteLine("在Sleep之后");
            //        stopwatch.Stop();
            //        Console.WriteLine($"Sleep耗时：{stopwatch.ElapsedMilliseconds}");
            //    }
            //    {
            //        //使用场景：如果我们想启动一个线程，但是又不想马上执行，又不想阻塞主线程，就可以使用Delay
            //        Stopwatch stopwatch = new Stopwatch();
            //        stopwatch.Start();
            //        Console.WriteLine("在Delay之前");
            //        Task task = Task.Delay(2000).ContinueWith(t => {
            //            //Thread.Sleep(2000);//Task.Delay(2000)本质就是在延续任务里面等待了2s。
            //            stopwatch.Stop();
            //            Console.WriteLine($"Delay耗时：{stopwatch.ElapsedMilliseconds}");

            //            Console.WriteLine($"This is ContinueWith. ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            //        });//.Net Fromwork4.5出现的。异步等待--等待2s后启动一个线程执行任务，不是阻塞等待，而是在任务里面等。
            //        Console.WriteLine("在Delay之后");
            //    }
            //}

            {
                //什么时候用多线程？任务能并发的时候
                //多线程能干嘛？提升速度/优化用户体验
                Console.WriteLine("XXX开启了");
                this.Teach("Lesson1");
                this.Teach("Lesson2");
                this.Teach("Lesson3");//不能并发，因为有严格顺序，只有一个老师讲课，所以不符合并发场景
                Console.WriteLine("部署一下项目实战工作，需要多人合作完成");//这里就可以多线程，开发可以多人合作，提升性能

                TaskFactory taskFactory = new TaskFactory();
                List<Task> taskList = new List<Task>();
                taskList.Add(taskFactory.StartNew(o => this.Coding("组员1", "UI开发"),"参数1"));
                taskList.Add(taskFactory.StartNew(o => this.Coding("组员2", "数据库设计"), "参数2"));
                taskList.Add(taskFactory.StartNew(o => this.Coding("组员3", "项目框架搭建"), "参数3"));
                taskList.Add(taskFactory.StartNew(o => this.Coding("组员4", "后台服务"), "参数4"));

                {//阻塞
                    ////阻塞当前线程，等着任意一个任务完成
                    ////使用场景：列表页：核心数据可能来自数据库/远程接口/分布式搜索引擎/缓存，多线程并发请求，哪个先完成，就用哪个结果，其它的就不管了，WaitAny
                    //Task.WaitAny(taskList.ToArray());//millisecondsTimeout也可以限时等待
                    //Console.WriteLine("老师准备环境开始部署测试");

                    ////需要能够等待全部线程完成任务后在继续，阻塞当前线程，等着全部任务完成
                    ////使用场景：网站首页：A数据库 B接口 C分布式服务 D搜索引擎，这种情况就适合多线程并发，都完成后才能返回给用户，需要等待WaitAll
                    //Task.WaitAll(taskList.ToArray());
                    //Console.WriteLine("4个模块全部完成后，技术总监需要集中审查");

                    ////Task.WaitAny/Task.WaitAll 都是阻塞当前线程，等待任务完成后执行操作
                    ////阻塞卡界面，是为了并发以及顺序控制
                    ////列表页：核心数据可能来自数据库/远程接口/分布式搜索引擎/缓存，多线程并发请求，哪个先完成，就用哪个结果，其它的就不管了，WaitAny
                    ////网站首页：A数据库 B接口 C分布式服务 D搜索引擎，这种情况就适合多线程并发，都完成后才能返回给用户，需要等待WaitAll

                }

                //******************************************常用****************************************
                {
                    //非阻塞式的回调
                    ////多个任务全部完成后，回调
                    //taskFactory.ContinueWhenAll(taskList.ToArray(), tArray =>
                    //{
                    //    Console.WriteLine($"开发都完成了，大家一起庆祝一下 TaskId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    //});
                    ////任意一个任务完成后，回调
                    //taskFactory.ContinueWhenAny(taskList.ToArray(), t =>
                    //{
                    //    Console.WriteLine($"{t.AsyncState}第一个开发完成，给你一个红包奖励 TaskId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    //});
                    ////ContinueWhenAll和ContinueWhenAny非阻塞式的回调，而且使用的线程可能是新线程，也可能是刚完成任务的线程，唯一不可能是主线程。
                }
                //******************************************常用****************************************

                {//回调
                    //Task.Run(() => this.DoSomethingLong("btnTask_Click")).ContinueWith(t =>
                    //{
                    //    Console.WriteLine($"btnTask_Click已完成_{Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    //});//回调
                }
                {//获取返回值，阻塞
                   //Task<int> iResult = Task.Run<int>(() =>
                   // {
                   //     Thread.Sleep(2000);
                   //     return DateTime.Now.Year;
                   // });
                   // int i = iResult.Result;//会阻塞
                }
                {//获取返回值，并传入回调函数，非阻塞
                    //Task.Run<int>(() =>
                    //{
                    //    Thread.Sleep(2000);
                    //    return DateTime.Now.Year;
                    //}).ContinueWith(tInt=> {
                    //    int i = tInt.Result;
                    //});
                }
                {
                    ////假如说我想控制下Task的并发数量，该怎么做？线程最大不超过20个。使用Parallel是更好的解决方案！
                    //List<Task> taskList1 = new List<Task>();
                    //for (int i = 0; i < 10000; i++)
                    //{
                    //    if (taskList.Count(t => t.Status != TaskStatus.RanToCompletion) >= 20)
                    //    {
                    //        Task.WaitAny(taskList.ToArray());
                    //        taskList = taskList.Where(t => t.Status != TaskStatus.RanToCompletion).ToList();
                    //    }
                    //    taskList1.Add(Task.Run(() =>
                    //    {
                    //        Console.WriteLine($"TaskId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    //        Thread.Sleep(2000);
                    //    }));
                    //}

                    //这种方式不可取
                    //for (int i = 0; i < 10000; i++)
                    //{
                    //    Task.Run(() =>
                    //    {
                    //        Console.WriteLine($"TaskId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                    //        Thread.Sleep(2000);
                    //    });
                    //}
                }

            }

            Console.WriteLine($"********************btnTask_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        #endregion

        #region Parallel
        private void btnParallel_Click(object sender, EventArgs e)
        {
            //假如说我想控制下Task的并发数量，该怎么做？
            Console.WriteLine($"********************btnParallel_Click Start" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");

            //Parallel.For和Parallel.Invoke和Parallel.ForEach效果一样
            //Parallel并发执行多个Action 多线程的，主线程也会参与计算--阻塞界面
            //等于TaskWaitAll+主线程计算

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 3;

            {
                //未控制并发数量--阻塞
                Parallel.Invoke(
                    () => this.DoSomethingLong("btnParallel_Click1"),
                    () => this.DoSomethingLong("btnParallel_Click2"),
                    () => this.DoSomethingLong("btnParallel_Click3"),
                    () => this.DoSomethingLong("btnParallel_Click4"),
                    () => this.DoSomethingLong("btnParallel_Click5"));

                //已控制并发数量--阻塞
                Parallel.Invoke(options,
                    () => this.DoSomethingLong("btnParallel_Click1"),
                    () => this.DoSomethingLong("btnParallel_Click2"),
                    () => this.DoSomethingLong("btnParallel_Click3"),
                    () => this.DoSomethingLong("btnParallel_Click4"),
                    () => this.DoSomethingLong("btnParallel_Click5"));
            }
            {
                //未控制并发数量--阻塞
                Parallel.For(0, 5, i =>
                {
                    this.DoSomethingLong($"btnParallel_Click_{i}");
                });
                //已控制并发数量--阻塞
                Parallel.For(0, 10, options, i =>
                {
                    this.DoSomethingLong($"btnParallel_Click_{i}");
                });
            }
            {
                //未控制并发数量--阻塞
                Parallel.ForEach(new List<string>() { "111", "222", "333" }, str =>
                   {
                       this.DoSomethingLong($"btnParallel_Click_{str}");
                   });
                //已控制并发数量--阻塞
                Parallel.ForEach(new List<string>() { "111", "222", "333" }, options, str =>
                 {
                     this.DoSomethingLong($"btnParallel_Click_{str}");
                 });
            }
            {
                //有没有办法不阻塞？包一层
                Task.Run(() =>
                {
                    //已控制并发数量
                    Parallel.ForEach(new List<string>() { "111", "222", "333" }, options, str =>
                    {
                        this.DoSomethingLong($"btnParallel_Click_{str}");
                    });
                });
                //Parallel线程取消停止
            }

            //几乎90%以上的多线程场景，以及顺序控制，以上的Task方法就可以完成，如果你的线程场景太复杂搞不定，那么请梳理一下你的流程，简化一下。
            //建议最好不要线程嵌套线程，两层勉强能懂，三次hold不住了，更多只能求神

            Console.WriteLine($"********************btnParallel_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        #endregion

        #region 多线程异常处理
        private void btnException_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnException_Click Start" +
            $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
            $"************************");

            //{
            //    try
            //    {
            //        //这种方式就是Task.WaitAll全部线程执行完后在捕获异常，根据自己实际业务场景选择把，如果不需要对某个线程异常后，做通知其它线程取消等操作，就可以使用这种方式
            //        List<Task> taskList = new List<Task>();
            //        for (int i = 0; i < 100; i++)
            //        {
            //            string name = $"btnException_Click_{i}";//临时变量
            //            taskList.Add(Task.Run(() =>
            //            {
            //                if (name.Equals("btnException_Click_11"))
            //                {
            //                    throw new Exception("btnException_Click_11异常");
            //                }
            //                else if (name.Equals("btnException_Click_12"))
            //                {
            //                    throw new Exception("btnException_Click_12异常");
            //                }
            //                else if (name.Equals("btnException_Click_38"))
            //                {
            //                    throw new Exception("btnException_Click_38异常");
            //                }
            //                Console.WriteLine($"This is {name}成功 ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            //            }));
            //        }
            //        //多线程里面抛出的异常，只会终结当前线程；不会影响别的线程；
            //        //那线程异常哪里去了？被吞了，
            //        //假如我想获取异常信息，还需要通知别的线程
            //        Task.WaitAll(taskList.ToArray());//1 可以捕获线程异常
            //    }
            //    catch(AggregateException aex)//2 需要try-catch-AggregateException
            //    {
            //        //AggregateException专门用于捕获多线程的异常
            //        foreach (var exception in aex.InnerExceptions)
            //        {
            //            Console.WriteLine(exception.Message);
            //        }
            //    }
            //    catch (Exception ex)//可以多catch，先匹配具体异常，如果为捕获在全部捕获
            //    {
            //        Console.WriteLine(ex);
            //    }
            //}

            {
                //线程异常后经常是需要通知别的线程，而不是等到WaitAll全部执行完，问题就是要线程取消
                //工作中常规建议：多线程的委托里面不允许异常，包一层try-catch，然后记录下来异常信息，完成需要的操作

                List<Task> taskList = new List<Task>();
                for (int i = 0; i < 100; i++)
                {
                    string name = $"btnException_Click_{i}";//临时变量
                    taskList.Add(Task.Run(() =>
                    {
                        //正确做法是在线程里面包一层try-catch，而不是等到Task.WaitAll全部线程执行完了在做响应处理
                        //因为这样做就可以在其中某个线程执行异常后，可以及时做一些其他额外操作，比如通知其它线程取消等...
                        try
                        {
                            if (name.Equals("btnException_Click_11"))
                            {
                                throw new Exception("btnException_Click_11异常");
                            }
                            else if (name.Equals("btnException_Click_12"))
                            {
                                throw new Exception("btnException_Click_12异常");
                            }
                            else if (name.Equals("btnException_Click_38"))
                            {
                                throw new Exception("btnException_Click_38异常");
                            }
                            Console.WriteLine($"This is {name}成功 ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }));
                }
                Task.WaitAll(taskList.ToArray());

            }

            Console.WriteLine($"********************btnException_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        #endregion

        #region 线程取消(标准做法)
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnCancel_Click Start" +
            $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
            $"************************");

            {
                //多线程并发任务，某个线程失败后，我们希望通知别的线程，都停下来，怎么做呢？如果有部分线程还没有启动，能不能就不启动了呢？
                //Thread.Abort--终止线程：原理是向当前线程抛一个异常来终结任务；
                //线程属于操作系统(OS)资源，可能不会立即停下来，因为当我们向操作系统发送一个请求要求停止线程时，最终需要操作系统的CPU响应，我们不知道CPU什么时候能接受到这个异常请求，就不知道什么时候能终止，
                //还有可能就是当前向操作系统发送取消线程操作时，可能这个线程已经被操作系统的CPU执行完了。
                //这方式就是无法掌控的不精准的，所以就不是很靠谱，不建议这么做。
                //所以多线程不是我们想象的这么简单，想暂停就暂停，先重启就重启，想终止就终止，这个都是需要操作系统调度的，时间是不能确定的，是由操作系统决定什么时候执行的。

                //Task不能外部终止任务，没有这种api，只能自己终止自己(上帝才能打败自己)，在线程内部！！！

                //非标准做法：共享变量var isOk=true;
                //标准做法：CancellationTokenSource里面有个IsCancellationRequested属性默认false，调用Cancel方法后变成true(不能在变回去)，可以重复Cancel
                //步骤：1准备CancellationTokenSource 2try-catch-Cancel 3Action里面要随时判断IsCancellationRequested
                //尽快取消，肯定有延迟，在判断环节才会结束

                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    List<Task> taskList = new List<Task>();
                    for (int i = 0; i < 50; i++)
                    {
                        string name = $"btnException_Click_{i}";//临时变量
                        taskList.Add(Task.Run(() =>
                        {
                            try
                            {
                                if (!cts.IsCancellationRequested)
                                    Console.WriteLine($"This is {name}开始 ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");

                                Thread.Sleep(new Random().Next(50, 100));//建议在这里sleep一下

                                if (name.Equals("btnException_Click_11"))
                                {
                                    //异常取消
                                    throw new Exception("btnException_Click_11异常");
                                }
                                else if (name.Equals("btnException_Click_12"))
                                {
                                    //异常取消
                                    throw new Exception("btnException_Click_12异常");
                                }
                                else if (name.Equals("btnException_Click_13"))
                                {
                                    //符合某种条件取消
                                    cts.Cancel();
                                }

                                if (!cts.IsCancellationRequested)//业务核心环节判断是否需要继续
                                {
                                    Console.WriteLine($"This is {name}结束 ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                                }
                                else
                                {
                                    Console.WriteLine($"This is {name}中途停止 ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                                    return;//
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                cts.Cancel();
                            }
                        }, cts.Token)); //启动线程是传递cts.Token，一旦我们调用了cts.Cancel()方法，那么cts.Token就会抛一个cts.Token.ThrowIfCancellationRequested异常，终止还没有启动的线程，他们就不会启动了。
                    }
                    Task.WaitAll(taskList.ToArray());
                    //如果有部分线程还没有启动，能不能就不启动了呢？
                    //1 启动线程传递Token 2 异常抓取 
                    //在Cancel时还没有启动的任务，就不启动了，也是抛异常cts.Token.ThrowIfCancellationRequested
                }
                catch (AggregateException aex)
                {
                    foreach (var exception in aex.InnerExceptions)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine($"********************btnCancel_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        #endregion

        #region 临时变量
        private void btnVariable_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnVariable_Click Start" +
            $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
            $"************************");

            //for (int i = 0; i < 5; i++)
            //{
            //    Task.Run(() =>
            //    {
            //        Console.WriteLine($"This is btnVariable_Click_{i} ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
            //    });
            //}

            //声明变量时堆栈存储、闭包不仅仅是在js中体现，在c#中也是同样的道理，C#委托里面对方法外部的变量引用就和js里面的闭包一样。

            //临时变量问题，线程是非阻塞的，飞快的创建了5个线程，然后线程又是延迟启动的，等着线程去执行的时候，i已经是5了，
            //k是闭包里面的变量，每次循环都有一个独立的k。
            //注意：类似这样的用法不光是多线程里面有，比如有时候用for/foreach=》创建委托，但是委托占时没有执行，然后把委托传递给别人了，但是在后面又把这个委托给修改了，如果委托把变量带走了，包括那个时候的变量，有可能就有问题。
            for (int i = 0; i < 5; i++)//i只有一个，在栈里面只有1个
            {
                int k = i;//k每次都是一个全新的变量，在内存栈中有5个。
                Task.Run(() =>
                {
                    Console.WriteLine($"This is btnVariable_Click_{i}_{k} ThreadId={Thread.CurrentThread.ManagedThreadId.ToString("00")}");
                });
            }

            Console.WriteLine($"********************btnVariable_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        #endregion

        #region 线程安全&lock
        private void btnLock_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************btnLock_Click Start" +
           $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
           $"************************");

            //线程安全：如果你的代码在进程中有多个线程同时运行这一段，如果每次运行的结果都跟单线程运行的结果一致，那么就是线程安全的。
            //线程安全问题一般都是有全局变量/共享变量/静态变量/硬盘文件/数据库的值，只要多线程都能访问和修改

            {
                ////同步阻塞操作
                //for (int i = 0; i < 10000; i++)
                //{
                //    this.iNumSync++;
                //}
                ////异步多线程非阻塞操作(非线程安全)
                //for (int i = 0; i < 10000; i++)
                //{
                //    Task.Run(() =>
                //    {
                //        this.iNumAsync++;
                //    });
                //}
                ////异步多线程非阻塞操作(非线程安全)
                //for (int i = 0; i < 10000; i++)
                //{
                //    int k = i;
                //    Task.Run(() =>
                //    {
                //        this.iListAsync.Add(k);
                //    });
                //}
                //Thread.Sleep(5 * 1000);
                //Console.WriteLine($"iNumSync={iNumSync};iNumAsync={iNumAsync};iListAsync={iListAsync.Count}");
            }

            //多个线程同时操作内存，导致覆盖问题。怎么解决？
            {
                {
                    //1 lock(不推荐)：lock是语法糖，Monitor.Enter占据一个引用，别的线程就只能等着，建议lock里面的代码不要太多。这里是单线程。
                    //推荐锁是private static readonly object，不能是null；也不应该是string(string会享元模式)
                    {
                        for (int i = 0; i < 10000; i++)
                        {
                            Task.Run(() =>
                            {
                                lock (_lock)//任意时候只有一个线程进入方法块，这不就变成了单线程
                                {
                                    this.iNumAsync++;
                                }
                            });
                        }
                        Thread.Sleep(5 * 1000);
                        Console.WriteLine($"iNumSync={iNumSync};iNumAsync={iNumAsync};iListAsync={iListAsync.Count}");
                    }

                    ////lock(this)，外面如果也要用实例，就冲突了
                    //{
                    //    Test test = new Test();
                    //    Task.Delay(1000).ContinueWith(t =>
                    //    {
                    //        lock (test)
                    //        {
                    //            Console.WriteLine("*************start************");
                    //            Thread.Sleep(5000);
                    //            Console.WriteLine("*************end************");
                    //        }
                    //    });
                    //    test.DoTest();
                    //    //this和test是同一个对象，会导致冲突，排队问题。string也是相同问题。
                    //}
                }
                {
                    //2 线程安全集合：System.Collections.Concurrent这个命名空间下的集合。保证集合操作的原子性
                    //比如：System.Collections.Concurrent.ConcurrentQueue<int>
                }
                {
                    //3 数据分拆(推荐)：避免多线程操作同一个数据；既安全又高效。需要在一定场景下应用。否则就考虑lock
                    //比如有100个任务需要处理，开了10个线程去处理，我们加锁又会影响性能，那就拆分，1-10个任务给一个线程，10-20任务给一个线程，以此类推，大家各搞各的，互不影响，操作完成后在合并。
                }
            }

            Console.WriteLine($"********************btnLock_Click End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        //标准lock锁写法
        private static readonly object _lock = new object();

        private int iNumSync = 0;//同步的变量字段
        private int iNumAsync = 0;//异步的变量字段，非线程安全
        private List<int> iListAsync = new List<int>();//异步的集合，非线程安全
        /// <summary>
        /// 为了搞清楚this和实例对象锁的测试
        /// </summary>
        public class Test
        {
            /// <summary>
            /// 这是非常经典的一个面试题
            /// </summary>
            public void DoTest()
            {
                lock (this)//递归调用lock this会不会死锁？不会死锁，但是会死循环，是不是很奇怪。（因为这里是同一个线程，这个引用就是被这个线程所占据。锁是排他，不是排自己！）
                {
                    Thread.Sleep(500);
                    this.iDoTestNum++;
                    if (DateTime.Now.Day < 12 && this.iDoTestNum < 10)
                    {
                        Console.WriteLine($"This is {DateTime.Now.Day};i={this.iDoTestNum}");
                        this.DoTest();
                    }
                    else
                    {
                        Console.WriteLine("11号结束啦");
                    }
                }
            }
            private int iDoTestNum = 0;
        }
        #endregion

        #region async/await(不推荐在winform使用，有时候会出现一些莫名其妙的问题，很难解释通)
        private void button4_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"********************async/await Start" +
            $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
            $"************************");

            MessageBox.Show("不推荐在winform使用，有时候会出现一些莫名其妙的问题，很难解释通。请移步【MVC项目】和【控制台应用程序】查看测试demo！！！");

            Console.WriteLine($"********************async/await End" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }
        #endregion

        #region private
        private void DoSomethingLong(string name)
        {
            Console.WriteLine($"********************DoSomethingLong Start {name}" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
            long iResult = 0;
            //打开任务管理器查看CPU利用率对比同步和异步cpu利用率情况
            for (int i = 0; i < 1000_000_000; i++)
            {
                iResult += i;
            }
            //Thread.Sleep(2000);
            Console.WriteLine($"********************DoSomethingLong End {name}" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }

        private void Teach(string lesson)
        {
            Console.WriteLine($"{lesson}开始讲。。。");
            long iResult = 0;
            //打开任务管理器查看CPU利用率对比同步和异步cpu利用率情况
            for (int i = 0; i < 1000_000_000; i++)
            {
                iResult += i;
            }
            Console.WriteLine($"{lesson}讲完啦。。。");
        }

        private void Coding(string name, string projectName)
        {
            Console.WriteLine($"********************Coding Start {name} {projectName}" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
            long iResult = 0;
            //打开任务管理器查看CPU利用率对比同步和异步cpu利用率情况
            for (int i = 0; i < 1000_000_000; i++)
            {
                iResult += i;
            }
            //Thread.Sleep(2000);
            Console.WriteLine($"********************Coding End {name} {projectName}" +
              $" {Thread.CurrentThread.ManagedThreadId.ToString("00")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" +
              $"************************");
        }


        #endregion

        
    }
}
