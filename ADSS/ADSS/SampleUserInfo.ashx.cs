using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
//using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Microsoft.ApplicationBlocks.Data;
using System.Text;

namespace ADSS
{
    /// <summary>
    /// Summary description for SampleUserInfo
    /// </summary>
    public class SampleUserInfo : IHttpHandler
    {
        private class StatItem
        {
            public int id { get; set; }
            //public string token { get; set; }
            public int action { get; set; }
            public string time { get; set; }
            public string ip { get; set; }
        }
        public void ProcessRequest(HttpContext context)
        {
            string strResult = "{}";
            if (context.Request.QueryString["token"] != null)
            {
                strResult = getDetailInfo(Convert.ToString(context.Request.QueryString["token"]));
            }
            else
            {
                strResult = getResult();
            }

            //context.Response.ContentType = "application/json";
            context.Response.ContentType = "text/html";
            context.Response.Charset = Encoding.UTF8.WebName;
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.Write(strResult);
        }

        private string getDetailInfo(string token)
        {
            string strSQL;
            try
            {
                string strToken = string.Format("'{0}' order by s.id, s.time", token);
                strSQL = "select s.id, s.action, DATEDIFF(SECOND,{d '1970-01-01'}, s.time) as time, s.ip from tb_user_info as i, tb_user_stat as s where i.token = s.token and s.valid = 1 and i.ip = s.ip and s.token = " + strToken;
                // what is the matter with this? {d '1970-01-01'}??? as {0}, {1}?? maybe
                //strSQL = string.Format("select s.id, s.token, s.action, DATEDIFF(SECOND,{d '1970-01-01'}, s.time), s.ip from tb_user_info as i, tb_user_stat as s where i.token = s.token and s.valid = 1 and i.ip = s.ip and s.token = '{0}' order by s.id, s.time", token);
                // DATEDIFF(SECOND,{d '1970-01-01'}, s.time) as timestamp
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleUserInfo --- Exception: " + e.Message);
                return "{}";
            }
            
            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            List<StatItem> sl = new List<StatItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                StatItem si = new StatItem();
                                si.id = Convert.ToInt32(r[0]);
                                //si.token = Convert.ToString(r[1]);
                                si.action = Convert.ToInt32(r[1]);
                                si.time = Convert.ToString(r[2]);
                                si.ip = Convert.ToString(r[3]);

                                sl.Add(si);
                            }
                            return new JavaScriptSerializer().Serialize(sl);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleUserInfo --- Exception: " + e.Message);
            }
            return "{}";
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string getResult()
        {
            string strSQL = "select distinct(i.token), i.ip, i.agent, i.language, i.color_depth, i.screen_resolution, i.time_zone, i.platform, i.device, i.os, i.country, i.province, i.city from tb_user_info as i, tb_user_stat as s where i.token = s.token and s.valid = 1 and i.ip = s.ip";
            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            List<UserFingerPrint> uInfo = new List<UserFingerPrint>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                UserFingerPrint fp = new UserFingerPrint();
                                fp.token = Convert.ToString(r[0]);
                                fp.ip = Convert.ToString(r[1]);
                                fp.agent = Convert.ToString(r[2]);
                                fp.language = Convert.ToString(r[3]);
                                fp.color_depth = Convert.ToInt32(r[4]);
                                fp.screen_resolution = Convert.ToString(r[5]);
                                fp.time_zone = Convert.ToInt32(r[6]);
                                fp.platform = Convert.ToString(r[7]);
                                fp.device = Convert.ToString(r[8]);
                                fp.os = Convert.ToString(r[9]);
                                fp.country = Convert.ToString(r[10]);
                                if (String.Equals(fp.country, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    fp.country = "";
                                }
                                fp.province = Convert.ToString(r[11]);
                                if (String.Equals(fp.province, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    fp.province = "";
                                }
                                fp.city = Convert.ToString(r[12]);
                                if (String.Equals(fp.city, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    fp.city = "";
                                }
                                uInfo.Add(fp);
                            }
                            return new JavaScriptSerializer().Serialize(uInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleUserInfo --- Exception: " + e.Message);
            }
            return "{}";
        }
    }
}