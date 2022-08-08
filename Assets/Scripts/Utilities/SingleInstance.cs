using System;
using System.Reflection;
/// <summary>
/// 带接口的单例，通常用于可MOCK的类
/// </summary>
/// <typeparam name="T">单例的类型</typeparam>
/// <typeparam name="I">单例类型的接口</typeparam>
public class SingleInstance<T, I> where T : I
{
    private static object lockObj = new object();
    private static T mySelf = default(T);
    public static I Instance
    {
        get
        {
#if DEBUG
            if (Mocking != null)
                return Mocking;
#endif
            lock (lockObj)
            {
                if (mySelf == null)
                {
                    mySelf = InstanceCreater.CreateInstance<T>();
                }
            }

            return mySelf;
        }
    }

#if DEBUG
    public static I Mocking { get; set; }
#endif

}

/// <summary>
/// 单例,需要在当前类中定义
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingleInstance<T> where T : class
{
    private static object lockObj = new object();
    private static T mySelf;//= default(T);
    public static T Instance
    {
        get
        {
            lock (lockObj)
            {
                if (mySelf == null)
                {
                    mySelf = InstanceCreater.CreateInstance<T>();
                }
            }

            return mySelf;
        }
    }
#if MOCK
        public static void InstanceClear()
        {
           mySelf=null;
        } 
#endif
}

static class InstanceCreater
{
    public static T CreateInstance<T>()
    {
        var type = typeof(T);
        try
        {
            return (T)type.Assembly.CreateInstance(type.FullName, true, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
        }
        catch (MissingMethodException ex)
        {
            throw new System.Exception(string.Format("{0}(单例模式下，构造函数必须为private)", ex.Message));
        }
    }
}