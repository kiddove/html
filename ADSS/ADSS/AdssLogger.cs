using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Linq;
using System.Web;

namespace ADSS
{
    public class AdssLogger
    {
        //public static void WriteLog(string strPage, string strIp, string strContent)
        //{
        //    // time + processID + page + ip? + content
        //    Trace.WriteLine(DateTime.Now.ToString() + " " + Process.GetCurrentProcess().Id + " " + strPage + " " + strIp + " " + strContent);
        //}
        public static void WriteLog(string strContent)
        {
            // time + processID + page + ip? + content
            Trace.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ", " + Process.GetCurrentProcess().Id + ", " + strContent);
        }
    }
}