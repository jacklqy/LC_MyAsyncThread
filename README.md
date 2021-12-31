# LC_MyAsyncThread
### 基本概念

#### 阻塞

阻塞状态指程序未得到所需计算资源时被挂起的状态。程序在等待某个操作完成期间，自身无法继续处理其他的事情，则称该程序在该操作上是阻塞的。阻塞随时都可能发生，最典型的就是 I/O 中断（包括网络 I/O 、磁盘 I/O 、用户输入等）、休眠操作、等待某个线程执行结束，甚至包括在 CPU 切换上下文时，程序都无法真正的执行，这就是所谓的阻塞。

#### 非阻塞

程序在等待某操作过程中，自身不被阻塞，可以继续处理其他的事情，则称该程序在该操作上是非阻塞的。非阻塞并不是在任何程序级别、任何情况下都可以存在的。仅当程序封装的级别可以囊括独立的子程序单元时，它才可能存在非阻塞状态。显然，某个操作的阻塞可能会导程序耗时以及效率低下，所以我们会希望把它变成非阻塞的。

#### 同步

不同程序单元为了完成某个任务，在执行过程中需靠某种通信方式以协调一致，我们称这些程序单元是同步执行的。例如前面讲过的给银行账户存钱的操作，我们在代码中使用了“锁”作为通信信号，让多个存钱操作强制排队顺序执行，这就是所谓的同步。

#### 异步

不同程序单元在执行过程中无需通信协调，也能够完成一个任务，这种方式我们就称之为异步。例如，使用爬虫下载页面时，调度程序调用下载程序后，即可调度其他任务，而无需与该下载任务保持通信以协调行为。不同网页的下载、保存等操作都是不相关的，也无需相互通知协调。很显然，异步操作的完成时刻和先后顺序并不能确定。

很多人都不太能准确的把握这几个概念，这里我们简单的总结一下，同步与异步的关注点是**消息通信机制**，最终表现出来的是“有序”和“无序”的区别；阻塞和非阻塞的关注点是**程序在等待消息时状态**，最终表现出来的是程序在等待时能不能做点别的。如果想深入理解这些内容，推荐大家阅读经典著作[《UNIX网络编程》](https://item.jd.com/11880047.html)，这本书非常的赞。

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
    /// 4 Task：Waitall WaitAny Delay
    /// 5 TaskFactory：ContinueWhenAny ContinueWhenAll
    /// 6 并行运算Parallel.Invoke/For/Foreach
    /// 7 await/async
    /// </summary>
    
    //------------------------------------await/async------------------------------------
    
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

![image](https://user-images.githubusercontent.com/26539681/114360767-f825c280-9ba7-11eb-8282-787c62327a8d.png)
![image](https://user-images.githubusercontent.com/26539681/114360835-1095dd00-9ba8-11eb-9e9b-cad7ea0ceeb4.png)
![image](https://user-images.githubusercontent.com/26539681/114360891-20152600-9ba8-11eb-9bc9-6014090c65f3.png)


希望为.net开源社区尽绵薄之力，探lu者###一直在探索前进的路上###（QQ:529987528）
