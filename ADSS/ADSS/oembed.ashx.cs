using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace ADSS
{
    /// <summary>
    /// Summary description for oembed
    /// </summary>
    public class oembedhandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string strRet = "wrong request";
            // https://vimeo.com/api/oembed.json?url=http%3A%2F%2Fvimeo.com%2F7100569
            oembed tb = new oembed();
            tb.type = "video";
            tb.version = "1.0";
            tb.provider_name = "Vimeo";
            tb.provider_url = "https://vimeo.com/";
            tb.title = "Brad!";
            tb.author_name = "Casey Donahue";
            tb.author_url = "https://vimeo.com/caseydonahue";
            tb.is_plus = "0";
            tb.html = "<iframe frameborder=\"0\" width=\"660\" height=\"380\" src=\"http://www.wyslink.com/emshow.aspx?id=cnRtcDovLzIwNi4xOTAuMTMzLjE0MC92b2QvbXA0Omx1Y3lrMzkzX2dtYWlsX2NvbS8xNDM5OTE5MTY5MzYxLm1wNA==&wys=NjQwOzM2MDtUcnVlO0ZhbHNlO0ZhbHNlO1RydWU7RmFsc2U7MjA7\"></iframe>";
            tb.width = 1280;
            tb.height = 720;
            tb.duration = 118;
            tb.description = "Brad finally gets the attention he deserves.";
            tb.thumbnail_url = "https://i.vimeocdn.com/video/29412830_1280.webp";
            tb.thumbnail_width = 1280;
            tb.thumbnail_height = 720;
            tb.video_id = 7100569;
            tb.uri = "/videos/7100569";

            //// http://flickr.com/photos/bees/2362225867
            //oembed tb = new oembed();
            //tb.type = "photo";
            //tb.flickr_type = "photo";
            //tb.author_name = "bees‬";
            //tb.author_url = "https://www.flickr.com/photos/bees/";
            //tb.width = 1280;
            //tb.height = 768;
            //tb.url = "https://farm4.staticflickr.com/3040/2362225867_4a87ab8baf_b.jpg";
            //tb.web_page = "https://www.flickr.com/photos/bees/2362225867/";
            //tb.thumbnail_url = "https://farm4.staticflickr.com/3040/2362225867_4a87ab8baf_q.jpg";
            //tb.thumbnail_width = 150;
            //tb.thumbnail_height = 150;
            //tb.web_page_short_url = "https://flic.kr/p/4AK2sc";
            //tb.license = "All Rights Reserved";
            //tb.license_id = 0;
            //tb.html = "<a data-flickr-embed=\"true\" href=\"https://www.flickr.com/photos/bees/2362225867/\" title=\"Bacon Lollys by ‮‭‬bees‬, on Flickr\"><img src=\"https://farm4.staticflickr.com/3040/2362225867_4a87ab8baf_b.jpg\" width=\"1024\" height=\"768\" alt=\"Bacon Lollys\"></a><script async src=\"https://embedr.flickr.com/assets/client-code.js\" charset=\"utf-8\"></script>";
            //tb.version = "1.0";
            //tb.cache_age = 3600;
            //tb.provider_name = "Flickr";
            //tb.provider_url = "https://www.flickr.com/";


            // parse request params
            string param_url, param_format = "json", param_maxwidth, param_maxheight;
            if (context.Request.QueryString["url"] != null)
                param_url = context.Request.QueryString["url"];
            else
                param_url = "";
            if (context.Request.QueryString["format"] != null)
                param_format = context.Request.QueryString["format"];
            else
                param_format = "json";
            if (context.Request.QueryString["maxwidth"] != null)
                param_maxwidth = context.Request.QueryString["maxwidth"];
            else
                param_maxwidth = "";
            if (context.Request.QueryString["maxheight"] != null)
                param_maxheight = context.Request.QueryString["maxheight"];
            else
                param_maxheight = "";

            if (!String.IsNullOrEmpty(param_url))
            {
                if (String.Equals(param_format, "xml", StringComparison.OrdinalIgnoreCase))
                {
                    XmlSerializer x = new XmlSerializer(tb.GetType());
                    using (StringWriter txtWriter = new Utf8StringWriter())
                    {
                        x.Serialize(txtWriter, tb);
                        strRet = txtWriter.ToString();
                    }
                    context.Response.ContentType = "text/xml";
                }
                else
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    strRet = js.Serialize(tb);
                    context.Response.ContentType = "application/json";
                }
            }
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
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

    public class oembed
    {
        public string type { get; set; }
        public string version { get; set; }
        public string provider_name { get; set; }
        public string provider_url { get; set; }
        public string title { get; set; }
        public string author_name { get; set; }
        public string author_url { get; set; }
        public string is_plus { get; set; }
        public string html { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int duration { get; set; }
        public string description { get; set; }
        public string thumbnail_url { get; set; }
        public int thumbnail_width { get; set; }
        public int thumbnail_height { get; set; }
        public int video_id { get; set; }
        public string uri { get; set; }
    }

    //public class oembed
    //{
    //    public string type { get; set; }
    //    public string flickr_type { get; set; }
    //    public string title { get; set; }
    //    public string author_name { get; set; }
    //    public string author_url { get; set; }
    //    public int width { get; set; }
    //    public int height { get; set; }
    //    public string url { get; set; }
    //    public string web_page { get; set; }
    //    public string thumbnail_url { get; set; }
    //    public int thumbnail_width { get; set; }
    //    public int thumbnail_height { get; set; }
    //    public string web_page_short_url { get; set; }
    //    public string license { get; set; }
    //    public int license_id { get; set; }
    //    public string html { get; set; }
    //    public string version { get; set; }
    //    public int cache_age { get; set; }
    //    public string provider_name { get; set; }
    //    public string provider_url { get; set; }
    //}

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}