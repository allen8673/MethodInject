using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MethodInject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // 測試注入前
            HelloClass.SayHello();
            // 注入方法
            Injection.install<InjectClass, HelloClass>("InjectMethod_A", "SayHello");
            // 測試注入後
            HelloClass.SayHello();
        }
    }

    /// <summary>
    /// 這是注入目標
    /// </summary>
    public class HelloClass
    {
        public static void SayHello()
        {
            Console.WriteLine("Hello World");
        }
    }

    /// <summary>
    /// 這是注入來源
    /// </summary>
    public class InjectClass
    {
        public void InjectMethod_A()
        {
            Console.WriteLine("Inject method A");
        }

        public void InjectMethod_B()
        {
            Console.WriteLine("Inject method B");
        }
    }

    /// <summary>
    /// Ref: https://stackoverflow.com/questions/7299097/dynamically-replace-the-contents-of-a-c-sharp-method
    /// </summary>
    public class Injection
    {
        /// <summary>
        /// 注入方法至指定類別的指定方法"前"
        /// </summary>
        /// <typeparam name="Src">來源型別</typeparam>
        /// <typeparam name="Target">目標型別</typeparam>
        /// <param name="scrMethod">來源方法</param>
        /// <param name="targetMethod">目標方法</param>
        public static void install<Src, Target>(string scrMethod, string targetMethod)
        {
            #region 取得&準備 來源&目標方法
            MethodInfo scrMethodInfo = typeof(Src).GetMethod(scrMethod, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            MethodInfo targetMethodInfo = typeof(Target).GetMethod(targetMethod, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            RuntimeHelpers.PrepareMethod(targetMethodInfo.MethodHandle);
            RuntimeHelpers.PrepareMethod(scrMethodInfo.MethodHandle);
            #endregion

            // unsafe修辭: 允許直接存取底層記憶體，不透過CLR監控
            unsafe
            {
                #region 這塊還沒看懂在幹嘛 哈哈哈
                //TODO: 查IntPtr.Size的判斷是幹嘛用的，後面會影響記憶體位置 +2 或 +1
                bool inPtrSize = IntPtr.Size == 4;

                int* src = (int*)scrMethodInfo.MethodHandle.Value.ToPointer() + (inPtrSize ? 2 : 1);
                int* tar = (int*)targetMethodInfo.MethodHandle.Value.ToPointer() + (inPtrSize ? 2 : 1);
                byte* srcInst = (byte*)*src;
                byte* tarInst = (byte*)*tar;
                int* injSrc = (int*)(srcInst + 1);
                int* tarSrc = (int*)(tarInst + 1);
                *tarSrc = (((int)srcInst + 5) + *injSrc) - ((int)tarInst + 5); 
                #endregion
            }
        }
    }
}
