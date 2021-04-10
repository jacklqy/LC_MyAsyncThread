using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    }
}
