/*****************************************************************************
Copyright: 
File name: Condition.cs
Description: interface and implement of conditions, used for getting condtions of sql statement.
Author: paul
Version: 1.0
Date: 04/30/2015
History: 
 * 1. create interface and several implements, including language, network, location, device, etc
*****************************************************************************/
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Web;

namespace ADSS
{
    public interface ICondition
    {
        string GetCondition(List<int> ids);
    }

    // language
    public class LanguageCondition : ICondition
    {
        public LanguageInfo languageinfo { get; set; }
        public virtual string GetCondition(List<int> ids)
        {
            string strRet = "";
            //if (m_language.language.Count == 1)
            //{
            //    strRet = String.Format("= '{0}'", m_language.language[0]);
            //}
            //else if (m_language.language.Count > 1)
            //{
            strRet = "in (";
            foreach (string str in languageinfo.language)
            {
                strRet += "'" + str + "', ";
            }

            //strRet = strRet.Substring(0, strRet.Length - 2);
            strRet += "'all')";
            //}

            if (ids != null && ids.Count > 0)
            {
                strRet += " and id in (";
                foreach (int id in ids)
                {
                    strRet += Convert.ToString(id) + ", ";
                }
                strRet = strRet.Substring(0, strRet.Length - 2);
                strRet += ")";
            }
            return strRet;
        }

        // constructor
        public LanguageCondition()
        {
            languageinfo = new LanguageInfo();
        }
    }

    // device
    public class DeviceCondition : ICondition
    {
        public DeviceInfo deviceinfo { get; set; }
        public virtual string GetCondition(List<int> ids)
        {
            string strRet = "";
            //if (m_device.device.Count == 1)
            //{
            //    strRet = String.Format("= '{0}'", m_device.device[0]);
            //}
            //else if (m_device.device.Count > 1)
            //{
            strRet = "in (";
            foreach (string str in deviceinfo.device)
            {
                strRet += "'" + str + "', ";
            }

            //strRet = strRet.Substring(0, strRet.Length - 2);
            strRet += "'all')";
            //}
            if (ids != null && ids.Count > 0)
            {
                strRet += " and id in (";
                foreach (int id in ids)
                {
                    strRet += Convert.ToString(id) + ", ";
                }
                strRet = strRet.Substring(0, strRet.Length - 2);
                strRet += ")";
            }
            return strRet;
        }

        // constructor
        public DeviceCondition()
        {
            deviceinfo = new DeviceInfo();
        }
    }

    // network
    public class NetWorkCondition : ICondition
    {
        public NetworkInfo networkinfo { get; set; }
        public virtual string GetCondition(List<int> ids)
        {
            string strRet = "";
            //if (m_network.network.Count == 1)
            //{
            //    strRet = String.Format("= '{0}'", m_network.network[0]);
            //}
            //else if (m_network.network.Count > 1)
            //{
            strRet = "in (";
            foreach (string str in networkinfo.network)
            {
                if (!String.IsNullOrEmpty(str))
                    strRet += "'" + str + "', ";
            }

            strRet = strRet.Substring(0, strRet.Length - 2);
            strRet += ")";
            //}

            if (ids != null && ids.Count > 0)
            {
                strRet += " and id in (";
                foreach (int id in ids)
                {
                    strRet += Convert.ToString(id) + ", ";
                }
                strRet = strRet.Substring(0, strRet.Length - 2);
                strRet += ")";
            }

            return strRet;
        }

        // constructor
        public NetWorkCondition()
        {
            networkinfo = new NetworkInfo();
        }
    }

    // location
    public class LocationCondition : ICondition
    {
        public LocationInfo locationinfo { get; set; }
        //public virtual string GetCondition()
        //{
        //    // where {0}
        //    // country = and province= city= or ()
        //    string strRet = "";
        //    strRet = "(country = 'all')";
        //    if (locationinfo != null && locationinfo.country != null)
        //    {
        //        foreach (CountryInfo ci in locationinfo.country)
        //        {
        //            //string strTemp = String.Format("(select right('000' + CAST((select country_id from tb_country where country = '{0}') as varchar(3)), 3) + right('000' + cast((select province_id from tb_province where province = '{1}') as varchar(3)), 3) + right('000' + cast((select city_id from tb_city where city = '{2}') as varchar(3)), 3))", ci.name, "all", "all");
        //            string strTemp = String.Format(" or (country = '{0}')", ci.name);
        //            strRet += strTemp;
        //            if (ci.province != null)
        //            {
        //                foreach (ProvinceInfo pi in ci.province)
        //                {
        //                    //strTemp = String.Format("(select right('000' + CAST((select country_id from tb_country where country = '{0}') as varchar(3)), 3) + right('000' + cast((select province_id from tb_province where province = '{1}') as varchar(3)), 3) + right('000' + cast((select city_id from tb_city where city = '{2}') as varchar(3)), 3))", ci.name, pi.name, "all");
        //                    strTemp = String.Format(" or (country = '{0}' and province = '{1}')", ci.name, pi.name);
        //                    strRet += strTemp;
        //                    if (pi.city != null)
        //                    {
        //                        foreach (string cName in pi.city)
        //                        {
        //                            //strTemp = String.Format("(select right('000' + CAST((select country_id from tb_country where country = '{0}') as varchar(3)), 3) + right('000' + cast((select province_id from tb_province where province = '{1}') as varchar(3)), 3) + right('000' + cast((select city_id from tb_city where city = '{2}') as varchar(3)), 3))", ci.name, pi.name, cName);
        //                            strTemp = String.Format(" or (country = '{0}' and province = '{1}' and city = '{2}')", ci.name, pi.name, cName);
        //                            strRet += strTemp;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    //if (arrLocation.Length == 1)
        //    //{
        //    ////    strRet = String.Format("= '{0}'", strLang[0]);
        //    //}
        //    //else if (arrLocation.Length > 1)
        //    //{
        //    //    strRet = "in (";
        //    //    foreach (LocationInfo li in arrLocation)
        //    //    {
        //    ////        strRet += "'" + str + "', ";
        //    //    }

        //    //    strRet = strRet.Substring(0, strRet.Length - 2);
        //    //    strRet += ")";
        //    //}
        //    return strRet;
        //}

        // constructor

        public virtual string GetCondition(List<int> ids)
        {
            // where {0}
            // country = and province= city= or ()
            string strRet = "";
            
            if (locationinfo != null && locationinfo.location.Count > 0)
            {
                strRet = "((country = 'all')";
                foreach (GeoLocation gl in locationinfo.location)
                {
                    //string strTemp = String.Format("(select right('000' + CAST((select country_id from tb_country where country = '{0}') as varchar(3)), 3) + right('000' + cast((select province_id from tb_province where province = '{1}') as varchar(3)), 3) + right('000' + cast((select city_id from tb_city where city = '{2}') as varchar(3)), 3))", ci.name, "all", "all");
                    //string strTemp = String.Format(" or (country = '{0}' and province = '{1}' and city = '{2}')", gl.country_name, gl.region_name, gl.city);
                    //string strTemp = String.Format(" or (country = '{0}' and province = 'all') or (country = '{0}' and province = '{1}' and city = 'all') or (country = '{0}' and province = '{1}' and city = '{2}')", gl.country_name, gl.region_name, gl.city);
                    string strTemp = String.Format(" or (country = '{0}' and province_code = 'all') or (country = '{0}' and province_code = '{1}' and city = 'all') or (country = '{0}' and province_code = '{1}' and city = '{2}')", gl.country_name, gl.region_code, gl.city);
                    strRet += strTemp;
                }
                strRet += ")";
            }

            if (ids != null && ids.Count > 0)
            {
                strRet += " and id in (";
                foreach (int id in ids)
                {
                    strRet += Convert.ToString(id) + ", ";
                }
                strRet = strRet.Substring(0, strRet.Length - 2);
                strRet += ")";
            }

            return strRet;
        }

        public LocationCondition()
        {
            locationinfo = new LocationInfo();
        }
    }
    // bind
    //public class BidCondition 
    // basic
    public class BasicCondition : ICondition
    {
        public List<int> id { get; set; }

        public List<string> source { get; set; }

        public List<byte> type_ads { get; set; }

        public List<string> type_video_media { get; set; }

        public List<string> click_reference { get; set; }

        public List<int> time_to_play_video_ads { get; set; }

        public List<string> src_account { get; set; }

        public List<string> camp_start_date { get; set; }

        public List<string> camp_stop_date { get; set; }

        public List<byte> camp_status { get; set; }

        public List<string> campaign_name { get; set; }

        public BasicCondition()
        {
            id = new List<int>();
            source = new List<string>();
            type_ads = new List<byte>();
            type_video_media = new List<string>();
            click_reference = new List<string>();
            time_to_play_video_ads = new List<int>();
            src_account = new List<string>();
            camp_start_date = new List<string>();
            camp_stop_date = new List<string>();
            camp_status = new List<byte>();
            campaign_name = new List<string>();
        }
        public virtual string GetCondition(List<int> ids)
        {
            /// XXX where 
            string strRet = "";
            // id
            if (id != null && id.Count > 0)
            {
                strRet += "and id ";
                if (id.Count == 1)
                {
                    strRet += String.Format("= {0}", id[0]);
                }
                else if (id.Count > 1)
                {
                    strRet += "in (";
                    foreach (int sid in id)
                    {
                        strRet += sid.ToString() + ", ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }
            // source
            if (source != null && source.Count > 0)
            {
                strRet += "and source ";
                if (source.Count == 1)
                {
                    strRet += String.Format("= '{0}'", source[0]);
                }
                else if (source.Count > 1)
                {
                    strRet += "in (";
                    foreach (string str in source)
                    {
                        strRet += "'" + str + "', ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }
            // type_ads
            if (type_ads != null && type_ads.Count > 0)
            {
                strRet += "and type_ads ";
                if (type_ads.Count == 1)
                {
                    strRet += String.Format("= {0}", type_ads[0]);
                }
                else if (type_ads.Count > 1)
                {
                    strRet += "in (";
                    foreach (byte sid in type_ads)
                    {
                        strRet += sid.ToString() + ", ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }
            // type_video_media
            if (type_video_media != null && type_video_media.Count > 0)
            {
                strRet += "and type_video_media ";
                if (type_video_media.Count == 1)
                {
                    strRet += String.Format("= '{0}'", type_video_media[0]);
                }
                else if (type_video_media.Count > 1)
                {
                    strRet += "in (";
                    foreach (string str in type_video_media)
                    {
                        strRet += "'" + str + "', ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }
            // click_reference
            if (click_reference != null && click_reference.Count > 0)
            {
                strRet += "and click_reference ";
                if (click_reference.Count == 1)
                {
                    strRet += String.Format("= '{0}'", click_reference[0]);
                }
                else if (click_reference.Count > 1)
                {
                    strRet += "in (";
                    foreach (string str in click_reference)
                    {
                        strRet += "'" + str + "', ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }
            // time_to_play_video_ads
            if (time_to_play_video_ads != null && time_to_play_video_ads.Count > 0)
            {
                strRet += "and time_to_play_video_ads ";
                if (time_to_play_video_ads.Count == 1)
                {
                    strRet += String.Format("= {0}", time_to_play_video_ads[0]);
                }
                else if (time_to_play_video_ads.Count > 1)
                {
                    strRet += "in (";
                    foreach (byte sid in time_to_play_video_ads)
                    {
                        strRet += sid.ToString() + ", ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }
            // src_account
            if (src_account != null && src_account.Count > 0)
            {
                strRet += "and src_account ";
                if (src_account.Count == 1)
                {
                    strRet += String.Format("= '{0}'", src_account[0]);
                }
                else if (src_account.Count > 1)
                {
                    strRet += "in (";
                    foreach (string str in src_account)
                    {
                        strRet += "'" + str + "', ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }
            // camp_start_date
            // camp_stop_date
            // these two is different...
            // tobe continued...

            // camp_status
            if (camp_status != null && camp_status.Count > 0)
            {
                strRet += "and camp_status ";
                if (camp_status.Count == 1)
                {
                    strRet += String.Format("= {0}", camp_status[0]);
                }
                else if (camp_status.Count > 1)
                {
                    strRet += "in (";
                    foreach (byte sid in camp_status)
                    {
                        strRet += sid.ToString() + ", ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }

            // campaign_name
            if (campaign_name != null && campaign_name.Count > 0)
            {
                strRet += "and campaign_name ";
                if (campaign_name.Count == 1)
                {
                    strRet += String.Format("= '{0}'", campaign_name[0]);
                }
                else if (campaign_name.Count > 1)
                {
                    strRet += "in (";
                    foreach (string str in campaign_name)
                    {
                        strRet += "'" + str + "', ";
                    }

                    strRet = strRet.Substring(0, strRet.Length - 2);
                    strRet += ")";
                }
            }

            return strRet;
        }

        public static bool ValidInt(int i)
        {
            if (i > 0)
                return true;
            else
                return false;
        }
        public static bool ValidByte(byte i)
        {
            if (i > 0)
                return true;
            else
                return false;
        }
        public static bool ValidString(string i)
        {
            if (string.IsNullOrEmpty(i))
                return false;
            else
                return true;
        }
    }

    // together
    public class FinalCondition : ICondition
    {
        public List<int> addlist { get; set; }

        public FinalCondition()
        {
            addlist = new List<int>();
        }
        public virtual string GetCondition(List<int> ids)
        {
            string strRet = "";
            if (addlist.Count > 0)
            {
                strRet += "in (";
                foreach (int id in addlist)
                {
                    strRet += Convert.ToString(id) + ", ";
                }
                strRet = strRet.Substring(0, strRet.Length - 2);
                strRet += ")";
            }

            return strRet;
        }
    }
}