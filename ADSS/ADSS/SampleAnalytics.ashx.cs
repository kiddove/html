﻿using System;
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
using System.Net;

namespace ADSS
{
    /// <summary>
    /// Summary description for SampleAnalytics
    /// </summary>
    public class SampleAnalytics : IHttpHandler
    {
        private class TimePair
        {
            public string start { get; set; }
            public string end { get; set; }
        }
        private class TimeInterval
        {
            public TimePair yesterday { get; set; }
            public TimePair last_7_days { get; set; }
            public TimePair last_30_days { get; set; }
            public TimePair this_month { get; set; }
            public TimePair last_month { get; set; }
        }
        private class PostReturn
        {
            public int success { get; set; }
            public string message { get; set; }
            public string key { get; set; }
            public PostReturn()
            {
                success = 0;
                message = key = "";
            }
        }
        //private class Visit
        //{
        //    public int n { get; set; }
        //    public int r { get; set; }
        //}
        private class Visitor
        {
            public int n { get; set; }
            public int r { get; set; }
        }
        private class DefaultInfo
        {
            public string period { get; set; }
            public int visit { get; set; }
            public Visitor visitor { get; set; }
        }
        private class StatItem
        {
            public string country { get; set; }
            public string province { get; set; }
            public string province_code { get; set; }
            public string city { get; set; }
            public string time { get; set; }
            public string ip { get; set; }
        }
        private class AdsStatItem
        {
            public string url { get; set; }
            public string page { get; set; }
            public string time { get; set; }
            public int count { get; set; }
            public string alias { get; set; }
        }
        private class PageViewItem
        {
            public string page { get; set; }
            public int count { get; set; }
            public string percentage { get; set; }
        }
        private class RegionItem : PageViewItem
        {
            public string province { get; set; }
            public string country { get; set; }
        }
        private class TopFiveStat
        {
            public List<PageViewItem> list { get; set; }
            public int total { get; set; }
        }

        private class TopFiveStatRegion
        {
            public List<RegionItem> list { get; set; }
            public int total { get; set; }
        }

        private class PageAnalysisInfo
        {
            //public AnalyticsIdentity identity;
            public string alias { get; set; }
            public int count { get; set; }
            public string language { get; set; }
            public string device { get; set; }
        }
        private class DetailVisitor : StatItem
        {
            public string alias { get; set; }
            public string type { get; set; }
            public string page { get; set; }
            public string refer { get; set; }
            public string video { get; set; }

        }
        private class PageTrackItem
        {
            public string distributor { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public string subject { get; set; }
            public int count { get; set; }
        }
        private class UploadAdsStat
        {
            public string url { get; set; }
            public string page { get; set; }
            public string distributor { get; set; }
            public string blog { get; set; }
            public string token { get; set; }
        }

        private class UploadLeadsStat
        {
            public string reader { get; set; }
            public string page { get; set; }
            public string distributor { get; set; }
            public string blog { get; set; }
            public string token { get; set; }
        }

        private class UploadPageTrack
        {
            public string id { get; set; }
            public string email { get; set; }
            public string distributor { get; set; }
            public string url { get; set; }
            public string title { get; set; }
        }

        private class UploadFingerPrint : UserFingerPrint
        {
            //public string alias { get; set; }   //trigger
            //public string type { get; set; }    // trigger
            public string page { get; set; }
            public string refer { get; set; }
            //public string visit_time { get; set; }  //getDate()
            public string distributor { get; set; }
            public string blog { get; set; }
            public string video { get; set; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // parameters
            // d --- distributor
            // t --- type
            //              "default"   --- count by yesterday, last 7 days, last 30 days, this month, last month, accept a json string as parameter
            //              "dv"        --- detail visitors
            //              "tot"       --- traffic over time
            //              "pv"        --- page view
            //              "to"        --- traffic origin (refer)
            //              "update"    --- update alias        use post..
            //              "as"        --- ads detail stat
            //              "aqs"       --- ads quick stat
            // s --- startDate
            // e --- endDate
            // ti --- timeInterval for default, must be a json string
            // o --- old value for update
            // n --- new value for update
            string strResult = "{}";
            if (context.Request.QueryString["d"] != null)
            {
                string strType = context.Request.QueryString["t"];
                if (strType != null)
                {
                    switch (strType.ToLower())
                    {
                        case "dv":
                            strResult = getDetailVisitor(context.Request.QueryString["d"], context.Request.QueryString["s"], context.Request.QueryString["e"], context.Request.QueryString["b"]);
                            break;
                        case "si":  // single
                            strResult = getSingleUser(Convert.ToString(context.Request.QueryString["a"]), Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"]);
                            break;
                        case "vt":
                            strResult = getVisitorTag(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"]);
                            break;
                        case "tot":
                            strResult = getTrafficOverTime(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"]);
                            break;
                        case "pv":
                            strResult = getPageView(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"]);
                            break;
                        case "to":
                            strResult = getTrafficOrigin(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"]);
                            break;
                        case "as":
                            strResult = getAdsStat(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"], context.Request.QueryString["u"]);
                            break;
                        case "aqs":
                            strResult = getAdsQuickStat(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"]);
                            break;
                        case "tfr":     // traffic from region, only use city
                            strResult = getTrafficFromRegion(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"]);
                            break;
                        case "pt":      // page track
                            strResult = getPageTrack(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]));
                            break;
                        // leads stat to be continued...
                        // 1. page, count
                        // 2. time line
                        default:
                            strResult = getDefaultInfo(context.Request.QueryString["d"], context.Request.QueryString["ti"], context.Request.QueryString["b"]);
                            break;
                    }
                }
                else
                {
                    // default
                    strResult = getDefaultInfo(context.Request.QueryString["d"], context.Request.QueryString["ti"], context.Request.QueryString["b"]);
                }
            }
            else if (context.Request.Form["update"] != null)
            {
                string strType = context.Request.Form["t"];
                if (strType != null)
                {
                    switch (strType.ToLower())
                    {
                        case "a":   // change alias --- mason calle in C#
                            // post
                            strResult = updateAlias(context.Request.Form["o"], context.Request.Form["n"], context.Request.Form["d"]);
                            break;
                        case "u":   // page stat
                            // json string
                            strResult = updateTableFromShowRoom(context.Request.Form["j"]);
                            break;
                        case "b":   // blog stat
                            // json string
                            strResult = updateTableFromBlog(context.Request.Form["j"]);
                            break;
                        case "c":   // ad click
                            strResult = updateTableAds(context.Request.Form["j"]);
                            break;
                        case "l":   // lead stat
                            strResult = updateTableLeads(context.Request.Form["j"]);
                            break;
                        case "t":   // page track --- mason called in C#
                            strResult = updateTableTrack(context.Request.Form["id"], context.Request.Form["email"], context.Request.Form["title"], context.Request.Form["distributor"], context.Request.Form["url"]);
                            break;
                        default:
                            break;
                    }
                }
            }

            context.Response.ContentType = "text/html";
            context.Response.Charset = Encoding.UTF8.WebName;
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.Write(strResult);
        }

        private string getPageTrack(string distributor, string startDate, string endDate)
        {
            // select distributor, email, url, subject, COUNT(uuid) as view_count from tb_page_click_track where distributor = 'kectech' group by uuid, email, distributor, url, subject
            string strSQL = "";
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);

                strSQL = string.Format("select distributor, email, url, subject, COUNT(uuid) as view_count from tb_page_click_track where distributor = '{0}'{1}{2}group by uuid, email, distributor, url, subject", distributor, strClause, strClause2);
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getPageTrack) --- Exception: " + e.Message);
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
                            List<PageTrackItem> ptl = new List<PageTrackItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                PageTrackItem pti = new PageTrackItem();
                                
                                pti.distributor = Convert.ToString(r[0]);
                                pti.email = Convert.ToString(r[1]);
                                string s = Convert.ToString(r[2]);
                                byte[] data = Convert.FromBase64String(s);
                                pti.url = Encoding.UTF8.GetString(data);

                                s = Convert.ToString(r[3]);
                                data = Convert.FromBase64String(s);
                                pti.subject = Encoding.UTF8.GetString(data);

                                pti.count = Convert.ToInt32(r[4]);

                                ptl.Add(pti);
                            }
                            return new JavaScriptSerializer().Serialize(ptl);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getPageTrack sql excute) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }

            return "{}";
        }

        private string updateTableTrack(string id, string email, string title, string distributor, string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    //url & title should be encode
                    byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(url);
                    string url_encode = Convert.ToBase64String(plainTextBytes);
                    plainTextBytes = System.Text.Encoding.UTF8.GetBytes(title);
                    string title_encode = Convert.ToBase64String(plainTextBytes);
                    string strSQL = "";

                    strSQL = string.Format("insert into tb_page_click_track (uuid, email, distributor, url, subject) values ('{0}', '{1}', '{2}', '{3}', '{4}')",
                        id, email, distributor, url_encode, title_encode);

                    using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                    {
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in updateTableLeads(): " + e.Message + " --- sql: " + strSQL);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //System.Diagnostics.Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("Exception --- updateTableTrack(): " + e.Message);
            }

            return "";
        }

        private string updateTableLeads(string strJson)
        {
            // todo 
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                UploadLeadsStat uls = js.Deserialize<UploadLeadsStat>(strJson);
                if (!string.IsNullOrEmpty(uls.reader))
                {
                    string strSQL = "";

                    if (!string.IsNullOrEmpty(uls.blog))
                    {
                        try
                        {
                            strSQL = string.Format("select distributor from tb_blog_to_distributor where blog = '{0}'", uls.blog);
                            using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                            {
                                if (sc != null)
                                {
                                    DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                    if (ds != null && ds.Tables.Count > 0)
                                    {
                                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                        {
                                            DataRow r = ds.Tables[0].Rows[i];
                                            uls.distributor = Convert.ToString(r[0]);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //Trace.WriteLine(e.Message);
                            AdssLogger.WriteLog("GetSampleAnalyticsInfo(updateTableAds) --- Exception: " + e.Message + " --- sql: " + strSQL);
                        }
                    }

                    strSQL = string.Format("insert into tb_page_leads_stat (reader, page, distributor, token) values ('{0}', '{1}', '{2}', '{3}')",
                        uls.reader, uls.page, string.IsNullOrEmpty(uls.distributor) ? uls.blog : uls.distributor, uls.token);

                    using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                    {
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in updateTableLeads(): " + e.Message + " --- sql: " + strSQL);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //System.Diagnostics.Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("Exception in Deserialize(): " + e.Message);
            }

            return "";
        }

        private string getTrafficFromRegion(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
                //strSQL = string.Format("select top 10 city, COUNT(*) as visit, COUNT(*) * 1.0 / SUM(COUNT(*)) over() as percentage, SUM(COUNT(*)) over() as total, province, country from tb_page_visit_info_xango where distributor='{0}'{1}{2} group by city, province, country order by percentage desc", distributor, strClause, strClause2);
                strSQL = string.Format("select top 10 f.city, COUNT(*) as visit, COUNT(*) * 1.0 / SUM(COUNT(*)) over() as percentage, SUM(COUNT(*)) over() as total, f.province, f.country from (select b.city, b.province, b.country from tb_page_visit_info_xango as a , tb_geolocation as b where a.ip = b.ip and a.distributor = '{0}'{1}{2}) as f group by f.city, f.province, f.country order by percentage desc", distributor, strClause, strClause2);
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrafficFromRegion) --- Exception: " + e.Message);
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
                            TopFiveStatRegion tfs = new TopFiveStatRegion();
                            tfs.list = new List<RegionItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                RegionItem pv = new RegionItem();
                                pv.page = Convert.ToString(r[0]);
                                pv.count = Convert.ToInt32(r[1]);
                                pv.percentage = string.Format("{0:P1}", Convert.ToSingle(r[2]));
                                pv.province = Convert.ToString(r[4]);
                                pv.country = Convert.ToString(r[5]);
                                tfs.list.Add(pv);
                                tfs.total = Convert.ToInt32(r[3]);
                            }
                            return new JavaScriptSerializer().Serialize(tfs);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrafficFromRegion) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }

            return "{}";
        }

        private string getAdsQuickStat(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            string strSQL = "";
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, click_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, click_time) <= '{0}'", endDate);

                //strSQL = string.Format("select url, page, count(*) as c from tb_page_ads_click_stat where distributor='{0}'{1}{2} group by distributor, url, page order by c desc", distributor, strClause, strClause2);
                strSQL = string.Format("select url, count(*) as c from tb_page_ads_click_stat where distributor='{0}'{1}{2} group by distributor, url order by c desc", distributor, strClause, strClause2);
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getAdsQuickStat) --- Exception: " + e.Message);
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
                            List<AdsStatItem> aList = new List<AdsStatItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                AdsStatItem ai = new AdsStatItem();
                                ai.url = Convert.ToString(r[0]);
                                //ai.page = Convert.ToString(r[1]);
                                ai.count = Convert.ToInt32(r[1]);
                                aList.Add(ai);
                            }
                            return new JavaScriptSerializer().Serialize(aList);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getAdsQuickStat) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }

            return "{}";
        }

        private string getAdsStat(string distributor, string startDate, string endDate, string blog, string url)
        {
            combineBlogDistributor(blog, distributor);
            string strSQL = "";
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, a.click_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, a.click_time) <= '{0}'", endDate);
                string strClause3 = "";
                if (!string.IsNullOrEmpty(url))
                {
                    byte[] data = Convert.FromBase64String(url);
                    string decodedString = Encoding.UTF8.GetString(data);
                    strClause3 = string.Format(" and url = '{0}'", decodedString);
                }

                strSQL = string.Format("select a.url, a.page, a.click_time, b.alias from tb_page_visit_info_xango as b, tb_page_ads_click_stat as a where a.distributor='{0}'{1}{2}{3} and a.distributor = b.distributor and a.token = b.token group by a.url, a.page, a.click_time, b.alias order by a.click_time desc", distributor, strClause, strClause2, strClause3);
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getAdsStat) --- Exception: " + e.Message);
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
                            List<AdsStatItem> aList = new List<AdsStatItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                AdsStatItem ai = new AdsStatItem();
                                ai.url = Convert.ToString(r[0]);
                                ai.page = Convert.ToString(r[1]);
                                ai.time = ((DateTime)r[2]).ToString("yyyy-MM-dd HH:mm:ss");
                                ai.alias = Convert.ToString(r[3]);
                                aList.Add(ai);
                            }
                            return new JavaScriptSerializer().Serialize(aList);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getAdsStat) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }

            return "{}";
        }

        private string updateTableAds(string strJson)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                UploadAdsStat uas = js.Deserialize<UploadAdsStat>(strJson);
                if (!string.IsNullOrEmpty(uas.url))
                {
                    string strSQL = "";

                    if (!string.IsNullOrEmpty(uas.blog))
                    {
                        try
                        {
                            strSQL = string.Format("select distributor from tb_blog_to_distributor where blog = '{0}'", uas.blog);
                            using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                            {
                                if (sc != null)
                                {
                                    DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                    if (ds != null && ds.Tables.Count > 0)
                                    {
                                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                        {
                                            DataRow r = ds.Tables[0].Rows[i];
                                            uas.distributor = Convert.ToString(r[0]);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //Trace.WriteLine(e.Message);
                            AdssLogger.WriteLog("GetSampleAnalyticsInfo(updateTableAds) --- Exception: " + e.Message + " --- sql: " + strSQL);
                        }
                    }


                    strSQL = string.Format("insert into tb_page_ads_click_stat (url, page, distributor, token) values ('{0}', '{1}', '{2}', '{3}')",
                        uas.url, uas.page, string.IsNullOrEmpty(uas.distributor) ? uas.blog : uas.distributor, uas.token);

                    using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                    {
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in updateTableAds(): " + e.Message + " --- sql: " + strSQL);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //System.Diagnostics.Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("Exception in Deserialize(): " + e.Message);
            }

            return "";
        }

        private string updateTableFromShowRoom(string strJson)
        {
            string strIP = FingerPrint.GetVisitorIPAddress();
            GeoLocation gl = FingerPrint.GetGeoLocation(strIP);
            if (!string.IsNullOrEmpty(gl.ip))
            {
                try
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    UploadFingerPrint uf = js.Deserialize<UploadFingerPrint>(strJson);
                    uf.country = string.IsNullOrEmpty(gl.country_name) ? "all" : gl.country_name;
                    uf.province = string.IsNullOrEmpty(gl.region_name) ? "all" : gl.region_name;
                    uf.city = string.IsNullOrEmpty(gl.city) ? "all" : gl.city;
                    uf.province_code = string.IsNullOrEmpty(gl.region_code) ? "" : gl.region_code;
                    uf.ip = gl.ip;
                    // visit_time, alias, type
                    string strSQL;

                    strSQL = string.Format("insert into tb_page_visit_info_xango (token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, country, province, city, province_code, refer, page, distributor, video) values ('{0}', '{1}', '{2}', '{3}', {4}, '{5}', {6}, '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}')",
                        uf.token, uf.ip, uf.agent, uf.language, uf.color_depth, uf.screen_resolution, uf.time_zone, uf.platform, uf.device, uf.os, uf.country, uf.province, uf.city, uf.province_code, uf.refer, uf.page, uf.distributor, uf.video);

                    using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                    {
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in updateTableFromShowRoom(): " + e.Message + " --- sql: " + strSQL);
                        }
                    }
                }
                catch (Exception e)
                {
                    //System.Diagnostics.Trace.WriteLine(e.Message);
                    AdssLogger.WriteLog("Exception in Deserialize(): " + e.Message + " --- json: " + strJson);
                }


            }
            return "";
        }

        private string updateTableFromBlog(string strJson)
        {
            string strIP = FingerPrint.GetVisitorIPAddress();
            GeoLocation gl = FingerPrint.GetGeoLocation(strIP);
            if (!string.IsNullOrEmpty(gl.ip))
            {
                try
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    UploadFingerPrint uf = js.Deserialize<UploadFingerPrint>(strJson);
                    uf.country = string.IsNullOrEmpty(gl.country_name) ? "all" : gl.country_name;
                    uf.province = string.IsNullOrEmpty(gl.region_name) ? "all" : gl.region_name;
                    uf.city = string.IsNullOrEmpty(gl.city) ? "all" : gl.city;
                    uf.province_code = string.IsNullOrEmpty(gl.region_code) ? "" : gl.region_code;
                    uf.ip = gl.ip;
                    // visit_time, alias, type
                    string strSQL = "";

                    if (!string.IsNullOrEmpty(uf.blog))
                    {
                        try
                        {
                            strSQL = string.Format("select distributor from tb_blog_to_distributor where blog = '{0}'", uf.blog);
                            using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                            {
                                if (sc != null)
                                {
                                    DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                    if (ds != null && ds.Tables.Count > 0)
                                    {
                                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                        {
                                            DataRow r = ds.Tables[0].Rows[i];
                                            uf.distributor = Convert.ToString(r[0]);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //Trace.WriteLine(e.Message);
                            AdssLogger.WriteLog("GetSampleAnalyticsInfo(updateTableFromBlog) --- Exception: " + e.Message + " --- sql: " + strSQL);
                        }

                        strSQL = string.Format("insert into tb_page_visit_info_xango (token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, country, province, city, province_code, refer, page, distributor) values ('{0}', '{1}', '{2}', '{3}', {4}, '{5}', {6}, '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}')",
                            uf.token, uf.ip, uf.agent, uf.language, uf.color_depth, uf.screen_resolution, uf.time_zone, uf.platform, uf.device, uf.os, uf.country, uf.province, uf.city, uf.province_code, uf.refer, uf.page, string.IsNullOrEmpty(uf.distributor) ? uf.blog : uf.distributor);

                        using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                        {
                            try
                            {
                                SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                            }
                            catch (Exception e)
                            {
                                AdssLogger.WriteLog("Exception in updateTableFromBlog(): " + e.Message + " --- sql: " + strSQL);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //System.Diagnostics.Trace.WriteLine(e.Message);
                    AdssLogger.WriteLog("Exception in Deserialize(): " + e.Message + " --- json: " + strJson);
                }


            }
            return "";
        }

        private string getTrafficOrigin(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            // select distinct(refer), COUNT(id) as visit from tb_page_visit_info_xango where distributor='paul' and visit_time >= '2016-05-05' and visit_time <= '2016-06-03' group by refer order by visit desc
            string strSQL = "";
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
                strSQL = string.Format("select top 5 refer, COUNT(*) as visit, COUNT(*) * 1.0/ SUM(COUNT(*)) over() as percentage, SUM(COUNT(*)) over() as total from tb_page_visit_info_xango where distributor='{0}'{1}{2} group by refer order by percentage desc", distributor, strClause, strClause2);
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrfficOrigin) --- Exception: " + e.Message);
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
                            TopFiveStat tfs = new TopFiveStat();
                            tfs.list = new List<PageViewItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                PageViewItem pv = new PageViewItem();
                                pv.page = Convert.ToString(r[0]);
                                pv.count = Convert.ToInt32(r[1]);
                                pv.percentage = string.Format("{0:P1}", Convert.ToSingle(r[2]));
                                tfs.list.Add(pv);
                                tfs.total = Convert.ToInt32(r[3]);
                            }
                            return new JavaScriptSerializer().Serialize(tfs);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrfficOrigin) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }

            return "{}";
        }

        private string getPageView(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            // select distinct(page), COUNT(id) as visit from tb_page_visit_info_xango where distributor='paul' and visit_time >= '2016-05-05' and visit_time <= '2016-06-03' group by page order by visit desc
            // select page, COUNT(*) as count, COUNT(*) * 100.0 / SUM(COUNT(*)) over() as percentage from tb_page_visit_info_xango where distributor='paul' and visit_time >= '2016-05-05' and visit_time <= '2016-06-03' group by page order by percentage desc
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
                strSQL = string.Format("select top 10 page, COUNT(*) as visit, COUNT(*) * 1.0 / SUM(COUNT(*)) over() as percentage, SUM(COUNT(*)) over() as total from tb_page_visit_info_xango where distributor='{0}'{1}{2} group by page order by percentage desc", distributor, strClause, strClause2);
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getPageView) --- Exception: " + e.Message);
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
                            TopFiveStat tfs = new TopFiveStat();
                            tfs.list = new List<PageViewItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                PageViewItem pv = new PageViewItem();
                                pv.page = Convert.ToString(r[0]);
                                pv.count = Convert.ToInt32(r[1]);
                                pv.percentage = string.Format("{0:P1}", Convert.ToSingle(r[2]));
                                tfs.list.Add(pv);
                                tfs.total = Convert.ToInt32(r[3]);
                            }
                            return new JavaScriptSerializer().Serialize(tfs);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getPageView) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }

            return "{}";
        }

        //private string getTrafficOverTime(string distributor, string startDate, string endDate, string blog)
        //{
        //    combineBlogDistributor(blog, distributor);
        //    // select CAST(visit_time as DATE), COUNT(id) from tb_page_visit_info_xango where visit_time <= '2016-05-05' group by CAST(visit_time as DATE)
        //    // select t3.d, t3.t, COUNT(t4.type)as c from tb_page_visit_info_xango t4 right join (select * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where visit_time < '2016-01-01') t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 ) t3 on CAST(t4.visit_time as DATE) = t3.d and t4.type = t3.t group by t3.d, t3.t order by t3.d, t3.t
        //    string strSQL;
        //    try
        //    {
        //        string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
        //        string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
        //        //string strHead = "select DATEDIFF(SECOND,{d '1970-01-01'}, t3.d), t3.t, COUNT(t4.type)as c ";
        //        string strHead = "select t3.d, t3.t, COUNT(distinct t4.token)as visitor, COUNT(t4.type) as pageview ";
        //        strSQL = strHead + string.Format("from tb_page_visit_info_xango t4 right join (select '{3}' as dis, * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where distributor='{0}'{1}{2}) t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 ) t3 on CAST(t4.visit_time as DATE) = t3.d and t4.type = t3.t and t4.distributor = '{0}' group by t3.d, t3.t order by t3.d, t3.t", distributor, strClause, strClause2, distributor);
        //    }
        //    catch (Exception e)
        //    {
        //        AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrafficOverTime) --- Exception: " + e.Message);
        //        return "{}";
        //    }
        //    try
        //    {
        //        using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
        //        {
        //            if (sc != null)
        //            {
        //                DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
        //                if (ds != null && ds.Tables.Count > 0)
        //                {
        //                    List<DefaultInfo> dInfo = new List<DefaultInfo>();
        //                    DefaultInfo di = null;
        //                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //                    {
        //                        // d,t,visitor,pageview(visit)
        //                        DataRow r = ds.Tables[0].Rows[i];
        //                        if (i % 2 == 0)
        //                        {
        //                            di = new DefaultInfo();
        //                            di.visit = new Visit();
        //                            di.visitor = new Visitor();
        //                        }

        //                        //di.period = Convert.ToString(r[0]);
        //                        di.period = ((DateTime)r[0]).ToString("yyyy-MM-dd");
        //                        if (i % 2 == 0)
        //                        {
        //                            di.visit.n = Convert.ToInt32(r[3]);
        //                            di.visitor.n = Convert.ToInt32(r[2]);
        //                        }
        //                        else
        //                        {
        //                            di.visit.r = Convert.ToInt32(r[3]);
        //                            di.visitor.r = Convert.ToInt32(r[2]);
        //                            dInfo.Add(di);
        //                        }
        //                    }
        //                    return new JavaScriptSerializer().Serialize(dInfo);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //Trace.WriteLine(e.Message);
        //        AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrafficOverTime sql excute) --- Exception: " + e.Message + " --- sql: " + strSQL);
        //    }
        //    return "{}";
        //}

        private string getTrafficOverTime(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);

                strSQL = string.Format("select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango where distributor='{0}'{1}{2} order by d", distributor, strClause, strClause2);
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrafficOverTime) --- Exception: " + e.Message);
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
                            List<DefaultInfo> dInfo = new List<DefaultInfo>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];

                                DefaultInfo di = new DefaultInfo();
                                di.visitor = new Visitor();

                                di.period = ((DateTime)r[0]).ToString("yyyy-MM-dd");

                                string strSQL1 = string.Format("select COUNT(distinct token) as cnt from tb_page_visit_info_xango where distributor = '{0}' and type='new' and convert(date, visit_time) = '{1}'", distributor, di.period);
                                DataSet ds1 = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL1);
                                if (ds1 != null && ds1.Tables.Count > 0)
                                {
                                    DataRow r1 = ds1.Tables[0].Rows[0];
                                    di.visitor.n = Convert.ToInt32(r1[0]);
                                    string strSQL2 = string.Format("select COUNT(distinct token) as cnt, COUNT(*) as cnt_v from tb_page_visit_info_xango where distributor = '{0}' and convert(date, visit_time) = '{1}'", distributor, di.period);
                                    DataSet ds2 = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL2);
                                    if (ds2 != null && ds1.Tables.Count > 0)
                                    {
                                        DataRow r2 = ds2.Tables[0].Rows[0];
                                        di.visitor.r = Convert.ToInt32(r2[0]) - di.visitor.n;
                                        di.visit = Convert.ToInt32(r2[1]);
                                    }
                                }
                                dInfo.Add(di);
                            }
                            return new JavaScriptSerializer().Serialize(dInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrafficOverTime sql excute) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }
            return "{}";
        }

        private string getDefaultInfo(string distributor, string strJson, string blog)
        {
            combineBlogDistributor(blog, distributor);
            if (String.IsNullOrEmpty(distributor) || String.IsNullOrEmpty(strJson))
                return "{}";
            else
            {
                string strSQL = "";
                try
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    TimeInterval ti = js.Deserialize<TimeInterval>(strJson);

                    List<string> labels = new List<string>();
                    labels.Add("Yesterday");
                    labels.Add("Last 7 days");
                    labels.Add("Last 30 days");
                    labels.Add("This Month");
                    labels.Add("Last Month");
                    if (ti != null)
                    {
                        string strSQL1 = string.Format("select '{2}' as period, type, COUNT(distinct token) as cnt, COUNT(type) as cnt_v from tb_page_visit_info_xango where distributor = '{3}' and convert(date, visit_time) >= '{0}' and convert(date, visit_time) < '{1}' group by type;", ti.yesterday.start, ti.yesterday.end, "Yesterday", distributor);
                        string strSQL2 = string.Format("select '{2}' as period, type, COUNT(distinct token) as cnt, COUNT(type) as cnt_v from tb_page_visit_info_xango where distributor = '{3}' and convert(date, visit_time) >= '{0}' and convert(date, visit_time) < '{1}' group by type;", ti.last_7_days.start, ti.last_7_days.end, "Last 7 days", distributor);
                        string strSQL3 = string.Format("select '{2}' as period, type, COUNT(distinct token) as cnt, COUNT(type) as cnt_v from tb_page_visit_info_xango where distributor = '{3}' and convert(date, visit_time) >= '{0}' and convert(date, visit_time) < '{1}' group by type;", ti.last_30_days.start, ti.last_7_days.end, "Last 30 days", distributor);
                        string strSQL4 = string.Format("select '{2}' as period, type, COUNT(distinct token) as cnt, COUNT(type) as cnt_v from tb_page_visit_info_xango where distributor = '{3}' and convert(date, visit_time) >= '{0}' and convert(date, visit_time) <= '{1}' group by type;", ti.this_month.start, ti.this_month.end, "This month", distributor);
                        string strSQL5 = string.Format("select '{2}' as period, type, COUNT(distinct token) as cnt, COUNT(type) as cnt_v from tb_page_visit_info_xango where distributor = '{3}' and convert(date, visit_time) >= '{0}' and convert(date, visit_time) <= '{1}' group by type;", ti.last_month.start, ti.last_month.end, "Last month", distributor);

                        strSQL = strSQL1 + strSQL2 + strSQL3 + strSQL4 + strSQL5;

                        List<string> times = new List<string>();
                        times.Add(string.Format(" and convert(date, visit_time) >= '{0}' and convert(date, visit_time) < '{1}';", ti.yesterday.start, ti.yesterday.end));
                        times.Add(string.Format(" and convert(date, visit_time) >= '{0}' and convert(date, visit_time) < '{1}';", ti.last_7_days.start, ti.last_7_days.end));
                        times.Add(string.Format(" and convert(date, visit_time) >= '{0}' and convert(date, visit_time) < '{1}';", ti.last_30_days.start, ti.last_7_days.end));
                        times.Add(string.Format(" and convert(date, visit_time) >= '{0}' and convert(date, visit_time) < '{1}';", ti.this_month.start, ti.this_month.end));
                        times.Add(string.Format(" and convert(date, visit_time) >= '{0}' and convert(date, visit_time) < '{1}';", ti.last_month.start, ti.last_month.end));
                        using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                        {
                            if (sc != null)
                            {
                                // select count(new)
                                // select count(*)
                                // do minus --- return
                                // because return can be new as well
                                List<DefaultInfo> dInfo = new List<DefaultInfo>();
                                for (int i = 0; i < times.Count; i++)
                                {
                                    DefaultInfo di = new DefaultInfo();
                                    di.visitor = new Visitor();

                                    strSQL = string.Format("select COUNT(distinct token) as cnt from tb_page_visit_info_xango where distributor = '{0}' and type='new'{1}", distributor, times[i]);
                                    DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                    {
                                        DataRow r = ds.Tables[0].Rows[0];
                                        di.period = labels[i];
                                        di.visitor.n = Convert.ToInt32(r[0]);
                                    }

                                    strSQL = string.Format("select COUNT(distinct token) as cnt, COUNT(*) as cnt_v from tb_page_visit_info_xango where distributor = '{0}'{1}", distributor, times[i]);
                                    ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                    {
                                        DataRow r = ds.Tables[0].Rows[0];
                                        di.period = labels[i];
                                        di.visitor.r = Convert.ToInt32(r[0]) - di.visitor.n;
                                        di.visit = Convert.ToInt32(r[1]);
                                    }
                                    dInfo.Add(di);
                                }

                                return new JavaScriptSerializer().Serialize(dInfo);

                                //DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                //if (ds != null && ds.Tables.Count > 0)
                                //{
                                //    List<DefaultInfo> dInfo = new List<DefaultInfo>();
                                //    for (int i = 0; i < ds.Tables.Count; i++)
                                //    {
                                //        DefaultInfo di = new DefaultInfo();
                                //        di.visit = new Visit();
                                //        di.visitor = new Visitor();
                                //        if (ds.Tables[i].Rows.Count == 0)
                                //        {
                                //            di.period = labels[i];
                                //            di.visit.n = di.visit.r = 0;
                                //            di.visitor.n = di.visitor.r = 0;
                                //        }
                                //        else
                                //        {
                                //            for (int j = 0; j < ds.Tables[i].Rows.Count; j++)
                                //            {
                                //                DataRow r = ds.Tables[i].Rows[j];
                                //                di.period = Convert.ToString(r[0]);
                                //                if (String.Compare("new", Convert.ToString(r[1]), StringComparison.OrdinalIgnoreCase) == 0)
                                //                {
                                //                    di.visitor.n = Convert.ToInt32(r[2]);
                                //                    di.visit.n = Convert.ToInt32(r[3]);
                                //                }
                                //                else if (String.Compare("return", Convert.ToString(r[1]), StringComparison.OrdinalIgnoreCase) == 0)
                                //                {
                                //                    di.visitor.r = Convert.ToInt32(r[2]);
                                //                    di.visit.r = Convert.ToInt32(r[3]);
                                //                }
                                //            }
                                //        }
                                //        dInfo.Add(di);
                                //    }

                                //    return new JavaScriptSerializer().Serialize(dInfo);
                                //}
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AdssLogger.WriteLog("getDefaultInfo Exception: " + ex.Message + " --- sql: " + strSQL);
                }
            }
            return "{}";
        }

        private string getDetailVisitor(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            // select alias, ip, visit_time, type, page, refer, country, province, city from tb_page_visit_info_xango where visit_time >= '2014-06-01' and visit_time <= '2016-06-02' and distributor = 'paul'
            string strSQL = "";
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);


                //string strMain = "select alias, ip, DATEDIFF(SECOND,{d '1970-01-01'}, visit_time) as time, type, page, refer, country, province, city from tb_page_visit_info_xango where distributor = '" + distributor + "'";
                string strMain = "select a.alias, b.ip, a.visit_time, a.type, a.page, a.refer, b.country, b.province, b.city, b.province_code, a.video from tb_page_visit_info_xango as a, tb_geolocation as b where a.distributor = '" + distributor + "' and a.ip = b.ip";
                //group by token, alias, country, province, city, language, device", startDate, endDate, strDistributor);
                string strOrder = " order by a.visit_time desc";
                strSQL = strMain + strClause + strClause2 + strOrder;
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getDetailVisitor) --- Exception: " + e.Message);
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
                            List<DetailVisitor> dl = new List<DetailVisitor>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                DetailVisitor dv = new DetailVisitor();
                                dv.alias = Convert.ToString(r[0]);
                                dv.ip = Convert.ToString(r[1]);
                                //dv.time = Convert.ToString(r[2]);
                                dv.time = ((DateTime)r[2]).ToString("yyyy-MM-dd HH:mm:ss");
                                dv.type = Convert.ToString(r[3]);
                                dv.page = Convert.ToString(r[4]);
                                dv.refer = Convert.ToString(r[5]);
                                dv.country = Convert.ToString(r[6]);
                                dv.province = Convert.ToString(r[7]);
                                dv.city = Convert.ToString(r[8]);
                                dv.province_code = Convert.ToString(r[9]);
                                dv.video = Convert.ToString(r[10]);
                                if (String.Equals(dv.country, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    dv.country = "";
                                }
                                if (String.Equals(dv.province, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    dv.province = "";
                                }
                                if (String.Equals(dv.city, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    dv.city = "";
                                }
                                dl.Add(dv);
                            }
                            return new JavaScriptSerializer().Serialize(dl);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getDetailInfo sql excute) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }

            return "{}";
        }

        private void combineBlogDistributor(string blog, string distributor)
        {
            if (string.IsNullOrEmpty(blog) || string.IsNullOrEmpty(distributor))
                return;
            if (string.Compare("undefined", blog, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare("undefined", distributor, StringComparison.OrdinalIgnoreCase) == 0)
                return;
            string strSQL;
            try
            {
                strSQL = string.Format("select distributor from tb_blog_to_distributor where blog = '{0}'", blog);
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                                return;
                        }
                    }
                }

                // insert 
                strSQL = string.Format("insert into tb_blog_to_distributor values ('{0}', '{1}')", blog, distributor);

                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    try
                    {
                        SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                    }
                    catch (Exception e)
                    {
                        AdssLogger.WriteLog("Exception in update tb_blog_to_distributor: " + e.Message + " --- sql: " + strSQL);
                    }
                }

                // after this all stat from blog will be correct one with distributor 
                // may have 2 new...


                // update one by one
                strSQL = string.Format("select distinct token, alias from tb_page_visit_info_xango where distributor = '{0}'", distributor);
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                string token = Convert.ToString(r[0]);
                                string alias = Convert.ToString(r[1]);
                                // for each token 

                                strSQL = string.Format("select id from tb_page_visit_info_xango where distributor = '{0}' and type = 'new' and token = '{1}'", distributor, token);
                                int iDistributor = 0;
                                int iBlog = 0;
                                DataSet ds1 = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                if (ds1 != null && ds1.Tables.Count > 0)
                                {
                                    for (int i1 = 0; i1 < ds1.Tables[0].Rows.Count; i1++)
                                    {
                                        DataRow r1 = ds1.Tables[0].Rows[i1];
                                        iDistributor = Convert.ToInt32(r1[0]);
                                    }
                                }

                                strSQL = string.Format("select id from tb_page_visit_info_xango where distributor = '{0}' and type = 'new' and token = '{1}'", blog, token);
                                ds1 = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                if (ds1 != null && ds1.Tables.Count > 0)
                                {
                                    for (int i1 = 0; i1 < ds1.Tables[0].Rows.Count; i1++)
                                    {
                                        DataRow r1 = ds1.Tables[0].Rows[i1];
                                        iBlog = Convert.ToInt32(r1[0]);
                                    }
                                }

                                if (iDistributor > 0 && iBlog > 0 && iDistributor != iBlog)
                                {
                                    if (iDistributor < iBlog)
                                    {
                                        strSQL = string.Format("update tb_page_visit_info_xango set type = 'return' where id = {0}", iBlog);
                                    }
                                    else
                                    {
                                        strSQL = string.Format("update tb_page_visit_info_xango set type = 'return' where id = {0}", iDistributor);
                                    }
                                    try
                                    {
                                        SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                                    }
                                    catch (Exception e)
                                    {
                                        AdssLogger.WriteLog("Exception in update tb_page_visit_info_xango: " + e.Message + " --- sql: " + strSQL);
                                    }
                                }



                                string updateSQL = string.Format("update tb_page_visit_info_xango set alias = '{0}' where token = '{1}' and distributor = '{2}'", alias, token, blog);
                                try
                                {
                                    SqlHelper.ExecuteNonQuery(sc, CommandType.Text, updateSQL);
                                }
                                catch (Exception e)
                                {
                                    AdssLogger.WriteLog("Exception in update tb_page_visit_info_xango: " + e.Message + " --- sql: " + updateSQL);
                                }
                            }
                        }

                        // change alias
                        strSQL = string.Format("update tb_page_visit_info_xango set distributor = '{0}' where distributor = '{1}'", distributor, blog);
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in update tb_page_visit_info_xango: " + e.Message + " --- sql: " + strSQL);
                        }

                        // change distributor in ads_stat
                        strSQL = string.Format("update tb_page_ads_click_stat set distributor = '{0}' where distributor = '{1}'", distributor, blog);
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in update tb_page_visit_info_xango: " + e.Message + " --- sql: " + strSQL);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(combineBlogDistributor) --- Exception: " + e.Message);
            }

            // 2. update one by one....
        }

        private string getSingleUser(string alias, string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            string strSQL = "";
            try
            {
                //string strToken = string.Format("'{0}' order by s.id, s.time", token);
                //strSQL = "select s.id, s.action, DATEDIFF(SECOND,{d '1970-01-01'}, s.time) as time, s.ip from tb_user_info as i, tb_user_stat as s where i.token = s.token and s.valid = 1 and i.ip = s.ip and s.token = " + strToken;
                //// what is the matter with this? {d '1970-01-01'}??? as {0}, {1}?? maybe
                ////strSQL = string.Format("select s.id, s.token, s.action, DATEDIFF(SECOND,{d '1970-01-01'}, s.time), s.ip from tb_user_info as i, tb_user_stat as s where i.token = s.token and s.valid = 1 and i.ip = s.ip and s.token = '{0}' order by s.id, s.time", token);
                //// DATEDIFF(SECOND,{d '1970-01-01'}, s.time) as timestamp

                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);


                //string strMain = "select ip, country, province, city, DATEDIFF(SECOND,{d '1970-01-01'}, visit_time) as time from tb_page_visit_info_xango where distributor = '" + distributor + "' and alias='" + alias + "'";
                string strMain = "select a.alias, b.ip, a.visit_time, a.type, a.page, a.refer, b.country, b.province, b.city, b.province_code, a.video from tb_page_visit_info_xango as a, tb_geolocation as b where a.distributor = '" + distributor + "' and alias='" + alias + "' and a.ip=b.ip";
                //group by token, alias, country, province, city, language, device", startDate, endDate, strDistributor);
                string strOrder = " order by a.visit_time desc";
                strSQL = strMain + strClause + strClause2 + strOrder;
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getSingleUser) --- Exception: " + e.Message);
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
                            List<DetailVisitor> dl = new List<DetailVisitor>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                DetailVisitor dv = new DetailVisitor();
                                dv.alias = Convert.ToString(r[0]);
                                dv.ip = Convert.ToString(r[1]);
                                //dv.time = Convert.ToString(r[2]);
                                dv.time = ((DateTime)r[2]).ToString("yyyy-MM-dd HH:mm:ss");
                                dv.type = Convert.ToString(r[3]);
                                dv.page = Convert.ToString(r[4]);
                                dv.refer = Convert.ToString(r[5]);
                                dv.country = Convert.ToString(r[6]);
                                dv.province = Convert.ToString(r[7]);
                                dv.city = Convert.ToString(r[8]);
                                dv.province_code = Convert.ToString(r[9]);
                                dv.video = Convert.ToString(r[10]);
                                if (String.Equals(dv.country, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    dv.country = "";
                                }
                                if (String.Equals(dv.province, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    dv.province = "";
                                }
                                if (String.Equals(dv.city, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    dv.city = "";
                                }
                                dl.Add(dv);
                            }
                            return new JavaScriptSerializer().Serialize(dl);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getSingleUser sql excute) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }
            return "{}";
        }

        private string getVisitorTag(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
            string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
            string strMain = string.Format("select distinct alias, COUNT(alias)as count, language, device from tb_page_visit_info_xango where distributor = '{0}'", distributor);
            //group by token, alias, country, province, city, language, device", startDate, endDate, strDistributor);
            string strRear = " group by alias, language, device";

            string strSQL = strMain + strClause + strClause2 + strRear;
            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            List<PageAnalysisInfo> uInfo = new List<PageAnalysisInfo>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                PageAnalysisInfo fp = new PageAnalysisInfo();
                                //fp.identity = new AnalyticsIdentity();
                                //fp.identity.token = Convert.ToString(r[0]);
                                fp.alias = Convert.ToString(r[0]);
                                fp.count = Convert.ToInt32(r[1]);
                                //fp.country = Convert.ToString(r[3]);
                                //fp.province = Convert.ToString(r[4]);
                                //fp.city = Convert.ToString(r[5]);
                                fp.language = Convert.ToString(r[2]);
                                fp.device = Convert.ToString(r[3]);
                                //if (String.Equals(fp.country, "all", StringComparison.OrdinalIgnoreCase))
                                //{
                                //    fp.country = "";
                                //}
                                //if (String.Equals(fp.province, "all", StringComparison.OrdinalIgnoreCase))
                                //{
                                //    fp.province = "";
                                //}
                                //if (String.Equals(fp.city, "all", StringComparison.OrdinalIgnoreCase))
                                //{
                                //    fp.city = "";
                                //}
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
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getResult) --- Exception: " + e.Message + " --- sql: " + strSQL);
            }
            return "{}";
        }

        private string updateAlias(string oldValue, string newValue, string distributor)
        {
            PostReturn pr = new PostReturn();
            pr.key = oldValue;
            string strSQL = string.Format("select * from tb_page_visit_info_xango where alias = '{0}' and distributor='{1}'", newValue, distributor);
            //string strSQL = "select * from tb_page_visit_info_xango where alias = '" + newValue + "' and distributor='" + distributor + "'";
            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                pr.message = string.Format("Alias already exists. Please enter another one.");
                                //return strRet;
                            }
                            else
                            {
                                try
                                {
                                    strSQL = string.Format("update tb_page_visit_info_xango set alias = '{0}' where alias = '{1}' and distributor='{2}'", newValue, oldValue, distributor);
                                    SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                                    pr.success = 1;
                                }
                                catch (Exception e)
                                {
                                    AdssLogger.WriteLog("Exception in updateAlias(): " + e.Message);
                                    pr.message = "Operation failed.";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getResult) --- Exception: " + e.Message + " --- sql: " + strSQL);
                pr.message = "Operation failed.";
            }

            return new JavaScriptSerializer().Serialize(pr);
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