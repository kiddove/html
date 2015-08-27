/*****************************************************************************
Copyright: 
File name: GetList.ashx.cs
Description: receive a POST request from a client, return result in json format
Author: paul
Version: 1.0
Date: 04/28/2015
History: 
 * 1. create a connection pool or static connection to sqlserver, reading configure from .config;
 * 2. try to cache the data
 * 3. divide different request conditions(language, network, device, etc) into several sql statement, excute each sql get a id list
      merge all lists together, use final list to select in tb_ads_info
 * 4. parse parameter first, type(stream, display), language(english, mandarin, cantonese, etc), etc
 * 5. parse parameter in two ways, process different.
*****************************************************************************/
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
    /// Summary description for GetList1
    /// </summary>
    public class GetList : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.InputStream.Length > 0)
            {
                string strResult = "{}";
                try
                {
                    StreamReader oSR = new StreamReader(context.Request.InputStream);
                    string strJson = oSR.ReadToEnd();
                    oSR.Close();

                    //AdssLogger.WriteLog("GetList json parameter: " + strJson);
                    // deserialize, use the same structure as apply
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    GetListCondition glCon = null;
                    AdsInfo adi = null;
                    try
                    {
                        glCon = js.Deserialize<GetListCondition>(strJson);
                    }
                    catch (Exception ex)
                    {
                        string ss = ex.Message;
                        try
                        {
                            adi = js.Deserialize<AdsInfo>(strJson);
                        }
                        catch (Exception exAdi)
                        {
                            //System.Diagnostics.Trace.WriteLine(exAdi.Message);
                            AdssLogger.WriteLog("GetList Deserialize json Exception: " + exAdi.Message);
                        }
                    }
                    if (adi != null && adi.id > 0)
                    {
                        // return full info
                        FinalCondition fc = new FinalCondition();
                        fc.addlist.Add(adi.id);
                        FinalFilter ff = new FinalFilter(fc);
                        strResult = ff.GetFullResult();
                    }
                    else if (glCon != null)
                    {
                        // step1 addional condition, location, device, network, language, etc
                        // merge them together as one condition
                        // addtional conditions merge obey "and(&&)" rules
                        List<int> addList = new List<int>();

                        BasicCondition bc = new BasicCondition();
                        // 过滤 list
                        // 滤掉 “” 以及0
                        //glCon.id.FindAll()
                        //Array.FindAll(arrIpAddress, a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                        bc.id = glCon.id.FindAll(BasicCondition.ValidInt);
                        bc.source = glCon.source.FindAll(BasicCondition.ValidString);
                        bc.type_ads = glCon.type_ads.FindAll(BasicCondition.ValidByte);
                        bc.type_video_media = glCon.type_video_media.FindAll(BasicCondition.ValidString);
                        bc.click_reference = glCon.click_reference.FindAll(BasicCondition.ValidString);
                        bc.time_to_play_video_ads = glCon.time_to_play_video_ads.FindAll(BasicCondition.ValidInt);
                        bc.src_account = glCon.src_account.FindAll(BasicCondition.ValidString);
                        bc.camp_start_date = glCon.camp_start_date.FindAll(BasicCondition.ValidString);
                        bc.camp_stop_date = glCon.camp_stop_date.FindAll(BasicCondition.ValidString);
                        bc.camp_status = glCon.camp_status.FindAll(BasicCondition.ValidByte);
                        bc.campaign_name = glCon.campaign_name.FindAll(BasicCondition.ValidString);

                        BasicFilter bf = new BasicFilter(bc);
                        //if (addList.Count == 0)
                        //    addList = addList.Concat(bf.GetList()).ToList();
                        //else
                        addList = bf.GetList(null).ToList();

                        if (glCon.tgt_device.device.Count > 0)
                        {
                            DeviceCondition dc = new DeviceCondition();
                            dc.deviceinfo = glCon.tgt_device;

                            DeviceFilter df = new DeviceFilter(dc);
                            // use linq
                            //if (addList.Count == 0)
                            //    addList = addList.Concat(df.GetList()).ToList();
                            //else
                            //    addList = addList.Intersect(df.GetList()).ToList();
                            addList = addList.Concat(df.GetList(addList)).ToList();
                        }
                        // else
                        if (addList.Count > 0)
                        {
                            if (glCon.tgt_language.language.Count > 0)
                            {
                                LanguageCondition lc = new LanguageCondition();
                                lc.languageinfo = glCon.tgt_language;

                                LanguageFilter lf = new LanguageFilter(lc);
                                //if (addList.Count == 0)
                                //    addList = addList.Concat(lf.GetList()).ToList();
                                //else
                                addList = addList.Intersect(lf.GetList(addList)).ToList();
                            }
                            if (addList.Count > 0)
                            {
                                if (glCon.tgt_network.network.Count > 0)
                                {
                                    NetWorkCondition nc = new NetWorkCondition();
                                    nc.networkinfo = glCon.tgt_network;

                                    NetworkFilter nf = new NetworkFilter(nc);
                                    //if (addList.Count == 0)
                                    //    addList = addList.Concat(nf.GetList()).ToList();
                                    //else
                                    addList = addList.Intersect(nf.GetList(addList)).ToList();
                                }

                                if (addList.Count > 0)
                                {
                                    List<string> userToken = glCon.token.FindAll(BasicCondition.ValidString);
                                    if (userToken.Count == 1)
                                    {
                                        LocationInfo li = GetLocationInfoFromDB(userToken[0]);
                                        glCon.tgt_location = li;
                                    }

                                    ///////////////////////////////////////////////////////////
                                    //if (glCon.tgt_location.country.Count > 0)
                                    //{
                                    //    LocationCondition lc = new LocationCondition();
                                    //    lc.locationinfo = glCon.tgt_location;

                                    //    LocationFilter lf = new LocationFilter(lc);
                                    //    //if (addList.Count == 0)
                                    //    //    addList = addList.Concat(lf.GetList()).ToList();
                                    //    //else
                                    //    addList = addList.Intersect(lf.GetList()).ToList();
                                    //}
                                    ///////////////////////////////////////////////////////////

                                    if (glCon.tgt_location.location.Count > 0)
                                    {
                                        LocationCondition lc = new LocationCondition();
                                        lc.locationinfo = glCon.tgt_location;

                                        LocationFilter lf = new LocationFilter(lc);
                                        //if (addList.Count == 0)
                                        //    addList = addList.Concat(lf.GetList()).ToList();
                                        //else
                                        addList = addList.Intersect(lf.GetList(addList)).ToList();
                                    }

                                    // step2 basic condition + addtional conditon
                                    if (addList.Count > 0)
                                    {
                                        //BidCondition bc
                                        // to be continued...
                                        FinalCondition fc = new FinalCondition();
                                        fc.addlist = addList;
                                        FinalFilter ff = new FinalFilter(fc);
                                        strResult = ff.GetResult(null);
                                    }
                                }
                            }
                        }
                    }

                    context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(strResult);

                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Trace.WriteLine(ex.Message);
                    AdssLogger.WriteLog("GetList Exception: " + ex.Message);
                    context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(strResult);
                }
            }
            else
            {
                // test for dummy info

                DummyInfo di = new DummyInfo();
                di.account = "kiddove@gmail.com";
                di.viewer = "paul@kectech.com";
                di.whitelist.Add("masonluo@kectech.com");
                di.whitelist.Add("iris@wyslink.com");

                JavaScriptSerializer js = new JavaScriptSerializer();
                string strJson = js.Serialize(di);
                context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                context.Response.ContentType = "text/plain";
                context.Response.Write(strJson);
            }
        }

        public bool IsReusable
        {
            get
            {
                //return false;
                return true;
            }
        }

        private LocationInfo GetLocationInfoFromDB(string sToken)
        {
            LocationInfo li = new LocationInfo();
            string strIp = FingerPrint.GetVisitorIPAddress();
            if (string.IsNullOrEmpty(strIp) || string.IsNullOrEmpty(sToken))
                return li;
            try
            {
                //SqlConnection sc = DBConn.GetConnection();
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                //{
                //    if (sc != null)
                //    {
                //        string strSQL = String.Format("select country, province, city from dbo.tb_user_info where token = '{0}' and ip = '{1}'", sToken, strIp);
                //        DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                //        DataTable dtb = dt.Tables[0];

                //        if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count == 1)
                //        {
                //            DataRow r = dt.Tables[0].Rows[0];
                //            ProvinceInfo pi = new ProvinceInfo();
                //            pi.name = Convert.ToString(r[1]);
                //            pi.city.Add(Convert.ToString(r[2]));
                //            CountryInfo ci = new CountryInfo();
                //            ci.name = Convert.ToString(r[0]);
                //            ci.province.Add(pi);
                //            li.country.Add(ci);
                //        }
                //    }
                //    else
                //    {
                //        //Trace.WriteLine("can not get sql connection.");
                //        AdssLogger.WriteLog("GetList.GetLocationInfoFromDB() --- can not get sql connection.");
                //    }
                //}
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                using (SqlConnection sc = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    if (sc != null)
                    {
                        string strSQL = String.Format("select country, province, city, province_code from dbo.tb_user_info where token = '{0}' and ip = '{1}'", sToken, strIp);
                        DataSet dt = SqlHelper.ExecuteDataset(sc, CommandType.Text, strSQL);
                        DataTable dtb = dt.Tables[0];

                        if (dt != null && dt.Tables.Count > 0 && dt.Tables[0].Rows.Count == 1)
                        {
                            DataRow r = dt.Tables[0].Rows[0];
                            GeoLocation gl = new GeoLocation();

                            
                            gl.country_name = Convert.ToString(r[0]);
                            gl.region_name = Convert.ToString(r[1]);
                            gl.city = Convert.ToString(r[2]);
                            gl.region_code = Convert.ToString(r[3]);
                            li.location.Add(gl);
                        }
                    }
                    else
                    {
                        //Trace.WriteLine("can not get sql connection.");
                        AdssLogger.WriteLog("GetList.GetLocationInfoFromDB() --- can not get sql connection.");
                    }
                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message);
                AdssLogger.WriteLog("Getlist.GetLocationInfoFromDB() --- Exception: " + e.Message);
            }
            return li;
        }
    }
}