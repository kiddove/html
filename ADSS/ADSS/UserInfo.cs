/*****************************************************************************
Copyright: 
File name: UserInfo.cs
Description: receive a POST request from a client, insert data into table
Author: paul
Version: 1.0
Date: 04/05/2015
History: 
 * 1. create classed stored user information.
 * 2. first create FingerPrint class, stored the information upload by js, may need to store others uploaded by webpage
*****************************************************************************/
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Web;

namespace ADSS
{
    public class UserFingerPrint
    {
        // property
        public string token { get; set; }
        public string ip { get; set; }
        public string agent { get; set; }
        public string language { get; set; }
        public int color_depth { get; set; }
        public string screen_resolution { get; set; }
        // offset time zone
        public int time_zone { get; set; }
        public string platform { get; set; }
        public string device { get; set; }
        public string os { get; set; }

        public string country { get; set; }
        public string province { get; set; }
        public string province_code { get; set; }
        public string city { get; set; }
    }

    public class UserInfo
    {
        public UserFingerPrint fingerprint { get; set; }
        public UserInfo()
        {
            fingerprint = new UserFingerPrint();
        }
    }
}