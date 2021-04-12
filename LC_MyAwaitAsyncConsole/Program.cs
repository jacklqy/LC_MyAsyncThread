using AwaitAsyncLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LC_MyAwaitAsyncConsole
{
    /// <summary>
    /// 1 await/async语法和使用
    /// 2 原理探究和使用建议
    /// 
    /// await/async 是c#保留关键字，通常是成对出现的
    /// async是修饰方法的，可以单独出现，但是有警告
    /// await在方法体，只能出现在task/async方法前面，只有await会报错。
    /// 
    /// .NetFramework4.5----await/async语法糖：由编译器提供的功能
    /// 
    /// 主线程调用await/async放，主线程遇到await后，就返回执行后续动作，
    /// await后面的代码会等着task任务完成后在继续执行，
    /// 其实就像把await后面的代码包装成一个ContinueWith的回调动作，
    /// 然后这个回调动作可能是Task线程，也可能是新的子线程，也可能是主线程
    /// 
    /// 一个async方法，如果没有返回值，方法声明可以返回Task
    /// ***await/async能够用同步的方式编写代码，但又是非阻塞的***
    /// 
    /// await/async是语法糖，本身就是编译器提供的功能
    /// 
    /// 
    /// async方法在编译后会生成一个状态机(实新了IAsyncStateMachine接口)
    /// 状态机：初始化状态0--执行就修改状态1--再执行就修改状态0--执行就修改状态1---如果出现其他状态就结束了
    ///         红绿灯
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**************************这里是控制台测试(async/await)**************************");
            AwaitAsyncClass.TestShow();

            //可以通过反编译查看
            //AwaitAsyncILSpy.Show();
        }
    }
}
