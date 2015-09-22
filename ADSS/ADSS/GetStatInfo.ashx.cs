using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Microsoft.ApplicationBlocks.Data;

namespace ADSS
{
    /// <summary>
    /// Summary description for GetStatInfo
    /// </summary>
    public class GetStatInfo : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            // get stat of one ads, based on ads id. and period of time

            //get parameters, GET methods

            RequestParameters rp = getRequestParams(context);
            List<BidInfo> bidList = getBidInfo(rp);

            string strResult = getResult(rp, bidList);


            context.Response.ContentType = "text/plain";
            context.Response.Write(strResult);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public List<BidInfo> getBidInfo(RequestParameters rp)
        {
            if (!rp.moneyIssue)
                return null;
            string strSQL = String.Format("select price, describe from tb_bid_info where id = {0}", rp.id);
            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            List<BidInfo> bl = new List<BidInfo>();
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                BidInfo bi = new BidInfo();
                                bi.price = Convert.ToInt32(r[0]);
                                bi.describe = Convert.ToString(r[1]);
                                bl.Add(bi);
                            }
                            return bl;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetStat --- Exception: " + e.Message);
                return null;
            }

        }
        public RequestParameters getRequestParams(HttpContext context)
        {
            RequestParameters rp = new RequestParameters();
            // type -- 1, by location
            // type -- 2, by date
            // type -- 3, by dow
            // type -- 4, by week
            // type -- 5, by device
            // type -- 6, by platform
            if (context.Request.QueryString["type"] != null)
                rp.type = Convert.ToInt32(context.Request.QueryString["type"]);
            // language, en/zh/jp by default en
            if (context.Request.QueryString["lang"] != null)
                rp.language = Convert.ToString(context.Request.QueryString["lang"]);
            // format , json or xml, by default json
            if (context.Request.QueryString["format"] != null)
                rp.format = context.Request.QueryString["format"];
            // id -- id of ads
            if (context.Request.QueryString["id"] != null)
                rp.id = Convert.ToInt32(context.Request.QueryString["id"]);
            // start, and end date
            if (context.Request.QueryString["start"] != null)
                rp.startDate = context.Request.QueryString["start"];
            if (context.Request.QueryString["end"] != null)
                rp.endDate = context.Request.QueryString["end"];
            // money issue, 1 -- enable, 0 -- disable
            if (context.Request.QueryString["mi"] != null)
                rp.moneyIssue = Convert.ToInt32(context.Request.QueryString["mi"]) > 0 ? true : false;
            return rp;
        }

        public string getResult(RequestParameters rp, List<BidInfo> bidInfo)
        {
            if (rp.id > 0)
            {
                switch (rp.type)
                {
                    case 1:
                        return getLocationStat(rp, bidInfo);
                    case 2:
                        return getDateStat(rp, bidInfo);
                    case 3:
                        return getDowStat(rp, bidInfo);
                    case 4:
                        return getWeekStat(rp, bidInfo);
                    case 5:
                        return getDeviceStat(rp, bidInfo);
                    default:
                        break;
                }
            }

            return "";
        }

        //public string getLocationStat(RequestParameters rp, List<BidInfo> bidInfo)
        //{
        //    string strSQL = String.Format("select price, describe from tb_bid_info where id = {0}", rp.id);

        //    if (!String.IsNullOrEmpty(strSQL))
        //    {
        //        StatByLocation sbl = new StatByLocation();
        //        List<BidInfo> bl = new List<BidInfo>();
        //        // execute sql get result
        //        try
        //        {
        //            using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
        //            {
        //                if (sc != null)
        //                {
        //                    // step 1 get bin info, if null return
        //                    if (rp.moneyIssue)
        //                    {
        //                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
        //                        if (ds != null && ds.Tables.Count > 0) //&&  == 1)
        //                        {
        //                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //                            {
        //                                DataRow r = ds.Tables[0].Rows[i];
        //                                BidInfo bi = new BidInfo();
        //                                bi.price = Convert.ToInt32(r[0]);
        //                                bi.describe = Convert.ToString(r[1]);
        //                                bl.Add(bi);
        //                            }
        //                        }
        //                    }

        //                    // step 2, get distinct location
        //                    strSQL = getLocationQuerySQL(rp);
        //                    DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);

        //                    if (dt != null && dt.Tables.Count > 0)
        //                    {
        //                        for (int i = 0; i < dt.Tables[0].Rows.Count; i++)
        //                        {
        //                            DataRow r = dt.Tables[0].Rows[i];
        //                            LocationReportItem lri = new LocationReportItem();
        //                            lri.country = Convert.ToString(r[0]);
        //                            lri.province = Convert.ToString(r[1]);
        //                            lri.city = Convert.ToString(r[2]);
        //                            lri.province_code = Convert.ToString(r[3]);

        //                            // step 2 -- by cpm, cpv, cpa
        //                            // start, end.... do not forget...
        //                            // cpa
        //                            //strSQL = String.Format("select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip and a.country = '{1}' and a.province_code = '{2}' and a.city = '{3}' and b.action = 1 and b.valid = 1", rp.id, lri.country, lri.province_code, lri.city);
        //                            strSQL = getLocationQuerySQLByAction(rp, lri, 1);

        //                            DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
        //                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count == 1)
        //                            {
        //                                r = ds.Tables[0].Rows[0];
        //                                lri.cpa_count = Convert.ToInt32(r[0]);
        //                            }

        //                            // cpm
        //                            //strSQL = String.Format("select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip and a.country = '{1}' and a.province_code = '{2}' and a.city = '{3}' and b.action = 2 and b.valid = 1", rp.id, lri.country, lri.province_code, lri.city);
        //                            strSQL = getLocationQuerySQLByAction(rp, lri, 2);

        //                            ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
        //                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count == 1)
        //                            {
        //                                r = ds.Tables[0].Rows[0];
        //                                lri.cpm_count = Convert.ToInt32(r[0]);
        //                            }

        //                            // cpv
        //                            //strSQL = String.Format("select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip and a.country = '{1}' and a.province_code = '{2}' and a.city = '{3}' and b.action = 3 and b.valid = 1", rp.id, lri.country, lri.province_code, lri.city);
        //                            strSQL = getLocationQuerySQLByAction(rp, lri, 3);

        //                            ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
        //                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count == 1)
        //                            {
        //                                r = ds.Tables[0].Rows[0];
        //                                lri.cpv_count = Convert.ToInt32(r[0]);
        //                            }

        //                            // if has 'all'
        //                            if (String.Equals(lri.country, "all", StringComparison.OrdinalIgnoreCase))
        //                                lri.country = "";
        //                            if (String.Equals(lri.province, "all", StringComparison.OrdinalIgnoreCase))
        //                                lri.province = "";
        //                            if (String.Equals(lri.province_code, "all", StringComparison.OrdinalIgnoreCase))
        //                                lri.province_code = "";
        //                            if (String.Equals(lri.city, "all", StringComparison.OrdinalIgnoreCase))
        //                                lri.city = "";

        //                            if (rp.moneyIssue && bl.Count > 0)
        //                            {
        //                                // step 3
        //                                // calculate cost
        //                                for (int j = 0; j < bl.Count; j++)
        //                                {
        //                                    if (String.Equals("Focus on action", bl[j].describe, StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        // cpa
        //                                        lri.cpa = lri.cpa_count * bl[j].price / 1000.0f;
        //                                    }
        //                                    else if (String.Equals("Focus on impression", bl[j].describe, StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        // cpm
        //                                        lri.cpm = lri.cpm_count * bl[j].price / 1000.0f;
        //                                    }
        //                                    else if (String.Equals("Focus on view", bl[j].describe, StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        // cpv
        //                                        lri.cpv = lri.cpv_count * bl[j].price / 1000.0f;
        //                                    }
        //                                }
        //                            }

        //                            if (lri.cpa_count > 0 || lri.cpm_count > 0 || lri.cpv_count > 0)
        //                                sbl.ReportByLocation.Add(lri);
        //                        }
        //                        sbl.startDate = rp.startDate;
        //                        sbl.endDate = rp.endDate;

        //                        if (sbl.ReportByLocation.Count > 0)
        //                        {
        //                            // total
        //                            LocationReportItem lr = new LocationReportItem();
        //                            lr.country = "Total";
        //                            for (int i = 0; i < sbl.ReportByLocation.Count; i++)
        //                            {
        //                                lr.cpa_count += sbl.ReportByLocation[i].cpa_count;
        //                                lr.cpm_count += sbl.ReportByLocation[i].cpm_count;
        //                                lr.cpv_count += sbl.ReportByLocation[i].cpv_count;

        //                                lr.cpa += sbl.ReportByLocation[i].cpa;
        //                                lr.cpm += sbl.ReportByLocation[i].cpm;
        //                                lr.cpv += sbl.ReportByLocation[i].cpv;
        //                            }

        //                            sbl.ReportByLocation.Add(lr);
        //                        }

        //                        if (String.Equals(rp.format, "xml", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            XmlSerializer x = new XmlSerializer(sbl.GetType());
        //                            using (StringWriter txtWriter = new StringWriter())
        //                            {
        //                                x.Serialize(txtWriter, sbl);
        //                                return txtWriter.ToString();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            JavaScriptSerializer js = new JavaScriptSerializer();
        //                            return js.Serialize(sbl);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //Trace.WriteLine("can not get sql connection.");
        //                    AdssLogger.WriteLog("GetStat --- can not get sql connection.");
        //                }
        //            }

        //        }
        //        catch (Exception e)
        //        {
        //            //Trace.WriteLine(e.Message);
        //            AdssLogger.WriteLog("GetStat --- Exception: " + e.Message);
        //        }
        //    }

        //    // for each location, get total count
        //    return "";
        //}

        //public string getLocationQuerySQL(RequestParameters rp)
        //{
        //    string strSQL = "";
        //    if (String.IsNullOrEmpty(rp.startDate))
        //    {
        //        // all time section.
        //        strSQL = String.Format("select distinct a.country, a.province, a.city, a.province_code from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip", rp.id);
        //    }
        //    else if (String.IsNullOrEmpty(rp.endDate))
        //    {
        //        // after startDate
        //        strSQL = String.Format("select distinct a.country, a.province, a.city, a.province_code from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip and b.time > '{1}'", rp.id, rp.startDate);
        //    }
        //    else
        //    {
        //        // between
        //        strSQL = String.Format("select distinct a.country, a.province, a.city, a.province_code from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip and b.time > '{1}' and b.time < '{2}'", rp.id, rp.startDate, rp.endDate);
        //    }
        //    return strSQL;
        //}

        //public string getLocationQuerySQLByAction(RequestParameters rp, LocationReportItem lri, int action)
        //{
        //    string strSQL = "";
        //    if (String.IsNullOrEmpty(rp.startDate))
        //    {
        //        // all time section.
        //        strSQL = String.Format("select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip and a.country = '{1}' and a.province_code = '{2}' and a.city = '{3}' and b.action = {4} and b.valid = 1", rp.id, lri.country, lri.province_code, lri.city, action);
        //    }
        //    else if (String.IsNullOrEmpty(rp.endDate))
        //    {
        //        // after startDate
        //        strSQL = String.Format("select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip and a.country = '{1}' and a.province_code = '{2}' and a.city = '{3}' and b.action = {5} and b.valid = 1 and b.time > '{4}'", rp.id, lri.country, lri.province_code, lri.city, rp.startDate, action);
        //    }
        //    else
        //    {
        //        // between
        //        strSQL = String.Format("select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = {0} and a.token = b.token and a.ip = b.ip and a.country = '{1}' and a.province_code = '{2}' and a.city = '{3}' and b.action = {6} and b.valid = 1 and b.time > '{4}' and b.time < '{5}'", rp.id, lri.country, lri.province_code, lri.city, rp.startDate, rp.endDate, action);
        //    }
        //    return strSQL;
        //}

        public string getLocationStat(RequestParameters rp, List<BidInfo> bl)
        {
            string strResult = "";
            string strSQLMain, strSQLCondition, strSQLGroup, strSQLOrder;
            strSQLMain = String.Format("select (tb_user_info.country + tb_user_info.province_code + tb_user_info.city) as code, tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct from tb_user_info inner join tb_user_stat on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip and tb_user_stat.id = {0} and tb_user_stat.valid = 1", rp.id);
            strSQLGroup = " group by tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city, tb_user_stat.action having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0";
            strSQLCondition = getTimeCondition(rp);
            strSQLOrder = " order by tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city";
            string strSQL = strSQLMain + strSQLCondition + strSQLGroup + strSQLOrder;

            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            StatByLocation sbl = new StatByLocation();
                            string ideCode = "";
                            int iIndex = -1;
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                string sCode = Convert.ToString(r[0]);
                                int iAction = Convert.ToInt32(r[5]);
                                if (!String.Equals(ideCode, sCode, StringComparison.OrdinalIgnoreCase))
                                {
                                    // new location, new item
                                    LocationReportItem lri = new LocationReportItem();
                                    lri.country = Convert.ToString(r[1]);
                                    lri.province = Convert.ToString(r[2]);
                                    lri.city = Convert.ToString(r[3]);
                                    lri.province_code = Convert.ToString(r[4]);

                                    sbl.ReportByLocation.Add(lri);
                                    iIndex++;
                                    ideCode = sCode;
                                }

                                // use sbl.ReportByLocation[iIndex]
                                // action, ct
                                switch (iAction)
                                {
                                    case 1:
                                        // cpa Focus on action
                                        sbl.ReportByLocation[iIndex].cpa_count = Convert.ToInt32(r[6]);
                                        break;
                                    case 2:
                                        // cpm Focus on impression
                                        sbl.ReportByLocation[iIndex].cpm_count = Convert.ToInt32(r[6]);
                                        break;
                                    case 3:
                                        // cpv Focus on view
                                        sbl.ReportByLocation[iIndex].cpv_count = Convert.ToInt32(r[6]);
                                        break;
                                    default:
                                        break;
                                }

                                // if has 'all'
                                if (String.Equals(sbl.ReportByLocation[iIndex].country, "all", StringComparison.OrdinalIgnoreCase))
                                    sbl.ReportByLocation[iIndex].country = "";
                                if (String.Equals(sbl.ReportByLocation[iIndex].province, "all", StringComparison.OrdinalIgnoreCase))
                                    sbl.ReportByLocation[iIndex].province = "";
                                if (String.Equals(sbl.ReportByLocation[iIndex].province_code, "all", StringComparison.OrdinalIgnoreCase))
                                    sbl.ReportByLocation[iIndex].province_code = "";
                                if (String.Equals(sbl.ReportByLocation[iIndex].city, "all", StringComparison.OrdinalIgnoreCase))
                                    sbl.ReportByLocation[iIndex].city = "";

                                if (rp.moneyIssue && bl.Count > 0)
                                {
                                    // step 3
                                    // calculate cost
                                    for (int j = 0; j < bl.Count; j++)
                                    {
                                        if (String.Equals("Focus on action", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpa
                                            sbl.ReportByLocation[iIndex].cpa = sbl.ReportByLocation[iIndex].cpa_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on impression", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpm
                                            sbl.ReportByLocation[iIndex].cpm = sbl.ReportByLocation[iIndex].cpm_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on view", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpv
                                            sbl.ReportByLocation[iIndex].cpv = sbl.ReportByLocation[iIndex].cpv_count * bl[j].price / 1000.0f;
                                        }
                                    }
                                }
                            }// end of for

                            // total
                            sbl.startDate = rp.startDate;
                            sbl.endDate = rp.endDate;

                            if (sbl.ReportByLocation.Count > 0)
                            {
                                // total
                                LocationReportItem lr = new LocationReportItem();
                                lr.country = "Total";
                                for (int i = 0; i < sbl.ReportByLocation.Count; i++)
                                {
                                    lr.cpa_count += sbl.ReportByLocation[i].cpa_count;
                                    lr.cpm_count += sbl.ReportByLocation[i].cpm_count;
                                    lr.cpv_count += sbl.ReportByLocation[i].cpv_count;

                                    lr.cpa += sbl.ReportByLocation[i].cpa;
                                    lr.cpm += sbl.ReportByLocation[i].cpm;
                                    lr.cpv += sbl.ReportByLocation[i].cpv;
                                }

                                sbl.ReportByLocation.Add(lr);
                            }

                            if (String.Equals(rp.format, "xml", StringComparison.OrdinalIgnoreCase))
                            {
                                XmlSerializer x = new XmlSerializer(sbl.GetType());
                                using (StringWriter txtWriter = new StringWriter())
                                {
                                    x.Serialize(txtWriter, sbl);
                                    strResult = txtWriter.ToString();
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                strResult = js.Serialize(sbl);
                            }
                        }// ds == null or result is null
                    }
                    else
                    {
                        //Trace.WriteLine("can not get sql connection.");
                        AdssLogger.WriteLog("GetStat --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetStat --- Exception: " + e.Message);
            }
            return strResult;
        }

        public string getTimeCondition(RequestParameters rp)
        {
            if (String.IsNullOrEmpty(rp.startDate))
            {
                return "";
            }
            else if (String.IsNullOrEmpty(rp.endDate))
            {
                // after startDate
                return String.Format(" and tb_user_stat.time > '{0}'", rp.startDate);
            }
            else
            {
                // between
                return String.Format(" and tb_user_stat.time > '{0}' and tb_user_stat.time < '{1}'", rp.startDate, rp.endDate);
            }
        }
        public string getDateStat(RequestParameters rp, List<BidInfo> bl)
        {
            // stat by single date
            string strResult = "";
            string strSQLMain, strSQLCondition, strSQLGroup, strSQLOrder;
            strSQLMain = String.Format("select (YEAR(tb_user_stat.time) * 10000 + MONTH(tb_user_stat.time) * 100 + DAY(tb_user_stat.time)) as iDate , datename(dw, tb_user_stat.time) as dow, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct from tb_user_info inner join tb_user_stat on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip and tb_user_stat.id = {0} and tb_user_stat.valid = 1", rp.id);
            strSQLGroup = " group by YEAR(tb_user_stat.time), MONTH(tb_user_stat.time), DAY(tb_user_stat.time), datename(dw, tb_user_stat.time), tb_user_stat.action having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0";
            strSQLCondition = getTimeCondition(rp);
            strSQLOrder = " order by (YEAR(tb_user_stat.time) * 10000 + MONTH(tb_user_stat.time) * 100 + DAY(tb_user_stat.time))";
            string strSQL = strSQLMain + strSQLCondition + strSQLGroup + strSQLOrder;

            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            StatByDate sbd = new StatByDate();
                            int ideDate = 0;
                            int iIndex = -1;
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                int iDate = Convert.ToInt32(r[0]);
                                int iAction = Convert.ToInt32(r[2]);
                                if (iDate != ideDate)
                                {
                                    // new location, new item
                                    TimeReportItem tri = new TimeReportItem();
                                    tri.date = iDate;
                                    tri.dow = Convert.ToString(r[1]);

                                    sbd.ReportByTime.Add(tri);
                                    iIndex++;
                                    ideDate = iDate;
                                }

                                // use sbl.ReportByLocation[iIndex]
                                // action, ct
                                switch (iAction)
                                {
                                    case 1:
                                        // cpa Focus on action
                                        sbd.ReportByTime[iIndex].cpa_count = Convert.ToInt32(r[3]);
                                        break;
                                    case 2:
                                        // cpm Focus on impression
                                        sbd.ReportByTime[iIndex].cpm_count = Convert.ToInt32(r[3]);
                                        break;
                                    case 3:
                                        // cpv Focus on view
                                        sbd.ReportByTime[iIndex].cpv_count = Convert.ToInt32(r[3]);
                                        break;
                                    default:
                                        break;
                                }

                                if (rp.moneyIssue && bl.Count > 0)
                                {
                                    // step 3
                                    // calculate cost
                                    for (int j = 0; j < bl.Count; j++)
                                    {
                                        if (String.Equals("Focus on action", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpa
                                            sbd.ReportByTime[iIndex].cpa = sbd.ReportByTime[iIndex].cpa_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on impression", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpm
                                            sbd.ReportByTime[iIndex].cpm = sbd.ReportByTime[iIndex].cpm_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on view", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpv
                                            sbd.ReportByTime[iIndex].cpv = sbd.ReportByTime[iIndex].cpv_count * bl[j].price / 1000.0f;
                                        }
                                    }
                                }
                            }// end of for

                            // total
                            sbd.startDate = rp.startDate;
                            sbd.endDate = rp.endDate;

                            if (sbd.ReportByTime.Count > 0)
                            {
                                // total
                                TimeReportItem tr = new TimeReportItem();
                                tr.dow = "Total";
                                tr.date = 0;
                                for (int i = 0; i < sbd.ReportByTime.Count; i++)
                                {
                                    tr.cpa_count += sbd.ReportByTime[i].cpa_count;
                                    tr.cpm_count += sbd.ReportByTime[i].cpm_count;
                                    tr.cpv_count += sbd.ReportByTime[i].cpv_count;

                                    tr.cpa += sbd.ReportByTime[i].cpa;
                                    tr.cpm += sbd.ReportByTime[i].cpm;
                                    tr.cpv += sbd.ReportByTime[i].cpv;
                                }

                                sbd.ReportByTime.Add(tr);
                            }

                            if (String.Equals(rp.format, "xml", StringComparison.OrdinalIgnoreCase))
                            {
                                XmlSerializer x = new XmlSerializer(sbd.GetType());
                                using (StringWriter txtWriter = new StringWriter())
                                {
                                    x.Serialize(txtWriter, sbd);
                                    strResult = txtWriter.ToString();
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                strResult = js.Serialize(sbd);
                            }
                        }// ds == null or result is null
                    }
                    else
                    {
                        //Trace.WriteLine("can not get sql connection.");
                        AdssLogger.WriteLog("GetStat --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetStat --- Exception: " + e.Message);
            }
            return strResult;
        }

        public string getDowStat(RequestParameters rp, List<BidInfo> bl)
        {
            string strResult = "";
            string strSQLMain, strSQLCondition, strSQLGroup, strSQLOrder;
            strSQLMain = String.Format("select datepart(dw, tb_user_stat.time) as di, datename(dw, tb_user_stat.time) as dow, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct from tb_user_info inner join tb_user_stat on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip and tb_user_stat.id = {0} and tb_user_stat.valid = 1", rp.id);
            strSQLGroup = " group by datename(dw, tb_user_stat.time), datepart(dw, tb_user_stat.time), tb_user_stat.action having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0";
            strSQLCondition = getTimeCondition(rp);
            strSQLOrder = " order by datepart(dw, tb_user_stat.time)";
            string strSQL = strSQLMain + strSQLCondition + strSQLGroup + strSQLOrder;

            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            StatByDate sbd = new StatByDate();
                            int ideDate = 0; // should be 1-7
                            int iIndex = -1;
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                int iDate = Convert.ToInt32(r[0]);
                                int iAction = Convert.ToInt32(r[2]);
                                if (iDate != ideDate)
                                {
                                    // new location, new item
                                    TimeReportItem tri = new TimeReportItem();
                                    tri.date = iDate;
                                    tri.dow = Convert.ToString(r[1]);

                                    sbd.ReportByTime.Add(tri);
                                    iIndex++;
                                    ideDate = iDate;
                                }

                                // use sbl.ReportByLocation[iIndex]
                                // action, ct
                                switch (iAction)
                                {
                                    case 1:
                                        // cpa Focus on action
                                        sbd.ReportByTime[iIndex].cpa_count = Convert.ToInt32(r[3]);
                                        break;
                                    case 2:
                                        // cpm Focus on impression
                                        sbd.ReportByTime[iIndex].cpm_count = Convert.ToInt32(r[3]);
                                        break;
                                    case 3:
                                        // cpv Focus on view
                                        sbd.ReportByTime[iIndex].cpv_count = Convert.ToInt32(r[3]);
                                        break;
                                    default:
                                        break;
                                }

                                if (rp.moneyIssue && bl.Count > 0)
                                {
                                    // step 3
                                    // calculate cost
                                    for (int j = 0; j < bl.Count; j++)
                                    {
                                        if (String.Equals("Focus on action", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpa
                                            sbd.ReportByTime[iIndex].cpa = sbd.ReportByTime[iIndex].cpa_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on impression", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpm
                                            sbd.ReportByTime[iIndex].cpm = sbd.ReportByTime[iIndex].cpm_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on view", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpv
                                            sbd.ReportByTime[iIndex].cpv = sbd.ReportByTime[iIndex].cpv_count * bl[j].price / 1000.0f;
                                        }
                                    }
                                }
                            }// end of for

                            // total
                            sbd.startDate = rp.startDate;
                            sbd.endDate = rp.endDate;

                            if (sbd.ReportByTime.Count > 0)
                            {
                                // total
                                TimeReportItem tr = new TimeReportItem();
                                tr.dow = "Total";
                                tr.date = 0;
                                for (int i = 0; i < sbd.ReportByTime.Count; i++)
                                {
                                    tr.cpa_count += sbd.ReportByTime[i].cpa_count;
                                    tr.cpm_count += sbd.ReportByTime[i].cpm_count;
                                    tr.cpv_count += sbd.ReportByTime[i].cpv_count;

                                    tr.cpa += sbd.ReportByTime[i].cpa;
                                    tr.cpm += sbd.ReportByTime[i].cpm;
                                    tr.cpv += sbd.ReportByTime[i].cpv;
                                }

                                sbd.ReportByTime.Add(tr);
                            }

                            if (String.Equals(rp.format, "xml", StringComparison.OrdinalIgnoreCase))
                            {
                                XmlSerializer x = new XmlSerializer(sbd.GetType());
                                using (StringWriter txtWriter = new StringWriter())
                                {
                                    x.Serialize(txtWriter, sbd);
                                    strResult = txtWriter.ToString();
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                strResult = js.Serialize(sbd);
                            }
                        }// ds == null or result is null
                    }
                    else
                    {
                        //Trace.WriteLine("can not get sql connection.");
                        AdssLogger.WriteLog("GetStat --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetStat --- Exception: " + e.Message);
            }
            return strResult;
        }

        public string getWeekStat(RequestParameters rp, List<BidInfo> bl)
        {
            string strResult = "";
            string strSQLMain, strSQLCondition, strSQLGroup, strSQLOrder;
            strSQLMain = String.Format("select re.id, (YEAR(re.start_of_week) * 10000 + MONTH(re.start_of_week) * 100 + DAY(re.start_of_week)) as startDate, CONVERT(VARCHAR(11),re.start_of_week,106) as ss, (YEAR(re.end_of_week) * 10000 + MONTH(re.end_of_week) * 100 + DAY(re.end_of_week)) as endDate, CONVERT(VARCHAR(11),re.end_of_week,106) as ae, re.action, re.ct from( select YEAR(tb_user_stat.time) * 100 + datepart(wk, tb_user_stat.time) as id, DATEADD(wk, DATEDIFF(wk, 6, CAST(RTRIM(YEAR(tb_user_stat.time) * 10000 + 1 * 100 + 1) AS DATETIME)) + ( datepart(wk, tb_user_stat.time) - 1 ), 6) AS [start_of_week], DATEADD(second, -1, DATEADD(day, DATEDIFF(day, 0, DATEADD(wk, DATEDIFF(wk, 5, CAST(RTRIM(YEAR(tb_user_stat.time) * 10000 + 1 * 100 + 1) AS DATETIME)) + ( datepart(wk, tb_user_stat.time) + -1 ), 5)) + 1, 0)) AS [end_of_week], tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip + CAST(tb_user_stat.action as varchar(10))) as ct from tb_user_info inner join tb_user_stat on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip and tb_user_stat.id = {0} and tb_user_stat.valid = 1", rp.id);
            strSQLGroup = " group by YEAR(tb_user_stat.time), datepart(wk, tb_user_stat.time), tb_user_stat.action having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0) as re";
            strSQLCondition = getTimeCondition(rp);
            strSQLOrder = " order by re.id";
            string strSQL = strSQLMain + strSQLCondition + strSQLGroup + strSQLOrder;

            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            StatByWeek sbw = new StatByWeek();
                            int ideDate = 0; 
                            int iIndex = -1;
                            // 201533,	20150809,	09 Aug 2015,	20150815,	15 Aug 2015,	2,	16
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                int iDate = Convert.ToInt32(r[0]);
                                int iAction = Convert.ToInt32(r[5]);
                                if (iDate != ideDate)
                                {
                                    // new location, new item
                                    WeekReportItem wri = new WeekReportItem();
                                    wri.strWeek = String.Format("{0} - {1}", Convert.ToString(r[2]), Convert.ToString(r[4]));
                                    wri.weekStart = Convert.ToInt32(r[1]);
                                    wri.weekEnd = Convert.ToInt32(r[3]);

                                    sbw.ReportByWeek.Add(wri);
                                    iIndex++;
                                    ideDate = iDate;
                                }

                                // use sbl.ReportByLocation[iIndex]
                                // action, ct
                                switch (iAction)
                                {
                                    case 1:
                                        // cpa Focus on action
                                        sbw.ReportByWeek[iIndex].cpa_count = Convert.ToInt32(r[6]);
                                        break;
                                    case 2:
                                        // cpm Focus on impression
                                        sbw.ReportByWeek[iIndex].cpm_count = Convert.ToInt32(r[6]);
                                        break;
                                    case 3:
                                        // cpv Focus on view
                                        sbw.ReportByWeek[iIndex].cpv_count = Convert.ToInt32(r[6]);
                                        break;
                                    default:
                                        break;
                                }

                                if (rp.moneyIssue && bl.Count > 0)
                                {
                                    // step 3
                                    // calculate cost
                                    for (int j = 0; j < bl.Count; j++)
                                    {
                                        if (String.Equals("Focus on action", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpa
                                            sbw.ReportByWeek[iIndex].cpa = sbw.ReportByWeek[iIndex].cpa_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on impression", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpm
                                            sbw.ReportByWeek[iIndex].cpm = sbw.ReportByWeek[iIndex].cpm_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on view", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpv
                                            sbw.ReportByWeek[iIndex].cpv = sbw.ReportByWeek[iIndex].cpv_count * bl[j].price / 1000.0f;
                                        }
                                    }
                                }
                            }// end of for

                            // total
                            sbw.startDate = rp.startDate;
                            sbw.endDate = rp.endDate;

                            if (sbw.ReportByWeek.Count > 0)
                            {
                                // total
                                WeekReportItem wr = new WeekReportItem();
                                wr.strWeek = "Total";
                                for (int i = 0; i < sbw.ReportByWeek.Count; i++)
                                {
                                    wr.cpa_count += sbw.ReportByWeek[i].cpa_count;
                                    wr.cpm_count += sbw.ReportByWeek[i].cpm_count;
                                    wr.cpv_count += sbw.ReportByWeek[i].cpv_count;

                                    wr.cpa += sbw.ReportByWeek[i].cpa;
                                    wr.cpm += sbw.ReportByWeek[i].cpm;
                                    wr.cpv += sbw.ReportByWeek[i].cpv;
                                }

                                sbw.ReportByWeek.Add(wr);
                            }

                            if (String.Equals(rp.format, "xml", StringComparison.OrdinalIgnoreCase))
                            {
                                XmlSerializer x = new XmlSerializer(sbw.GetType());
                                using (StringWriter txtWriter = new StringWriter())
                                {
                                    x.Serialize(txtWriter, sbw);
                                    strResult = txtWriter.ToString();
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                strResult = js.Serialize(sbw);
                            }
                        }// ds == null or result is null
                    }
                    else
                    {
                        //Trace.WriteLine("can not get sql connection.");
                        AdssLogger.WriteLog("GetStat --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetStat --- Exception: " + e.Message);
            }
            return strResult;
        }

        public string getDeviceStat(RequestParameters rp, List<BidInfo> bl)
        {
            string strResult = "";
            string strSQLMain, strSQLCondition, strSQLGroup, strSQLOrder;
            strSQLMain = String.Format("select tb_user_info.device, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct from tb_user_info inner join tb_user_stat on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip and tb_user_stat.id = {0} and tb_user_stat.valid = 1", rp.id);
            strSQLGroup = " group by tb_user_info.device, tb_user_stat.action having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0";
            strSQLCondition = getTimeCondition(rp);
            strSQLOrder = " order by tb_user_info.device";
            string strSQL = strSQLMain + strSQLCondition + strSQLGroup + strSQLOrder;

            try
            {
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet ds = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            StatByDevice sbd = new StatByDevice();
                            string sD = ""; 
                            int iIndex = -1;
                            // Desktop	2	21
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow r = ds.Tables[0].Rows[i];
                                string sDevice = Convert.ToString(r[0]);
                                int iAction = Convert.ToInt32(r[1]);
                                if (!String.Equals(sD, sDevice))
                                {
                                    // new location, new item
                                    DeviceReportItem dri = new DeviceReportItem();
                                    dri.device = sDevice;

                                    sbd.ReportByDevice.Add(dri);
                                    iIndex++;
                                    sD = sDevice;
                                }

                                // use sbl.ReportByLocation[iIndex]
                                // action, ct
                                switch (iAction)
                                {
                                    case 1:
                                        // cpa Focus on action
                                        sbd.ReportByDevice[iIndex].cpa_count = Convert.ToInt32(r[2]);
                                        break;
                                    case 2:
                                        // cpm Focus on impression
                                        sbd.ReportByDevice[iIndex].cpm_count = Convert.ToInt32(r[2]);
                                        break;
                                    case 3:
                                        // cpv Focus on view
                                        sbd.ReportByDevice[iIndex].cpv_count = Convert.ToInt32(r[2]);
                                        break;
                                    default:
                                        break;
                                }

                                if (rp.moneyIssue && bl.Count > 0)
                                {
                                    // step 3
                                    // calculate cost
                                    for (int j = 0; j < bl.Count; j++)
                                    {
                                        if (String.Equals("Focus on action", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpa
                                            sbd.ReportByDevice[iIndex].cpa = sbd.ReportByDevice[iIndex].cpa_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on impression", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpm
                                            sbd.ReportByDevice[iIndex].cpm = sbd.ReportByDevice[iIndex].cpm_count * bl[j].price / 1000.0f;
                                        }
                                        else if (String.Equals("Focus on view", bl[j].describe, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // cpv
                                            sbd.ReportByDevice[iIndex].cpv = sbd.ReportByDevice[iIndex].cpv_count * bl[j].price / 1000.0f;
                                        }
                                    }
                                }
                            }// end of for

                            // total
                            sbd.startDate = rp.startDate;
                            sbd.endDate = rp.endDate;

                            if (sbd.ReportByDevice.Count > 0)
                            {
                                // total
                                DeviceReportItem dr = new DeviceReportItem();
                                dr.device = "Total";
                                for (int i = 0; i < sbd.ReportByDevice.Count; i++)
                                {
                                    dr.cpa_count += sbd.ReportByDevice[i].cpa_count;
                                    dr.cpm_count += sbd.ReportByDevice[i].cpm_count;
                                    dr.cpv_count += sbd.ReportByDevice[i].cpv_count;

                                    dr.cpa += sbd.ReportByDevice[i].cpa;
                                    dr.cpm += sbd.ReportByDevice[i].cpm;
                                    dr.cpv += sbd.ReportByDevice[i].cpv;
                                }

                                sbd.ReportByDevice.Add(dr);
                            }

                            if (String.Equals(rp.format, "xml", StringComparison.OrdinalIgnoreCase))
                            {
                                XmlSerializer x = new XmlSerializer(sbd.GetType());
                                using (StringWriter txtWriter = new StringWriter())
                                {
                                    x.Serialize(txtWriter, sbd);
                                    strResult = txtWriter.ToString();
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                strResult = js.Serialize(sbd);
                            }
                        }// ds == null or result is null
                    }
                    else
                    {
                        //Trace.WriteLine("can not get sql connection.");
                        AdssLogger.WriteLog("GetStat --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("GetStat --- Exception: " + e.Message);
            }
            return strResult;
        }
    }
}