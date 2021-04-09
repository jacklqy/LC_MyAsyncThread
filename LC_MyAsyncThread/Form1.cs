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

        #region AsyncAdvanced
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
