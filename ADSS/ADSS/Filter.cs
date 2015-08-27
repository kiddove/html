/*****************************************************************************
Copyright: 
File name: Filter.cs
Description: interface and implement of filters, used for getting id list from DB by given condition
Author: paul
Version: 1.0
Date: 04/30/2015
History: 
 * 1. create interface and several implements, including language, network, location, device, etc
 * 2. read all sql or fields from config --- to be continued
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.ApplicationBlocks.Data;

namespace ADSS
{
    /*
    List<int> ages = new List<int> { 21, 46, 46, 55, 17, 21, 55, 55 };

    IEnumerable<int> distinctAges = ages.Distinct();
     */
    public interface IFilter
    {
        string FormSQL(List<int> ids);
    }

    // use for specific item such as language, device, ec.
    public class SpecificFilter : IFilter
    {
        // property
        protected ICondition m_condition;

        public SpecificFilter()
        {
        }

        // function
        public virtual string FormSQL(List<int> ids) { return ""; }

        public virtual List<int> GetList(List<int> ids)
        {
            // by default only id
            List<int> idList = new List<int>();
            try
            {
                //SqlConnection sc = DBConn.GetConnection();
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        using (DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, FormSQL(ids)))
                        {
                            if (dt.Tables.Count > 0)
                            {
                                using (DataTable dtb = dt.Tables[0])
                                {
                                    foreach (DataRow r in dtb.Rows)
                                    {
                                        //string strInfo = String.Format("{0}: {1}", r[0], r[1]);
                                        //System.Diagnostics.Trace.WriteLine(strInfo);
                                        idList.Add(Convert.ToInt32(r[0]));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        AdssLogger.WriteLog("SpecificFilter.GetList() --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("SpecificFilter.GetList() --- Exception: " + e.Message);
            }

            return idList;
        }
    }

    public class BasicFilter : SpecificFilter
    {
        public BasicFilter(ICondition con)
        {
            m_condition = con;
        }
        // function
        public override string FormSQL(List<int> ids)
        {
            // todo paul "and problem"
            string strSQL = String.Format("select distinct(id) from dbo.tb_ads_info where camp_status = 1 {0};", m_condition.GetCondition(ids));
            return strSQL;
        }
    }

    // device
    public class DeviceFilter : SpecificFilter
    {
        // constructor
        public DeviceFilter(ICondition con)
        {
            m_condition = con;
        }
        // function
        public override string FormSQL(List<int> ids)
        {
            //string strSQL = String.Format("select distinct(a.id) from dbo.tb_tgt_device as a, dbo.tb_device as b where a.device_id = b.device_id and b.device {0};", m_condition.GetCondition());
            string strSQL = String.Format("select distinct(id) from dbo.tb_tgt_device where device {0};", m_condition.GetCondition(ids));
            return strSQL;
        }
    }

    // language
    public class LanguageFilter : SpecificFilter
    {
        // constructor
        public LanguageFilter(ICondition con)
        {
            m_condition = con;
        }

        public override string FormSQL(List<int> ids)
        {
            //string strSQL = String.Format("select distinct(a.id) from dbo.tb_tgt_language as a, dbo.tb_language as b where a.language_id = b.language_id and b.language {0};", m_condition.GetCondition());
            string strSQL = String.Format("select distinct(id) from dbo.tb_tgt_language where language {0};", m_condition.GetCondition(ids));
            return strSQL;
        }
        // select distinct(a.id), b.language from dbo.tb_tgt_language as a, dbo.tb_language as b where a.language_id = b.language_id and b.language in ('English', 'ALL');
    }

    // location
    public class LocationFilter : SpecificFilter
    {
        // constructor
        public LocationFilter(ICondition con)
        {
            m_condition = con;
        }

        public override string FormSQL(List<int> ids)
        {
            //string strSQL = String.Format("select distinct(id) from dbo.tb_tgt_location where location_code {0};", m_condition.GetCondition());
            string strSQL = String.Format("select distinct(id) from dbo.tb_tgt_location where {0};", m_condition.GetCondition(ids));
            return strSQL;
        }
    }

    // network
    public class NetworkFilter : SpecificFilter
    {
        // constructor
        public NetworkFilter(ICondition con)
        {
            m_condition = con;
        }

        public override string FormSQL(List<int> ids)
        {
            //string strSQL = String.Format("select distinct(a.id) from dbo.tb_tgt_network as a, dbo.tb_network as b where a.network_id = b.network_id and b.network {0};", m_condition.GetCondition());
            string strSQL = String.Format("select distinct(id) from dbo.tb_tgt_network where network {0};", m_condition.GetCondition(ids));
            return strSQL;
        }
        // select distinct(a.id), b.language from dbo.tb_tgt_language as a, dbo.tb_language as b where a.language_id = b.language_id and b.language in ('English', 'ALL');
    }

    public class FinalFilter : SpecificFilter
    {
        public FinalFilter(ICondition con)
        {
            m_condition = con;
        }
        // function
        public override string FormSQL(List<int> ids)
        {
            string strSQL = String.Format("select id, source, type_ads, type_video_media, click_reference, time_to_play_video_ads, multstream from dbo.tb_ads_info where camp_status = 1 and id {0}", m_condition.GetCondition(ids));
            return strSQL;
        }

        public string GetResult(List<int> ids)
        {
            string strResult = "{}";
            GenericInfo gInfo = new GenericInfo();
            try
            {
                //SqlConnection sc = DBConn.GetConnection();
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, FormSQL(ids));
                        DataTable dtb = dt.Tables[0];

                        foreach (DataRow r in dtb.Rows)
                        {
                            //string strInfo = String.Format("{0}: {1}", r[0], r[1]);
                            //System.Diagnostics.Trace.WriteLine(strInfo);
                            //idList.Add(Convert.ToInt32(r[0]));
                            resultInfo ri = new resultInfo();
                            ri.id = Convert.ToInt32(r[0]);
                            ri.src = Convert.ToString(r[1]);
                            ri.ads_type = Convert.ToByte(r[2]);
                            ri.type = Convert.ToString(r[3]);
                            ri.link = Convert.ToString(r[4]);
                            ri.ads_time = Convert.ToInt32(r[5]);
                            ri.multstream = Convert.ToString(r[6]);

                            switch (ri.ads_type)
                            {
                                case 1:
                                case 2:
                                    gInfo.pre.Add(ri);
                                    break;
                                //case 2:
                                //    gInfo.mid.Add(ri);
                                //    break;
                                case 3:
                                    gInfo.display_big.Add(ri);
                                    break;
                                case 4:
                                    gInfo.display_small.Add(ri);
                                    break;
                                default:
                                    break;
                            }
                        }
                        dtb.Clear();
                        dt.Clear();

                        strResult = new JavaScriptSerializer().Serialize(gInfo);
                        //System.Diagnostics.Trace.WriteLine(strResult);
                    }
                    else
                    {
                        //Trace.WriteLine("can not get sql connection.");
                        AdssLogger.WriteLog("FinalFilter.GetResult() --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("FinalFilter.GetResult() --- Exception: " + e.Message);
            }

            return strResult;
        }

        public string GetFullResult()
        {
            string strResult = "{}";
            AdsInfo adi = new AdsInfo();
            try
            {
                //SqlConnection sc = DBConn.GetConnection();
                using (TransactionScope ts = new TransactionScope())
                {
                    using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                    {
                        if (sc != null)
                        {
                            // step1. basic
                            //string strSQL = String.Format("select id, source, type_ads, type_video_media, click_reference, time_to_play_video_ads, src_account, convert(VARCHAR(24), camp_start_date, 20), convert(VARCHAR(24), camp_stop_date, 20), camp_status, campaign_name from dbo.tb_ads_info where camp_status = 1 and id {0}", m_condition.GetCondition());
                            string strSQL = String.Format("select id, source, type_ads, type_video_media, click_reference, time_to_play_video_ads, src_account, convert(VARCHAR(24), camp_start_date, 20), convert(VARCHAR(24), camp_stop_date, 20), camp_status, campaign_name, multstream from dbo.tb_ads_info where id {0}", m_condition.GetCondition(null));
                            // step2. addional
                            DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);

                            if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count == 1)
                            {
                                DataRow r = dt.Tables[0].Rows[0];
                                adi.id = Convert.ToInt32(r[0]);
                                adi.source = Convert.ToString(r[1]);
                                adi.type_ads = Convert.ToByte(r[2]);
                                adi.type_video_media = Convert.ToString(r[3]);
                                adi.click_reference = Convert.ToString(r[4]);
                                adi.time_to_play_video_ads = Convert.ToInt32(r[5]);
                                adi.src_account = Convert.ToString(r[6]);
                                adi.camp_start_date = Convert.ToString(r[7]);
                                adi.camp_stop_date = Convert.ToString(r[8]);
                                adi.camp_status = Convert.ToByte(r[9]);
                                adi.campaign_name = Convert.ToString(r[10]);
                                adi.multstream = Convert.ToString(r[11]);
                            }
                            // step2. addional

                            // network
                            //strSQL = String.Format("select b.network from dbo.tb_tgt_network as a, dbo.tb_network as b where a.network_id = b.network_id and a.id {0}", m_condition.GetCondition());
                            strSQL = String.Format("select network from dbo.tb_tgt_network where id {0}", m_condition.GetCondition(null));
                            dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                            if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count > 0)
                            {
                                NetworkInfo ni = new NetworkInfo();
                                foreach (DataRow r in dt.Tables[0].Rows)
                                {
                                    ni.network.Add(Convert.ToString(r[0]));
                                }
                                adi.tgt_network = ni;
                            }
                            // device
                            //strSQL = String.Format("select b.device from dbo.tb_tgt_device as a, dbo.tb_device as b where a.device_id = b.device_id and a.id {0}", m_condition.GetCondition());
                            strSQL = String.Format("select device from dbo.tb_tgt_device where id {0}", m_condition.GetCondition(null));
                            dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                            if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count > 0)
                            {
                                DeviceInfo di = new DeviceInfo();
                                foreach (DataRow r in dt.Tables[0].Rows)
                                {
                                    di.device.Add(Convert.ToString(r[0]));
                                }
                                adi.tgt_device = di;
                            }
                            // language
                            //strSQL = String.Format("select b.language from dbo.tb_tgt_language as a, dbo.tb_language as b where a.language_id = b.language_id and a.id {0}", m_condition.GetCondition());
                            strSQL = String.Format("select language from dbo.tb_tgt_language where id {0}", m_condition.GetCondition(null));
                            dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                            if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count > 0)
                            {
                                LanguageInfo li = new LanguageInfo();
                                foreach (DataRow r in dt.Tables[0].Rows)
                                {
                                    li.language.Add(Convert.ToString(r[0]));
                                }
                                adi.tgt_language = li;
                            }

                            ///////////////////////////////////////////////////////////////////////////////////////////////
                            //// location
                            ////strSQL = String.Format("select cou.country, pro.province, ci.city from dbo.tb_tgt_location as l, dbo.tb_country as cou, dbo.tb_province as pro, dbo.tb_city as ci where l.location_code = (select right('000' + CAST((select country_id from tb_country where country = cou.country) as varchar(3)), 3) + right('000' + cast((select province_id from tb_province where province = pro.province) as varchar(3)), 3) + right('000' + cast((select city_id from tb_city where city = ci.city) as varchar(3)), 3)) and l.id {0} order by cou.country, pro.province, ci.city;", m_condition.GetCondition());
                            //strSQL = String.Format("select country, province, city from dbo.tb_tgt_location where id {0} order by country, province, city;", m_condition.GetCondition());
                            //dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                            //if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count > 0)
                            //{
                            //    LocationInfo lci = new LocationInfo();
                            //    int i1, i2;
                            //    foreach (DataRow r in dt.Tables[0].Rows)
                            //    {
                            //        string sCountry = Convert.ToString(r[0]);
                            //        string sProvince = Convert.ToString(r[1]);
                            //        string sCity = Convert.ToString(r[2]);

                            //        //i1 = lci.country.FindIndex(x => x.name == sCountry);
                            //        // all value from database should use ==
                            //        i1 = lci.country.FindIndex(x => x.name.Equals(sCountry, StringComparison.OrdinalIgnoreCase));
                            //        if (i1 < 0)
                            //        {
                            //            CountryInfo ci = new CountryInfo();
                            //            ci.name = sCountry;
                            //            lci.country.Add(ci);
                            //            i1 = lci.country.Count - 1;
                            //        }

                            //        //lci.country[i1]
                            //        i2 = lci.country[i1].province.FindIndex(x => x.name == sProvince);
                            //        //i2 = lci.country.FindIndex(x => x.name.Equals(sProvince, StringComparison.OrdinalIgnoreCase));
                            //        if (i2 < 0)
                            //        {
                            //            ProvinceInfo pi = new ProvinceInfo();
                            //            pi.name = sProvince;
                            //            lci.country[i1].province.Add(pi);
                            //            i2 = lci.country[i1].province.Count - 1;
                            //        }
                            //        lci.country[i1].province[i2].city.Add(sCity);
                            //    }
                            //    adi.tgt_location = lci;
                            //}
                            ///////////////////////////////////////////////////////////////////////////////////////////////


                            // location
                            //strSQL = String.Format("select cou.country, pro.province, ci.city from dbo.tb_tgt_location as l, dbo.tb_country as cou, dbo.tb_province as pro, dbo.tb_city as ci where l.location_code = (select right('000' + CAST((select country_id from tb_country where country = cou.country) as varchar(3)), 3) + right('000' + cast((select province_id from tb_province where province = pro.province) as varchar(3)), 3) + right('000' + cast((select city_id from tb_city where city = ci.city) as varchar(3)), 3)) and l.id {0} order by cou.country, pro.province, ci.city;", m_condition.GetCondition());
                            strSQL = String.Format("select country, province_code, city from dbo.tb_tgt_location where id {0} order by country, province_code, city;", m_condition.GetCondition(null));
                            dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                            if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count > 0)
                            {
                                LocationInfo lci = new LocationInfo();
                                foreach (DataRow r in dt.Tables[0].Rows)
                                {
                                    GeoLocation gl = new GeoLocation();
                                    gl.country_name = Convert.ToString(r[0]);
                                    gl.region_code = Convert.ToString(r[1]);
                                    gl.city = Convert.ToString(r[2]);
                                    lci.location.Add(gl);
                                     
                                }
                                adi.tgt_location = lci;
                            }

                            // bid info
                            strSQL = String.Format("select budget, price, describe from dbo.tb_bid_info where id {0}", m_condition.GetCondition(null));
                            dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                            if (dt != null && dt.Tables.Count > 0)// && dt.Tables[0].Rows.Count == 1)
                            {
                                //BidInfo bi = new BidInfo();
                                foreach (DataRow r in dt.Tables[0].Rows)
                                {
                                    BidInfo bi = new BidInfo();
                                    bi.budget = Convert.ToInt32(r[0]);
                                    bi.price = Convert.ToInt32(r[1]);
                                    bi.describe = Convert.ToString(r[2]);

                                    adi.ads_bid.Add(bi);
                                }
                            }

                            // keyword
                            strSQL = String.Format("select keyword from dbo.tb_keyword_info where id {0}", m_condition.GetCondition(null));
                            dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                            if (dt != null && dt.Tables.Count > 0)// && dt.Tables[0].Rows.Count == 1)
                            {
                                //BidInfo bi = new BidInfo();
                                foreach (DataRow r in dt.Tables[0].Rows)
                                {
                                    adi.keyword.Add(Convert.ToString(r[0]));
                                }
                            }
                            strResult = new JavaScriptSerializer().Serialize(adi);
                            //System.Diagnostics.Trace.WriteLine(strResult);
                        }
                        else
                        {
                            //Trace.WriteLine("can not get sql connection.");
                            AdssLogger.WriteLog("FinalFilter.GetFullResult() --- can not get sql connection.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("FinalFilter.GetFullResult() --- Exception: " + e.Message);
            }

            return strResult;
        }
    }
}