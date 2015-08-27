using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.ApplicationBlocks.Data;

namespace ADSS
{
    /// <summary>
    /// Summary description for Actstat
    /// </summary>
    public class Actstat : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.InputStream.Length > 0)
            {
                StreamReader oSR = new StreamReader(context.Request.InputStream);
                string str = oSR.ReadToEnd();
                oSR.Close();

                // action 1--click, 2--skip, 3--played
                ActionInfo aci = Deserialize(str);
                if (IsValid(aci))
                {
                    aci.time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    aci.ip = FingerPrint.GetVisitorIPAddress();
                    string strSQL = String.Format("insert into dbo.tb_user_stat (id, token, action, time, ip) values ({0}, '{1}', {2}, '{3}', '{4}')", aci.id, aci.token, aci.action, aci.time, aci.ip);

                    string strConn = ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString;
                    try
                    {
                        using (SqlConnection sc = new SqlConnection(strConn))
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                    }
                    catch (Exception ex)
                    {
                        AdssLogger.WriteLog("Exception in insert of Actstat: " + ex.Message);
                    }
                }
            }
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.ContentType = "text/plain";
            context.Response.Write("");
        }

        public bool IsReusable
        {
            get
            {
                //return false;
                return true;
            }
        }

        private ActionInfo Deserialize(string strJson)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                ActionInfo aci = js.Deserialize<ActionInfo>(strJson);
                return aci;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("Actstat Deserialize() Exception: " + e.Message);
                return null;
            }
        }

        bool IsValid(ActionInfo aci)
        {
            if (aci == null || aci.id <= 0 || aci.action <= 0 || string.IsNullOrEmpty(aci.token))
                return false;
            return true;
        }
    }
}