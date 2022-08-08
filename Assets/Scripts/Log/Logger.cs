#define console
//#define unity

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;





//===============================================
//名 称 ：Log 
//描 述 ：打印日志 Assets/Scripts/Log/Log.cs 
//作 者 ：LewisLiu  
//邮 箱 ：2116788383@qq.com 
//时 间 ：2021.08.25 10:55:38  
//===============================================

//兼容windows控制台 unity控制台

#if unity
using UnityEngine;
#endif


public  class Logger: SingleInstance<Logger>
{
    private Logger()
    { 
    }
    /// <summary>
    /// color 转换hexadecimal
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    //    public   string ToHexColor(this Color color)
    //    {
    //        int r = (int)Math.Round(color.R * 255.0f);
    //        int g = (int)Math.Round(color.G * 255.0f);
    //        int b= (int)Math.Round(color.B * 255.0f);
    //        int a = (int)Math.Round(color.A * 255.0f);

    //        string hex = string.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
    //        //string hex = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
    //        return hex;
    //    }

    //    public  void Log(object log)
    //    {
    //#if (unity)
    //        Debug.Log(string.Format("<color=#FFFFFF>{0}</color>", log));
    //#elif (windows)
    //        Console.WriteLine(string.Format("<color=#FFFFFF>{0}</color>", log));
    //#endif
    //    }

    //    public  void Log(Color color, object log)
    //    {
    //        string col = "<color=#" + color.ToHexColor() + ">{0}</color>";

    //#if (unity)
    //        Debug.Log(string.Format(col, log));
    //#elif (windows)
    //        Console.WriteLine(string.Format(col, log));
    //#endif


    //    }

    public  void Log(string hexColor, object log)
    {
        string col = "<color=#" + hexColor + ">{0}</color>";

#if (unity)
        Debug.Log(string.Format(col, log));
#elif (console)
        Console.WriteLine(string.Format(col, log));
#endif

        //Debug.Log(string.Format(col, log));
        //Debug.Log(col);
    }

    public  void Log(ColorType type, object log)
    {
        //Console.WriteLine("font color:{0}", Console.ForegroundColor);
        string hexColor = "FFFFFF";
        switch (type)
        {
            case ColorType.Red:
                hexColor = "FF0000";
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case ColorType.Yellow:
                hexColor = "FFE900";
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case ColorType.Blue:
                hexColor = "00EAFF";
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            case ColorType.Green:
                hexColor = "07FF00";
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case ColorType.Purple:
                hexColor = "9800FF";
                
                break;
            case ColorType.Pink:
                hexColor = "FF00D2";
                
                break;
            case ColorType.Gray:
                //hexColor = "FF00D2";
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            default:
                break;
        }
        string col = "<color=#" + hexColor + ">{0}</color>";
#if unity
        Debug.Log(string.Format(col, log));
#elif console
        PrintLog(type, (string)(log));
#endif
        //WriteLog(type,(string)(log));
    }

    void PrintLog(ColorType type, object log)
    {
        string logType = "DEBUG";
        switch (type)
        {
            case ColorType.Red:
                logType = "ERROR";
                break;
            case ColorType.Yellow:
                logType = "WARNING";
                break;
            case ColorType.Green:
                logType = "INFO";
                break;
            case ColorType.Gray:
                logType = "DEBUG";
                break;
        }

        StackTrace st = new StackTrace(true);
      
        StackFrame sf = st.GetFrame(3);//调用堆栈





        string[] split = sf.GetFileName().Split(new char[] { '\\' }, 6);
        Console.WriteLine(string.Format("[{0}][{1} {2}][{3},line:{4}][{5}]", logType, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.TimeOfDay.ToString(), split[split.Length - 1], sf.GetFileLineNumber(), log));
        Console.ForegroundColor = ConsoleColor.Gray;

    }
    public  void Debug(object log)
    {
        Log(ColorType.Gray, log);
    }

    public  void Info(object log)
    {
        Log(ColorType.Green, log);
    }

    public  void Warning(object log)
    {
        Log(ColorType.Yellow, log);
    }

    public  void Error(object log)
    {
        Log(ColorType.Red,log);
    }


    public  void WriteLog(ColorType type,string msg)
    {
        string filePath = AppDomain.CurrentDomain.BaseDirectory + "Log";
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        string logPath = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        try
        {

            using (StreamWriter sw = File.AppendText(logPath))
            {
                string logType = "DEBUG";
                switch (type)
                {
                    case ColorType.Red:
                        logType = "ERROR";
                        break;
                    case ColorType.Yellow:
                        logType = "WARNING";
                        break;
                    case ColorType.Green:
                        logType = "INFO";
                        break;
                    case ColorType.Gray:
                        logType = "DEBUG";
                        break;
                }

                StackTrace st = new StackTrace(true);
                //Console.WriteLine(" Stack trace for current level: {0}", st.ToString());
                StackFrame sf = st.GetFrame(3);//调用堆栈

                //Console.WriteLine(" File: {0}", sf.GetFileName());
                //Console.WriteLine(" Method: {0}", sf.GetMethod().Name);
                //Console.WriteLine(" Line Number: {0}", sf.GetFileLineNumber());
                //Console.WriteLine(" Column Number: {0}", sf.GetFileColumnNumber());

                string[] split = sf.GetFileName().Split(new char[] { '\\' }, 6);

                sw.WriteLine(string.Format("[{0}][{1} {2}][{3},line:{4}][{5}]", logType, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.TimeOfDay.ToString(), split[split.Length - 1], sf.GetFileLineNumber(), msg));
                //sw.WriteLine();
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
        }
        catch (IOException e)
        {
            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine("时间：" + DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
                sw.WriteLine("异常：" + e.Message + "\r\n" + e.StackTrace);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
        }
    }
}
public enum ColorType
{
    Red,
    Yellow,
    Blue,
    Green,
    Purple,
    Pink,
    Gray
}
