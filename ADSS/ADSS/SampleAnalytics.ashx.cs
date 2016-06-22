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

            //public TimeInterval()
            //{
            //    yesterday = new TimePair();
            //    last_7_days = new TimePair();
            //    this_month = new TimePair();
            //    last_month = new TimePair();
            //}
        }
        private class Visit
        {
            public int n { get; set; }
            public int r { get; set; }
        }
        private class Visitor
        {
            public int n { get; set; }
            public int r { get; set; }
        }

        private class DefaultInfo
        {
            public string period { get; set; }
            public Visit visit { get; set; }
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
        }

        private class PageViewItem
        {
            public string page { get; set; }
            public int count { get; set; }
            public string percentage { get; set; }
        }
        //struct AnalyticsIdentity
        //{
        //    public string token { get; set; }
        //    public string alias { get; set; }
        //}

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

        }
        private class UploadAdsStat
        {
            public string url { get; set; }
            public string page { get; set; }
            public string distributor { get; set; }
            public string blog { get; set; }
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
                        case "single":
                            strResult = getSingleUser(Convert.ToString(context.Request.QueryString["a"]), Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]));
                            break;
                        case "vt":
                            strResult = getVisitorTag(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]));
                            break;
                        case "tot":
                            strResult = getTrafficOverTime(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]));
                            break;
                        case "pv":
                            strResult = getPageView(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]));
                            break;
                        case "to":
                            strResult = getTrfficOrigin(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]));
                            break;
                        case "as":
                            strResult = getAdsStat(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"], context.Request.QueryString["u"]);
                            break;
                        case "aqs":
                            strResult = getAdsQuickStat(Convert.ToString(context.Request.QueryString["d"]), Convert.ToString(context.Request.QueryString["s"]), Convert.ToString(context.Request.QueryString["e"]), context.Request.QueryString["b"]);
                            break;
                        default:
                            strResult = getDefaultInfo(context.Request.QueryString["d"], context.Request.QueryString["ti"]);
                            break;
                    }
                }
                else
                {
                    // default
                    strResult = getDefaultInfo(context.Request.QueryString["d"], context.Request.QueryString["ti"]);
                }
            }
            else if (context.Request.Form["update"] != null)
            {
                string strType = context.Request.Form["t"];
                if (strType != null)
                {
                    switch (strType.ToLower())
                    {
                        case "a":   // change alias
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
                    }
                }
            }

            context.Response.ContentType = "text/html";
            context.Response.Charset = Encoding.UTF8.WebName;
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.Write(strResult);
        }

        private string getAdsQuickStat(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, click_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, click_time) <= '{0}'", endDate);

                strSQL = string.Format("select url, page, count(*) as c from tb_page_ads_click_stat where distributor='{0}'{1}{2} group by distributor, url, page order by c desc", distributor, strClause, strClause2);
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
                                ai.page = Convert.ToString(r[1]);
                                ai.count = Convert.ToInt32(r[2]);
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
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getAdsQuickStat) --- Exception: " + e.Message);
            }

            return "{}";
        }

        private string getAdsStat(string distributor, string startDate, string endDate, string blog, string url)
        {
            combineBlogDistributor(blog, distributor);
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, click_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, click_time) <= '{0}'", endDate);
                string strClause3 = "";
                if (!string.IsNullOrEmpty(url))
                {
                    byte[] data = Convert.FromBase64String(url);
                    string decodedString = Encoding.UTF8.GetString(data);
                    strClause3 = string.Format(" and url = '{0}'", decodedString);
                }

                strSQL = string.Format("select url, page, click_time from tb_page_ads_click_stat where distributor='{0}'{1}{2}{3} order by click_time desc", distributor, strClause, strClause2, strClause3);
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
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getAdsStat) --- Exception: " + e.Message);
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
                    string strSQL;

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
                            AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrfficOrigin) --- Exception: " + e.Message);
                        }
                    }


                    strSQL = string.Format("insert into tb_page_ads_click_stat (url, page, distributor) values ('{0}', '{1}', '{2}')",
                        uas.url, uas.page, string.IsNullOrEmpty(uas.distributor) ? uas.blog : uas.distributor);

                    using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                    {
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in updateTableAds(): " + e.Message);
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
                    uf.province_code = string.IsNullOrEmpty(gl.region_code) ? "all" : gl.region_code;
                    uf.ip = gl.ip;
                    // visit_time, alias, type
                    string strSQL;

                    strSQL = string.Format("insert into tb_page_visit_info_xango (token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, country, province, city, province_code, refer, page, distributor) values ('{0}', '{1}', '{2}', '{3}', {4}, '{5}', {6}, '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}')",
                        uf.token, uf.ip, uf.agent, uf.language, uf.color_depth, uf.screen_resolution, uf.time_zone, uf.platform, uf.device, uf.os, uf.country, uf.province, uf.city, uf.province_code, uf.refer, uf.page, uf.distributor);

                    using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                    {
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                        catch (Exception e)
                        {
                            AdssLogger.WriteLog("Exception in updateTable(): " + e.Message + " --- sql: " + strSQL);
                        }
                    }
                }
                catch (Exception e)
                {
                    //System.Diagnostics.Trace.WriteLine(e.Message);
                    AdssLogger.WriteLog("Exception in Deserialize(): " + e.Message);
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
                    uf.province_code = string.IsNullOrEmpty(gl.region_code) ? "all" : gl.region_code;
                    uf.ip = gl.ip;
                    // visit_time, alias, type
                    string strSQL;

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
                            AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrfficOrigin) --- Exception: " + e.Message);
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
                                AdssLogger.WriteLog("Exception in updateTable(): " + e.Message + " --- sql: " + strSQL);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //System.Diagnostics.Trace.WriteLine(e.Message);
                    AdssLogger.WriteLog("Exception in Deserialize(): " + e.Message);
                }


            }
            return "";
        }

        private string getTrfficOrigin(string distributor, string startDate, string endDate)
        {
            // select distinct(refer), COUNT(id) as visit from tb_page_visit_info_xango where distributor='paul' and visit_time >= '2016-05-05' and visit_time <= '2016-06-03' group by refer order by visit desc
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
                strSQL = string.Format("select top 5 refer, COUNT(*) as visit, COUNT(*) * 1.0/ SUM(COUNT(*)) over() as percentage from tb_page_visit_info_xango where distributor='{0}'{1}{2} group by refer order by percentage desc", distributor, strClause, strClause2);
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
                            List<PageViewItem> pList = new List<PageViewItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                PageViewItem pv = new PageViewItem();
                                pv.page = Convert.ToString(r[0]);
                                pv.count = Convert.ToInt32(r[1]);
                                pv.percentage = string.Format("{0:P2}", Convert.ToSingle(r[2]));
                                pList.Add(pv);
                            }
                            return new JavaScriptSerializer().Serialize(pList);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrfficOrigin) --- Exception: " + e.Message);
            }

            return "{}";
        }

        private string getPageView(string distributor, string startDate, string endDate)
        {
            // select distinct(page), COUNT(id) as visit from tb_page_visit_info_xango where distributor='paul' and visit_time >= '2016-05-05' and visit_time <= '2016-06-03' group by page order by visit desc
            // select page, COUNT(*) as count, COUNT(*) * 100.0 / SUM(COUNT(*)) over() as percentage from tb_page_visit_info_xango where distributor='paul' and visit_time >= '2016-05-05' and visit_time <= '2016-06-03' group by page order by percentage desc
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
                strSQL = string.Format("select top 5 page, COUNT(*) as visit, COUNT(*) * 1.0 / SUM(COUNT(*)) over() as percentage from tb_page_visit_info_xango where distributor='{0}'{1}{2} group by page order by percentage desc", distributor, strClause, strClause2);
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
                            List<PageViewItem> pList = new List<PageViewItem>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                PageViewItem pv = new PageViewItem();
                                pv.page = Convert.ToString(r[0]);
                                pv.count = Convert.ToInt32(r[1]);
                                pv.percentage = string.Format("{0:P2}", Convert.ToSingle(r[2]));
                                pList.Add(pv);
                            }
                            return new JavaScriptSerializer().Serialize(pList);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getPageView) --- Exception: " + e.Message);
            }

            return "{}";
        }

        private string getTrafficOverTime(string distributor, string startDate, string endDate)
        {
            // select CAST(visit_time as DATE), COUNT(id) from tb_page_visit_info_xango where visit_time <= '2016-05-05' group by CAST(visit_time as DATE)
            // select t3.d, t3.t, COUNT(t4.type)as c from tb_page_visit_info_xango t4 right join (select * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where visit_time < '2016-01-01') t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 ) t3 on CAST(t4.visit_time as DATE) = t3.d and t4.type = t3.t group by t3.d, t3.t order by t3.d, t3.t
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
                //string strHead = "select DATEDIFF(SECOND,{d '1970-01-01'}, t3.d), t3.t, COUNT(t4.type)as c ";
                string strHead = "select t3.d, t3.t, COUNT(distinct t4.token)as visitor, COUNT(t4.type) as pageview ";
                strSQL = strHead + string.Format("from tb_page_visit_info_xango t4 right join (select '{3}' as dis, * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where distributor='{0}'{1}{2}) t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 ) t3 on CAST(t4.visit_time as DATE) = t3.d and t4.type = t3.t and t4.distributor = '{0}' group by t3.d, t3.t order by t3.d, t3.t", distributor, strClause, strClause2, distributor);
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
                            DefaultInfo di = null;
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                // d,t,visitor,pageview(visit)
                                DataRow r = ds.Tables[0].Rows[i];
                                if (i % 2 == 0)
                                {
                                    di = new DefaultInfo();
                                    di.visit = new Visit();
                                    di.visitor = new Visitor();
                                }

                                //di.period = Convert.ToString(r[0]);
                                di.period = ((DateTime)r[0]).ToString("yyyy-MM-dd");
                                if (i % 2 == 0)
                                {
                                    di.visit.n = Convert.ToInt32(r[3]);
                                    di.visitor.n = Convert.ToInt32(r[2]);
                                }
                                else
                                {
                                    di.visit.r = Convert.ToInt32(r[3]);
                                    di.visitor.r = Convert.ToInt32(r[2]);
                                    dInfo.Add(di);
                                }
                            }
                            return new JavaScriptSerializer().Serialize(dInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getTrafficOverTime sql excute) --- Exception: " + e.Message);
            }
            return "{}";
        }

        private string getDefaultInfo(string distributor, string strJson)
        {
            if (String.IsNullOrEmpty(distributor) || String.IsNullOrEmpty(strJson))
                return "{}";
            else
            {
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

                        string strSQL = strSQL1 + strSQL2 + strSQL3 + strSQL4 + strSQL5;
                        using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                        {
                            if (sc != null)
                            {
                                DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                                if (ds != null && ds.Tables.Count > 0)
                                {
                                    List<DefaultInfo> dInfo = new List<DefaultInfo>();
                                    for (int i = 0; i < ds.Tables.Count; i++)
                                    {
                                        DefaultInfo di = new DefaultInfo();
                                        di.visit = new Visit();
                                        di.visitor = new Visitor();
                                        if (ds.Tables[i].Rows.Count == 0)
                                        {
                                            di.period = labels[i];
                                            di.visit.n = di.visit.r = 0;
                                            di.visitor.n = di.visitor.r = 0;
                                        }
                                        else
                                        {
                                            for (int j = 0; j < ds.Tables[i].Rows.Count; j++)
                                            {   // yesterday	new	86
                                                // yesterday	return	124
                                                DataRow r = ds.Tables[i].Rows[j];
                                                di.period = Convert.ToString(r[0]);
                                                if (String.Compare("new", Convert.ToString(r[1]), StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    di.visitor.n = Convert.ToInt32(r[2]);
                                                    di.visit.n = Convert.ToInt32(r[3]);
                                                }
                                                else if (String.Compare("return", Convert.ToString(r[1]), StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    di.visitor.r = Convert.ToInt32(r[2]);
                                                    di.visit.r = Convert.ToInt32(r[3]);
                                                }
                                            }
                                        }
                                        dInfo.Add(di);
                                    }

                                    return new JavaScriptSerializer().Serialize(dInfo);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AdssLogger.WriteLog("getDefaultInfo Exception: " + ex.Message);
                }
            }
            return "{}";
        }

        private string getDetailVisitor(string distributor, string startDate, string endDate, string blog)
        {
            combineBlogDistributor(blog, distributor);
            // select alias, ip, visit_time, type, page, refer, country, province, city from tb_page_visit_info_xango where visit_time >= '2014-06-01' and visit_time <= '2016-06-02' and distributor = 'paul'
            string strSQL;
            try
            {
                string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
                string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);


                //string strMain = "select alias, ip, DATEDIFF(SECOND,{d '1970-01-01'}, visit_time) as time, type, page, refer, country, province, city from tb_page_visit_info_xango where distributor = '" + distributor + "'";
                string strMain = "select alias, ip, visit_time, type, page, refer, country, province, city, province_code from tb_page_visit_info_xango where distributor = '" + distributor + "'";
                //group by token, alias, country, province, city, language, device", startDate, endDate, strDistributor);
                string strOrder = " order by visit_time desc";
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
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getDetailInfo sql excute) --- Exception: " + e.Message);
            }

            return "{}";
        }

        private void combineBlogDistributor(string blog, string distributor)
        {
            if (string.IsNullOrEmpty(blog) || string.IsNullOrEmpty(distributor))
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

                                if (iDistributor > 0 && iBlog > 0)
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

        private string getSingleUser(string alias, string distributor, string startDate, string endDate)
        {
            string strSQL;
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
                string strMain = "select ip, country, province, city, visit_time from tb_page_visit_info_xango where distributor = '" + distributor + "' and alias='" + alias + "'";
                //group by token, alias, country, province, city, language, device", startDate, endDate, strDistributor);
                string strOrder = " order by visit_time desc";
                strSQL = strMain + strClause + strClause2 + strOrder;
            }
            catch (Exception e)
            {
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getDetailInfo) --- Exception: " + e.Message);
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
                                si.ip = Convert.ToString(r[0]);
                                si.country = Convert.ToString(r[1]);
                                si.province = Convert.ToString(r[2]);
                                si.city = Convert.ToString(r[3]);
                                //si.time = Convert.ToString(r[4]);
                                si.time = ((DateTime)r[4]).ToString("yyyy-MM-dd HH:mm:ss");
                                if (String.Equals(si.country, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    si.country = "";
                                }
                                if (String.Equals(si.province, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    si.province = "";
                                }
                                if (String.Equals(si.city, "all", StringComparison.OrdinalIgnoreCase))
                                {
                                    si.city = "";
                                }
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
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getDetailInfo sql excute) --- Exception: " + e.Message);
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

        private string getVisitorTag(string strDistributor, string startDate, string endDate)
        {
            string strClause = String.IsNullOrEmpty(startDate) ? "" : string.Format(" and convert(date, visit_time) >= '{0}'", startDate);
            string strClause2 = String.IsNullOrEmpty(endDate) ? "" : string.Format(" and convert(date, visit_time) <= '{0}'", endDate);
            string strMain = string.Format("select distinct alias, COUNT(alias)as count, language, device from tb_page_visit_info_xango where distributor = '{0}'", strDistributor);
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
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getResult) --- Exception: " + e.Message);
            }
            return "{}";
        }

        private string updateAlias(string oldValue, string newValue, string distributor)
        {
            string strRet = "";
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
                                strRet = string.Format("Alias already exists. Please enter another one.");
                                return strRet;
                            }
                            else
                            {
                                try
                                {
                                    strSQL = string.Format("update tb_page_visit_info_xango set alias = '{0}' where alias = '{1}' and distributor='{2}'", newValue, oldValue, distributor);
                                    SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                                }
                                catch (Exception e)
                                {
                                    AdssLogger.WriteLog("Exception in updateAlias(): " + e.Message);
                                    strRet = "Operation failed.";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetSampleAnalyticsInfo(getResult) --- Exception: " + e.Message);
                strRet = "Operation failed.";
            }
            return strRet;
        }
    }
}