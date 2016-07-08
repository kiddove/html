/*****************************************************************************
Copyright: 
File name: FingerPring.aspx.cs
Description: receive a POST request from a client, insert data into table
Author: paul
Version: 1.0
Date: 04/05/2015
History: 
 * 1. create a interface handles user infomation(fingerprint and others)
 * 2. add interface to get client IP
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
//using System.Linq;
using System.Net;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.ApplicationBlocks.Data;

namespace ADSS
{
    /// <summary>
    /// Summary description for FingerPrint
    /// </summary>
    public class FingerPrint : IHttpHandler
    {
        //private string m_strIp = "";
        public void ProcessRequest(HttpContext context)
        {
            string strIP = GetVisitorIPAddress();
            //m_strIp = "99.237.172.93";
            GeoLocation gl = GetGeoLocation(strIP);
            //m_strIp = GetVisitorIPAddress();
            if (context.Request.InputStream.Length > 0)
            {
                //m_strIp = GetVisitorIPAddress();
                //uploadObj.user_token = result;
                ////uploadObj.user_agent = fp.userAgentKey();
                //uploadObj.user_agent = bi.name + '-' + bi.version;
                //uploadObj.user_language = fp.languageKey();
                //uploadObj.user_color_depth = fp.colorDepthKey();
                //uploadObj.user_screen_resolution = fp.screenResolutionKey();
                //uploadObj.user_time_zone = fp.timezoneOffsetKey();
                //uploadObj.user_platform = fp.platformKey().toString().replace("navigatorPlatform: ", "");
                StreamReader oSR = new StreamReader(context.Request.InputStream);
                string str = oSR.ReadToEnd();
                oSR.Close();

                UserFingerPrint uf = Deserialize(str);
                UserInfo ui = new UserInfo();
                ui.fingerprint = uf;

                ui.fingerprint.country = string.IsNullOrEmpty(gl.country_name) ? "all" : gl.country_name;
                ui.fingerprint.province = string.IsNullOrEmpty(gl.region_name) ? "all" : gl.region_name;
                ui.fingerprint.city = string.IsNullOrEmpty(gl.city) ? "all" : gl.city;
                ui.fingerprint.province_code = string.IsNullOrEmpty(gl.region_code) ? "all" : gl.region_code;
                if (ui != null && !string.IsNullOrEmpty(ui.fingerprint.token))
                {
                    ui.fingerprint.ip = strIP;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        string strConn = ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString;
                        try
                        {
                            using (SqlConnection sc = new SqlConnection(strConn))
                            {
                                InsertUserInfo(sc, ui);
                                ts.Complete();
                            }
                        }
                        catch (Exception ex)
                        {
                            //System.Diagnostics.Trace.WriteLine(ex.Message);
                            AdssLogger.WriteLog("Exception in sql tracsaction: " + ex.Message);
                        }
                    }
                }
            }
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.ContentType = "text/plain";
            context.Response.Write(strIP);
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// method to get Client ip address
        /// </summary>
        /// <param name="GetLan"> set to true if want to get local(LAN) Connected ip address</param>
        /// <returns></returns>
        public static string GetVisitorIPAddress(bool GetLan = false)
        {
            string visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
            {
                GetLan = true;
                visitorIPAddress = string.Empty;
            }

            if (GetLan && string.IsNullOrEmpty(visitorIPAddress))
            {
                //This is for Local(LAN) Connected ID Address
                string stringHostName = Dns.GetHostName();
                //Get Ip Host Entry
                IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
                //Get Ip Address From The Ip Host Entry Address List
                IPAddress[] arrIpAddress = ipHostEntries.AddressList;
                // contains ipv6 and ipv4 or more
                IPAddress[] ipv4 = Array.FindAll(arrIpAddress, a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                IPAddress[] ipv6 = Array.FindAll(arrIpAddress, a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);

                try
                {
                    //visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
                    visitorIPAddress = ipv4[0].ToString();
                }
                catch
                {
                    try
                    {
                        //visitorIPAddress = arrIpAddress[0].ToString();
                        visitorIPAddress = ipv6[0].ToString();
                    }
                    catch
                    {
                        try
                        {
                            arrIpAddress = Dns.GetHostAddresses(stringHostName);
                            visitorIPAddress = arrIpAddress[0].ToString();
                        }
                        catch
                        {
                            visitorIPAddress = "127.0.0.1";
                        }
                    }
                }

            }

            //AdssLogger.WriteLog(m_strPage, m_strIp, "client IP: " + visitorIPAddress);
            return visitorIPAddress;
        }

        private UserFingerPrint Deserialize(string strJson)
        {
            // use JavaScriptSerializer
            // deserialize
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                UserFingerPrint ui = js.Deserialize<UserFingerPrint>(strJson);
                return ui;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("Exception in Deserialize(): " + e.Message);
                return null;
            }
        }

        // insert into tb_user_info
        private void InsertUserInfo(SqlConnection sc, UserInfo ui)
        {
            try
            {
                if (sc != null)
                {
                    // first query, if exist, then update the activate_time
                    string strQuery = String.Format("select id from tb_user_info where token = '{0}' and ip = '{1}'", ui.fingerprint.token, ui.fingerprint.ip);
                    string strSQL = "";
                    using (DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strQuery))
                    {
                        if (dt == null || dt.Tables.Count == 0 || (dt.Tables.Count == 1 && dt.Tables[0].Rows.Count == 0))
                            // insert
                            strSQL = String.Format("insert into tb_user_info (token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, register_time, activate_time, device, os, country, province, city, province_code) values ('{0}', '{1}', '{2}', '{3}', {4}, '{5}', {6}, '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}')",
                                ui.fingerprint.token, ui.fingerprint.ip, ui.fingerprint.agent, ui.fingerprint.language, ui.fingerprint.color_depth, ui.fingerprint.screen_resolution, ui.fingerprint.time_zone, ui.fingerprint.platform, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ui.fingerprint.device, ui.fingerprint.os, ui.fingerprint.country, ui.fingerprint.province, ui.fingerprint.city, ui.fingerprint.province_code);
                        else
                            // update
                            strSQL = String.Format("update tb_user_info set activate_time = '{0}' where token = '{1}' and ip = '{2}'", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ui.fingerprint.token, ui.fingerprint.ip);

                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in InsertUserInfo(): " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("Exception in InsertUserInfo(): " + e.Message);
            }
        }

        public static GeoLocation GetGeoLocation(string strIP)
        {
            GeoLocation gl = GetGeoLocationFromDB(strIP);
            if (string.IsNullOrEmpty(gl.ip) || String.Equals(gl.country_name, "all", StringComparison.OrdinalIgnoreCase)
                || String.Equals(gl.region_name, "all", StringComparison.OrdinalIgnoreCase) || String.Equals(gl.city, "all", StringComparison.OrdinalIgnoreCase))
                gl = GetGeoLocationFromHttp(strIP);
            return gl;
        }
        public static GeoLocation GetGeoLocationFromHttp(string strIP)
        {
            GeoLocation gl = new GeoLocation();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://freegeoip.net/json/" + strIP);
            req.Method = "GET";
            req.KeepAlive = false;
            WebResponse wres = req.GetResponse();
            HttpStatusCode iRet = ((HttpWebResponse)wres).StatusCode;
            if (iRet == HttpStatusCode.OK)
            {
                using (Stream stm = wres.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stm, Encoding.UTF8);
                    string strJson = sr.ReadToEnd();

                    gl = new JavaScriptSerializer().Deserialize<GeoLocation>(strJson);
                }
            }

            if (string.IsNullOrEmpty(gl.ip) || String.Equals(gl.country_name, "all", StringComparison.OrdinalIgnoreCase)
                || String.Equals(gl.region_name, "all", StringComparison.OrdinalIgnoreCase) || String.Equals(gl.city, "all", StringComparison.OrdinalIgnoreCase))
            {
                // try ip api http://ip-api.com/json/114.187.79.6
                GeoLocation gl_ipapi = GetGeoLocationFromIPAPI(strIP);
                if (string.IsNullOrEmpty(gl_ipapi.ip))
                InsertGeoLocation(gl);
            }
            return gl;
        }

        public static GeoLocation GetGeoLocationFromIPAPI(string strIP)
        {
            GeoLocation_IPAPI gl = new GeoLocation_IPAPI();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://ip-api.com/json/" + strIP);
            req.Method = "GET";
            req.KeepAlive = false;
            WebResponse wres = req.GetResponse();
            HttpStatusCode iRet = ((HttpWebResponse)wres).StatusCode;
            if (iRet == HttpStatusCode.OK)
            {
                using (Stream stm = wres.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stm, Encoding.UTF8);
                    string strJson = sr.ReadToEnd();

                    gl = new JavaScriptSerializer().Deserialize<GeoLocation_IPAPI>(strJson);
                }
            }

            if (String.Equals(gl.status, "success", StringComparison.OrdinalIgnoreCase))
            {

            }
            if (string.IsNullOrEmpty(gl.ip) || String.Equals(gl.country_name, "all", StringComparison.OrdinalIgnoreCase)
                || String.Equals(gl.region_name, "all", StringComparison.OrdinalIgnoreCase) || String.Equals(gl.city, "all", StringComparison.OrdinalIgnoreCase))
            {
                // try ip api http://ip-api.com/json/114.187.79.6
                InsertGeoLocation(gl);
            }
            return gl;
        }

        public static GeoLocation GetGeoLocationFromDB(string strIP)
        {
            GeoLocation gl = new GeoLocation();
            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    string strQuery = String.Format("select ip, country, province, city, province_code from dbo.tb_geolocation where ip = '{0}'", strIP);
                    using (DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strQuery))
                    {
                        if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count == 1)
                        {
                            DataRow r = dt.Tables[0].Rows[0];
                            gl.ip = Convert.ToString(r[0]);
                            gl.country_name = Convert.ToString(r[1]);
                            gl.region_name = Convert.ToString(r[2]);
                            gl.city = Convert.ToString(r[3]);
                            gl.region_code = Convert.ToString(r[4]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("Exception in GetGeoLocationFromDB(): " + e.Message);
            }

            return gl;
        }

        public static void InsertGeoLocation(GeoLocation gl)
        {
            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        // xi'an for apostrophe problem
                        gl.city = gl.city.Replace("'", "''");
                        string strSQL = String.Format("insert into tb_geolocation (ip, country, province, city, province_code) values ('{0}', '{1}', '{2}', '{3}', '{4}')", gl.ip, String.IsNullOrEmpty(gl.country_name) ? "all" : gl.country_name, String.IsNullOrEmpty(gl.region_name) ? "all" : gl.region_name, String.IsNullOrEmpty(gl.city) ? "all" : gl.city, String.IsNullOrEmpty(gl.region_code) ? "all" : gl.region_code);
                        SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                    }
                }
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("Exception in InsertGeoLocation(): " + e.Message);
            }
        }
    }// end of class
}