/*****************************************************************************
Copyright: 
File name: Submit.ashx.cs
Description: receive a POST request from a client, insert data/update into table
Author: paul
Version: 1.0
Date: 04/28/2015
History: 
 * 1. create a connection pool or static connection to sqlserver, reading configure from .config;
 * 2. parse the json string to class object, then divide into several sql statements, use JavaScriptSerializer instead of DataContractJsonSerializer(need fx 4.5), much more easy.
 * 3. excute all sql statements in a transaction(connection must be opened within transaction scope)
 * 4. use id to distinguish insert and update, when update addional info, first delete. (DO NOT delete data from tb_ads_info, the id will be stored at other server)
 * N. try to cache the data
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.ApplicationBlocks.Data;

namespace ADSS
{
    /// <summary>
    /// Summary description for Submit
    /// </summary>
    public class Submit : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            int recordID = 0;
            bool bDel = false;
            if (context.Request.InputStream.Length > 0)
            {
                StreamReader oSR = new StreamReader(context.Request.InputStream);
                string str = oSR.ReadToEnd();
                oSR.Close();

                AdsInfo adi = Deserialize(str);
                if (adi != null)
                {
                    // write to table, divide into several parts(tables), excute in a transaction.!!!!!!!!!
                    // in order to run it correctly, connection must be opened within transanction scope!!!!!
                    // so NO DBConn is needed, using () will handle everything

                    // delete then insert? or select first
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string strConn = ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString;
                        try
                        {
                            using (SqlConnection sc = new SqlConnection(strConn))
                            {
                                // step1. insert into tb_ads_info, then select id , assign to variable for later use, assign camp_status = 0;
                                if (adi.id > 0)
                                {
                                    if (adi.camp_status == 255)
                                    {
                                        AdssLogger.WriteLog("Delete Record : " + adi.id.ToString());
                                        DeleteRecord(sc, adi);
                                        recordID = adi.id;
                                        bDel = true;
                                    }
                                    else
                                    {
                                        UpdateBasicInfo(sc, adi);
                                        recordID = adi.id;
                                    }
                                }
                                else
                                {
                                    recordID = InsertBasicInfo(sc, adi);
                                    adi.id = recordID;
                                }
                                // step2. insert multi tables
                                if (recordID > 0 && !bDel)
                                {
                                    // delete then insert
                                    DeleteAddionalInfo(sc, adi);
                                    InsertAddionalInfo(sc, adi);
                                }

                                ts.Complete();
                            }
                        }
                        catch (Exception ex)
                        {
                            recordID = 0;
                            //System.Diagnostics.Trace.WriteLine(ex.Message);
                            AdssLogger.WriteLog("Submit Exception: " + ex.Message);
                        }
                    }
                }
            }

            // return record id???
            context.Response.ContentType = "text/plain";
            context.Response.Write(recordID.ToString());
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private AdsInfo Deserialize(string strJson)
        {
            //// use DataContractJsonSerializer
            //DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(AdsInfo));
            //MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.UTF8.GetBytes(strJson));
            //try
            //{
            //    AdsInfo adif = (AdsInfo)js.ReadObject(ms);
            //    //System.Diagnostics.Trace.WriteLine("OK");
            //    MemoryStream stream1 = new MemoryStream();
            //    js.WriteObject(stream1, adif);
            //    stream1.Position = 0;
            //    StreamReader sr = new StreamReader(stream1);
            //    System.Diagnostics.Trace.WriteLine("JSON form of Person object: ");
            //    System.Diagnostics.Trace.WriteLine(sr.ReadToEnd());
            //}
            //catch (Exception e)
            //{
            //    System.Diagnostics.Trace.WriteLine(e.Message);
            //}
            //ms.Close();


            // use JavaScriptSerializer

            // deserialize
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                AdsInfo adi = js.Deserialize<AdsInfo>(strJson);

                ////// for test serialize
                //AdsInfo adi = new AdsInfo();
                //// basic info
                //adi.source = "http://localhost/demo/samples/kitteh.mp4";
                //adi.type_ads = 2;
                //adi.type_video_media = "video/mp4";
                //adi.click_reference = "http://www.wyslink.com";
                //adi.time_to_play_video_ads = 20;
                //adi.src_account = "kiddove@gmail.com";
                //adi.camp_start_date = "2015-04-28 00:00:00";
                //adi.camp_stop_date = "2016-04-28 00:00:00";
                //adi.camp_status = 1;

                //// addtional info
                //// network
                //adi.tgt_network.network.Add("Rogers");
                //adi.tgt_network.network.Add("Bell");
                //// device
                //adi.tgt_device.device.Add("all");
                //// language
                //adi.tgt_language.language.Add("mandarin");
                //adi.tgt_language.language.Add("english");
                //// location
                //ProvinceInfo pi = new ProvinceInfo();
                //pi.city.Add("toronto");
                //pi.city.Add("markham");
                //pi.city.Add("richmondhill");
                //pi.name = "ontario";
                //CountryInfo ci = new CountryInfo();
                //ci.province.Add(pi);
                //ci.name = "canada";

                //ProvinceInfo pi2 = new ProvinceInfo();
                //pi2.name = "quebec";
                //pi2.city.Add("ottawa");
                //pi2.city.Add("montreal");
                //ci.province.Add(pi2);
                //adi.tgt_location.country.Add(ci);

                //ProvinceInfo pi1 = new ProvinceInfo();
                //pi1.city.Add("beijing");
                //pi1.name = "beijing";
                //CountryInfo ci1 = new CountryInfo();
                //ci1.province.Add(pi1);
                //ci1.name = "china";
                //adi.tgt_location.country.Add(ci1);


                //string strResult = js.Serialize(adi);
                //System.Diagnostics.Trace.WriteLine(strResult);

                // json for location example
                // {"country":[{"province":[{"city":["toronto"],"name":"ontario"}],"name":"canada"}]}
                return adi;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("Submit Deserialize() Exception: " + e.Message);
                return null;
            }
        }

        // insert into tb_ads_info
        private int InsertBasicInfo(SqlConnection sc, AdsInfo adi)
        {
            int recordID = 0;
            if (sc != null)
            {
                if (string.IsNullOrEmpty(adi.source))
                {
                    AdssLogger.WriteLog("Invalid source: source can NOT be null or \"\"");
                    return recordID;
                }
                // first insert
                // sql 
                // insert into tb_ads_info (source, type_ads, type_video_media, click_reference, time_to_play_video_ads, src_account, camp_start_date, camp_stop_date, camp_status,) value ()
                string strInsert = String.Format("insert into tb_ads_info (source, type_ads, type_video_media, click_reference, time_to_play_video_ads, src_account, camp_start_date, camp_stop_date, camp_status, campaign_name) values ('{0}', {1}, '{2}', '{3}', {4}, '{5}', '{6}', '{7}', {8}, '{9}')",
                    adi.source, adi.type_ads, adi.type_video_media, adi.click_reference, adi.time_to_play_video_ads, adi.src_account, adi.camp_start_date, adi.camp_stop_date, adi.camp_status, adi.campaign_name);
                try
                {
                    SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strInsert);

                    // then select id for later use
                    string strSelect = String.Format("select id from tb_ads_info where source = '{0}' and type_ads = {1}", adi.source, adi.type_ads);

                    using (DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSelect))
                    {
                        if (dt != null && dt.Tables.Count == 1 && dt.Tables[0].Rows.Count == 1)
                            recordID = Convert.ToInt32(dt.Tables[0].Rows[0][0]);
                    }
                }
                catch (Exception e)
                {
                    //System.Diagnostics.Trace.WriteLine(e.Message);
                    throw e;
                }
            }
            return recordID;
        }

        // insert into other tables
        //private void InsertAddionalInfo(SqlConnection sc, AdsInfo adi, int recordID)
        private void InsertAddionalInfo(SqlConnection sc, AdsInfo adi)
        {
            if (sc != null)
            {
                try
                {
                    string strSQL;
                    // network
                    if (adi.tgt_network != null && adi.tgt_network.network != null)
                    {
                        foreach (string strNetwork in adi.tgt_network.network)
                        {
                            strSQL = String.Format("insert into dbo.tb_tgt_network (id, network) values ({0}, '{1}')", adi.id, strNetwork);
                            //strSQL = String.Format("insert into tb_tgt_network (id, network_id) values ({0}, (select network_id from tb_network where network = '{1}'))", adi.id, strNetwork);
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                    }
                    // device
                    if (adi.tgt_device != null && adi.tgt_device.device != null)
                    {
                        foreach (string strDevice in adi.tgt_device.device)
                        {
                            strSQL = String.Format("insert into dbo.tb_tgt_device (id, device) values ({0}, '{1}')", adi.id, strDevice);
                            //strSQL = String.Format("insert into tb_tgt_device (id, device_id) values ({0}, (select device_id from tb_device where device = '{1}'))", adi.id, strDevice);
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                    }
                    // language
                    if (adi.tgt_language != null && adi.tgt_language.language != null)
                    {
                        foreach (string strLanguage in adi.tgt_language.language)
                        {
                            strSQL = String.Format("insert into dbo.tb_tgt_language (id, language) values ({0}, '{1}')", adi.id, strLanguage);
                            //strSQL = String.Format("insert into tb_tgt_language (id, language_id) values ({0}, (select language_id from tb_language where language = '{1}'))", adi.id, strLanguage);
                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                    }
                    // location
                    if (adi.tgt_location != null && adi.tgt_location.location != null)
                    {
                        foreach (GeoLocation gl in adi.tgt_location.location)
                        {
                            // insert into dbo.tgt_location values ()
                            //string strCode = String.Format("(select right('000' + CAST((select country_id from tb_country where country = '{0}') as varchar(3)), 3) + right('000' + cast((select province_id from tb_province where province = '{1}') as varchar(3)), 3) + right('000' + cast((select city_id from tb_city where city = '{2}') as varchar(3)), 3))", ci.name, pi.name, cName);
                            strSQL = String.Format("insert into dbo.tb_tgt_location (id, country, province_code, city) values ({0}, '{1}', '{2}', '{3}')", adi.id, gl.country_name, gl.region_code, gl.city);

                            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                        }
                    }
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //if (adi.tgt_location != null && adi.tgt_location.country != null)
                    //{
                    //    foreach (CountryInfo ci in adi.tgt_location.country)
                    //    {
                    //        if (ci.province != null && ci.province.Count > 0)
                    //        {
                    //            foreach (ProvinceInfo pi in ci.province)
                    //            {
                    //                if (pi.city != null && pi.city.Count > 0)
                    //                {
                    //                    foreach (string cName in pi.city)
                    //                    {
                    //                        // insert into dbo.tgt_location values ()
                    //                        //string strCode = String.Format("(select right('000' + CAST((select country_id from tb_country where country = '{0}') as varchar(3)), 3) + right('000' + cast((select province_id from tb_province where province = '{1}') as varchar(3)), 3) + right('000' + cast((select city_id from tb_city where city = '{2}') as varchar(3)), 3))", ci.name, pi.name, cName);
                    //                        strSQL = String.Format("insert into dbo.tb_tgt_location (id, country, province, city) values ({0}, '{1}', '{2}', '{3}')", adi.id, ci.name, pi.name, cName);

                    //                        SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    strSQL = String.Format("insert into dbo.tb_tgt_location (id, country, province, city) values ({0}, '{1}', '{2}', '{3}')", adi.id, ci.name, pi.name, "all");

                    //                    SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            strSQL = String.Format("insert into dbo.tb_tgt_location (id, country, province, city) values ({0}, '{1}', '{2}', '{3}')", adi.id, ci.name, "all", "all");

                    //            SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                    //        }
                    //    }
                    //}
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // bid info
                    if (adi.ads_bid != null)//adi.ads_bid.price > 0 && !string.IsNullOrEmpty(adi.ads_bid.describe) && adi.ads_bid.budget > 0)
                    {
                        foreach (BidInfo bi in adi.ads_bid)
                        {
                            if (bi.price > 0 && !string.IsNullOrEmpty(bi.describe) && bi.budget > 0)
                            // id, price, bude
                            {
                                strSQL = String.Format("insert into dbo.tb_bid_info (id, budget, price, describe) values ({0}, {1}, {2}, '{3}')", adi.id, bi.budget, bi.price, bi.describe);

                                SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                            }
                        }
                    }

                    // keyword
                    if (adi.keyword != null && adi.keyword.Count > 0)
                    {
                        foreach (string tag in adi.keyword)
                        {
                            if (!String.IsNullOrEmpty(tag))
                            {
                                strSQL = String.Format("insert into dbo.tb_keyword_info (id, keyword) values ({0}, '{1}')", adi.id, tag);

                                SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private void UpdateBasicInfo(SqlConnection sc, AdsInfo adi)
        {
            if (sc != null)
            {
                // first insert
                // sql 
                try
                {
                    // insert into tb_ads_info (source, type_ads, type_video_media, click_reference, time_to_play_video_ads, src_account, camp_start_date, camp_stop_date, camp_status, campaign_name) value ()
                    string strSQL = String.Format("Update tb_ads_info set type_video_media = '{0}', click_reference = '{1}', time_to_play_video_ads = {2}, src_account = '{3}', camp_start_date = '{4}', camp_stop_date = '{5}', camp_status = {6}, campaign_name = '{7}', multstream = '{8}' where id = {9};",
                        adi.type_video_media, adi.click_reference, adi.time_to_play_video_ads, adi.src_account, adi.camp_start_date, adi.camp_stop_date, adi.camp_status, adi.campaign_name, adi.multstream, adi.id);
                    SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                }
                catch (Exception e)
                {
                    //System.Diagnostics.Trace.WriteLine(e.Message);
                    throw e;
                }
            }
        }

        private void DeleteAddionalInfo(SqlConnection sc, AdsInfo adi)
        {
            if (sc != null)
            {
                try
                {
                    string strSQL = String.Format("delete from tb_tgt_network where id = {0};delete from tb_tgt_device where id = {0};delete from tb_tgt_language where id = {0};delete from tb_tgt_location where id = {0};delete from tb_bid_info where id = {0};delete from tb_keyword_info where id = {0};", adi.id);
                    SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private void DeleteRecord(SqlConnection sc, AdsInfo adi)
        {
            if (sc != null)
            {
                try
                {
                    string strSQL = String.Format("delete from tb_ads_info where id = {0};delete from tb_tgt_network where id = {0};delete from tb_tgt_device where id = {0};delete from tb_tgt_language where id = {0};delete from tb_tgt_location where id = {0};delete from tb_bid_info where id = {0};delete from tb_keyword_info where id = {0};", adi.id);
                    SqlHelper.ExecuteNonQuery(sc, CommandType.Text, strSQL);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}