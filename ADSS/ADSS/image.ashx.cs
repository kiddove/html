using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace ADSS
{
    /// <summary>
    /// Summary description for image
    /// </summary>
    public class image : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            // send and receive
            if (context.Request.QueryString["s"] != null)
            {
                if (context.Request.QueryString["g"] != null && context.Request.QueryString["t"] != null
                    && context.Request.QueryString["f"] != null && context.Request.QueryString["d"] != null
                    && context.Request.QueryString["sbj"] != null && context.Request.QueryString["cnt"] != null)
                {
                    string strSQL = null;
                    if (context.Request.QueryString["a"] != null)
                    {
                        try
                        {
                            strSQL = string.Format("insert into tb_email_click_stat (uuid, recipient, sender, distributor, subject, content, alias) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                                context.Request.QueryString["g"], context.Request.QueryString["t"],
                                context.Request.QueryString["f"], context.Request.QueryString["d"],
                                context.Request.QueryString["sbj"], context.Request.QueryString["cnt"],
                                context.Request.QueryString["a"]);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in formating string " + e.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            strSQL = string.Format("insert into tb_email_click_stat (uuid, recipient, sender, distributor, subject, content) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                                context.Request.QueryString["g"], context.Request.QueryString["t"],
                                context.Request.QueryString["f"], context.Request.QueryString["d"],
                                context.Request.QueryString["sbj"], context.Request.QueryString["cnt"]);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in formating string (without alias)" + e.Message);
                        }
                    }

                    if (!string.IsNullOrEmpty(strSQL))
                    {
                        //AdssLogger.WriteLog(strSQL);
                        using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                        {
                            try
                            {
                                SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                            }
                            catch (Exception e)
                            {
                                AdssLogger.WriteLog("Exception in insert into TableEmailStat: " + e.Message + " --- sql: " + strSQL);
                            }
                        }
                    }
                }
            }
            else
            {
                if (context.Request.QueryString["g"] != null)
                {
                    // uuid
                    // receive...
                    // write db, guid, time...
                    // if receive time is null... then update, write a trigger?
                    string strSQL = string.Format("update tb_email_click_stat set receive_time = ISNULL(receive_time, GETDATE()) where uuid = '{0}'", context.Request.QueryString["g"]);
                    using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                    {
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in update TableEmailStat: " + e.Message + " --- sql: " + strSQL);
                        }
                    }
                }

                context.Response.ContentType = "image/png";

                context.Response.OutputStream.Write(ImageFile.Instance.Data, 0, ImageFile.Instance.Data.Length);
                context.Response.OutputStream.Flush();
                context.Response.End();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public class ImageFile
    {
        private static readonly ImageFile _Instance = new ImageFile();
        static ImageFile() { }
        private ImageFile() { }

        public static ImageFile Instance
        {
            get { return _Instance; }
        }

        // The expensive data that this service exposes      
        private byte[] _data = null;

        public byte[] Data
        {
            get
            {
                if (_data == null)
                    loadData();
                return _data;
            }
        }


        private byte[] loadData()
        {
            _data = (byte[])HttpContext.Current.Cache.Get("image");
            if (_data == null)
            {
                // Get the data from our datasource
                //AdssLogger.WriteLog("folder: " + HttpRuntime.AppDomainAppPath);
                _data = File.ReadAllBytes(HttpRuntime.AppDomainAppPath + "1.png");

                // Insert into Cache
                HttpContext.Current.Cache.Insert("image", _data);
            }

            return _data;
        }
    }
}