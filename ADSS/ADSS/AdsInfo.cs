/*****************************************************************************
Copyright: 
File name: AdsInfo.cs
Description: store an Ads object, corresponding to the fields in the database table
Author: paul
Version: 1.0
Date: 04/28/2015
History: 
 * 1. define the property, prepare for the json serialize and deserialize
 * 2. define specifc class
*****************************************************************************/
using System;
using System.Collections.Generic;

namespace ADSS
{
    //[DataContract]
    public class AdsInfo
    {
        // id --- the id of the ads, unique in table, AUTO Increacement
        //[DataMember(Name = "id")]
        public int id { get; set; }

        // source --- the video/ad url
        //[DataMember(Name = "source")]
        public string source { get; set; }

        // type_ads --- type of ads, preroll stream(1), midroll stream(2), display big(3), display small(4)
        //[DataMember(Name = "type_ads")]
        public byte type_ads { get; set; }

        // type_video_media --- if is stream video, the video type, eg: video/mp4
        //[DataMember(Name = "type_video_media")]
        public string type_video_media { get; set; }

        // click_reference --- the reference if ads is clicked
        //[DataMember(Name = "click_reference")]
        public string click_reference { get; set; }

        // time_to_play_video_ads --- MIDROLL ONLY,  when to play midroll ads
        //[DataMember(Name = "time_to_play_video_ads")]
        public int time_to_play_video_ads { get; set; }

        // src_account --- ads belonging to which account
        //[DataMember(Name = "src_account")]
        public string src_account { get; set; }

        // camp_start_date --- start time of play this ad in webpage
        //[DataMember(Name = "camp_start_date")]
        public string camp_start_date { get; set; }

        // camp_stop_date --- stop time of play this ad in webpage
        //[DataMember(Name = "camp_stop_date")]
        public string camp_stop_date { get; set; }

        // camp_status --- running(1), pause(2), stop(0), end(also 0?)
        //[DataMember(Name = "camp_status")]
        public byte camp_status { get; set; }

        //// tgt_location 
        //[DataMember(Name = "tgt_location")]
        public LocationInfo tgt_location { get; set; }

        // tgt_network --- wyslink, wyslink+affiliate ???
        //[DataMember(Name = "tgt_network")]
        public NetworkInfo tgt_network { get; set; }

        // tgt_device --- pc(1), mobile(2), tablet(3)
        //[DataMember(Name = "tgt_device")]
        public DeviceInfo tgt_device { get; set; }

        // tgt_language -- en(1), ch(2), jp(3)
        //[DataMember(Name = "tgt_language")]
        public LanguageInfo tgt_language { get; set; }

        public List<BidInfo> ads_bid { get; set; }

        public string campaign_name { get; set; }

        public List<string> keyword { get; set; }
        // constructor
        public AdsInfo()
        {
            tgt_location = new LocationInfo();
            tgt_network = new NetworkInfo();
            tgt_device = new DeviceInfo();
            tgt_language = new LanguageInfo();
            ads_bid = new List<BidInfo>();
            keyword = new List<string>();
            campaign_name = source = type_video_media = click_reference = src_account = camp_start_date = camp_stop_date = "";
        }

        public string multstream { get; set; }
    }
    ////[DataContract]
    //public class LocationInfo
    //{
    //    public class CountryInfo
    //    {
    //        public class ProvinceInfo
    //        {
    //            public string name { get; set; }
    //            //[DataMember(Name = "province")]
    //            public string[] city;
    //        }
    //        public string name { get; set; }
    //        //[DataMember(Name = "country")]
    //        public ProvinceInfo[] province;
    //    }
    //    //[DataMember(Name = "location")]
    //    public CountryInfo[] country;
    //}

    //public class CountryInfo
    //{
    //    public string name { get; set; }
    //    //[DataMember(Name = "country")]
    //    public ProvinceInfo[] province;
    //}

    //public class ProvinceInfo
    //{
    //    public string name { get; set; }
    //    //[DataMember(Name = "province")]
    //    public string[] city;
    //}

    public class BidInfo
    {
        public string describe { get; set; }
        //public int type { get; set; }
        public int price { get; set; }
        public int budget { get; set; }
        public BidInfo()
        {
            price = budget = 0;
            describe = "";
        }
    }

    // added by paul Jul.30, 2015
    // Now use as whitelist 
    // webpage will ge a whitelist of video v, which says v allow [a, b, c]'s ads playing on v.
    // so use [a, b, c] as condition search in database.
    public class NetworkInfo
    {
        public List<string> network { get; set; }
        public NetworkInfo()
        {
            network = new List<string>();
        }
    }

    public class DeviceInfo
    {
        public List<string> device { get; set; }
        public DeviceInfo()
        {
            device = new List<string>();
        }
    }

    public class LanguageInfo
    {
        public List<string> language { get; set; }
        public LanguageInfo()
        {
            language = new List<string>();
        }
    }

    public class LocationInfo
    {
        public List<GeoLocation> location { get; set; }

        public LocationInfo()
        {
            location = new List<GeoLocation>();
        }
    }

    //public class CountryInfo
    //{
    //    public string name { get; set; }
    //    public List<ProvinceInfo> province { get; set; }
    //    public CountryInfo()
    //    {
    //        province = new List<ProvinceInfo>();
    //        name = "";
    //    }
    //}

    public class GeoLocation
    {
        public string ip { get; set; }
        public string country_name { get; set; }
        public string region_name { get; set; }
        public string region_code { get; set; }
        public string city { get; set; }
        public GeoLocation()
        {
            ip = country_name = region_name = region_code = city = "";
        }
    }

    public class GeoLocation_IPAPI
    {
        public string As { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
        public float lat { get; set; }
        public float lon { get; set; }
        public string org { get; set; }
        public string query { get; set; }
        public string region { get; set; }
        public string regionName { get; set; }
        public string status { get; set; }
        public string timezone { get; set; }
        public string zip { get; set; }
    }

    public class ProvinceInfo
    {
        public string name { get; set; }
        public List<string> city { get; set; }

        public ProvinceInfo()
        {
            city = new List<string>();
            name = "";
        }
    }


    // for response
    // only these fields will be shown in http response json.
    public class resultInfo
    {
        // id --- the id of the ads, unique in table, AUTO Increacement
        //[DataMember(Name = "id")]
        public int id { get; set; }

        // source --- the video/ad url
        //[DataMember(Name = "source")]
        public string src { get; set; }

        // type_ads --- type of ads, preroll stream(1), midroll stream(2), display big(3), display small(4)
        //[DataMember(Name = "type_ads")]
        public byte ads_type { get; set; }

        // type_video_media --- if is stream video, the video type, eg: video/mp4
        //[DataMember(Name = "type_video_media")]
        public string type { get; set; }

        // click_reference --- the reference if ads is clicked
        //[DataMember(Name = "click_reference")]
        public string link { get; set; }

        // time_to_play_video_ads --- MIDROLL ONLY,  when to play midroll ads
        //[DataMember(Name = "time_to_play_video_ads")]
        public int ads_time { get; set; }

        public string multstream { get; set; }
    }

    public class StreamInfo
    {
        public List<resultInfo> pre { get; set; }
        public List<resultInfo> mid { get; set; }

        public StreamInfo()
        {
            pre = new List<resultInfo>();
            mid = new List<resultInfo>();
        }
    }

    public class DisplayInfo
    {
        public List<resultInfo> display_big { get; set; }
        public List<resultInfo> display_small { get; set; }
        public DisplayInfo()
        {
            display_big = new List<resultInfo>();
            display_small = new List<resultInfo>();
        }
    }

    public class GenericInfo
    {
        public List<resultInfo> pre { get; set; }
        public List<resultInfo> mid { get; set; }
        public List<resultInfo> display_big { get; set; }
        public List<resultInfo> display_small { get; set; }

        public GenericInfo()
        {
            pre = new List<resultInfo>();
            mid = new List<resultInfo>();
            display_big = new List<resultInfo>();
            display_small = new List<resultInfo>();
        }
    }

    // for retrieve
    public class RetrieveCondition
    {
        public List<int> id { get; set; }
        public List<string> src_account { get; set; }


        public RetrieveCondition()
        {
            id = new List<int>();
            src_account = new List<string>();
        }
    }

    // for getlist
    public class GetListCondition : RetrieveCondition
    {
        public List<string> source { get; set; }

        public List<byte> type_ads { get; set; }

        public List<string> type_video_media { get; set; }

        public List<string> click_reference { get; set; }

        public List<int> time_to_play_video_ads { get; set; }

        public List<string> camp_start_date { get; set; }

        public List<string> camp_stop_date { get; set; }

        public List<byte> camp_status { get; set; }

        public LocationInfo tgt_location { get; set; }

        public NetworkInfo tgt_network { get; set; }

        public DeviceInfo tgt_device { get; set; }

        public LanguageInfo tgt_language { get; set; }

        public List<string> token { get; set; }

        public List<BidInfo> bid_ads { get; set; }

        public List<string> campaign_name { get; set; }

        public GetListCondition()
        {
            source = new List<string>();
            type_ads = new List<byte>();
            type_video_media = new List<string>();
            click_reference = new List<string>();
            time_to_play_video_ads = new List<int>();
            camp_start_date = new List<string>();
            camp_stop_date = new List<string>();
            camp_status = new List<byte>();
            tgt_location = new LocationInfo();
            tgt_network = new NetworkInfo();
            tgt_device = new DeviceInfo();
            tgt_language = new LanguageInfo();
            token = new List<string>();
            bid_ads = new List<BidInfo>();
            campaign_name = new List<string>();
        }
    }

    public class ActionInfo
    {
        public int id { get; set; }
        public string token { get; set; }
        public byte action { get; set; }
        public string time { get; set; }
        public string ip { get; set; }
        // add 2 accounts name
        public string login_account { get; set; }
        public string video_account { get; set; }
    }
}