using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.ApplicationBlocks.Data;

namespace ADSS
{
    /// <summary>
    /// Summary description for GetGlobalSetting
    /// </summary>
    public class GetGlobalSetting : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string strResult = "{}";

            strResult = new JavaScriptSerializer().Serialize(GetGlobalSettingFromDB());

            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.ContentType = "text/plain";
            context.Response.Write(strResult);
        }

        public GlobalSetting GetGlobalSettingFromDB()
        {
            GlobalSetting gs = new GlobalSetting();
            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        string strSQL = String.Format("select time_interval, percentage, camp_on_air, time_to_show_skip from dbo.tb_global_setting");
                        DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        DataTable dtb = dt.Tables[0];

                        if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count == 1)
                        {
                            DataRow r = dt.Tables[0].Rows[0];

                            gs.timeInterval = Convert.ToInt32(r[0]) * 60;
                            gs.percentage = Convert.ToInt32(r[1]);
                            gs.campOnAir = Convert.ToBoolean(r[2]);
                            gs.timeToShowSkip = Convert.ToInt32(r[3]);
                        }
                    }
                    else
                    {
                        //Trace.WriteLine("can not get sql connection.");
                        AdssLogger.WriteLog("GetGlobalSetting.GetGlobalSettingFromDB() --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetGlobalSetting.GetGlobalSettingFromDB() --- Exception: " + e.Message);
            }
            return gs;
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}