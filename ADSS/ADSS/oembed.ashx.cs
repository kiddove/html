using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ADSS
{
    /// <summary>
    /// Summary description for oembed
    /// </summary>
    public class oembed : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            testObj tb = new testObj();
            tb.type = "video";
            tb.version = "1.0";
            tb.provider_name = "Vimeo";
            tb.provider_url = @"https:\/\/vimeo.com\/";
            tb.title = "Brad!";
            tb.author_name = "Casey Donahue";
            tb.author_url = @"https:\/\/vimeo.com\/caseydonahue";
            tb.is_plus = "0";
            tb.html = "<iframe src=\"https:\\/\\/player.vimeo.com\\/video\\/7100569\" width=\"1280\" height=\"720\" frameborder=\"0\" title=\"Brad!\" webkitallowfullscreen mozallowfullscreen allowfullscreen><\\/iframe>";
            tb.width = 1280;
            tb.height = 720;
            tb.duration = 118;
            tb.description = "Brad finally gets the attention he deserves.";
            tb.thumbnail_url = @"https:\/\/i.vimeocdn.com\/video\/29412830_1280.webp";
            tb.thumbnail_width = 1280;
            tb.thumbnail_height = 720;
            tb.video_id = 7100569;
            tb.uri = @"\/videos\/7100569";

            JavaScriptSerializer js = new JavaScriptSerializer();
            string strJson = js.Serialize(tb);


            string strRet = "{\"author_name\": \"ZackScott\", \"version\": \"1.0\", \"width\": 480, \"type\": \"video\", \"provider_url\": \"https:\\/\\/www.youtube.com\\/\", \"height\": 270, \"author_url\": \"https:\\/\\/www.youtube.com\\/user\\/ZackScott\", \"thumbnail_width\": 480, \"provider_name\": \"YouTube\", \"thumbnail_height\": 360, \"html\": \"\\u003ciframe width=\\\"480\\\" height=\\\"270\\\" src=\\\"https:\\/\\/www.youtube.com\\/embed\\/M3r2XDceM6A?feature=oembed\\\" frameborder=\\\"0\\\" allowfullscreen\\u003e\\u003c\\/iframe\\u003e\", \"title\": \"Amazing Nintendo Facts\", \"thumbnail_url\": \"https:\\/\\/i.ytimg.com\\/vi\\/M3r2XDceM6A\\/hqdefault.jpg\"}";

            context.Response.ContentType = "text/plain";
            context.Response.Write(strRet);
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public class testObj
    {
        public string type { get; set; }
        public string version {get; set;}
        public string provider_name {get; set;}
        public string provider_url {get; set;}
        public string title {get; set;}
        public string author_name {get; set;}
        public string author_url {get; set;}
        public string is_plus {get; set;}
        public string html {get; set;}
        public int width {get; set;}
        public int height {get; set;}
        public int duration {get; set;}
        public string description {get; set;}
        public string thumbnail_url {get; set;}
        public int thumbnail_width {get; set;}
        public int thumbnail_height {get; set;}
        public int video_id {get; set;}
        public string uri {get; set;}

    }
}