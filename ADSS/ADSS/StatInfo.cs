using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSS
{
    //class StatInfo
    //{
    //}
    public class Charge
    {
        // Focus on action -- cpa -- click
        // Focus on impression -- cpm -- enable skip
        // Focus on view -- cpv -- disable skip
        public float cpm { get; set; }
        public float cpv { get; set; }
        public float cpa { get; set; }

        public int cpm_count { get; set; }
        public int cpv_count { get; set; }
        public int cpa_count { get; set; }

        //public string name { get; set; }
        public Charge()
        {
            cpm = cpv = cpa = 0.0f;
            cpm_count = cpv_count = cpa_count = 0;
        }
    }

    public class LocationReportItem : Charge
    {
        public string country { get; set; }
        public string province { get; set; }
        public string province_code { get; set; }
        public string city { get; set; }

        public LocationReportItem()
        {
            country = province_code = province = city = "";
        }
    }
    public class StatResult
    { 
        public string startDate;
        public string endDate;
    }
    public class StatByLocation : StatResult
    {

        public List<LocationReportItem> ReportByLocation { get; set; }

        public StatByLocation()
        {
            ReportByLocation = new List<LocationReportItem>();
            startDate = endDate = "";
        }
    }

    public class TimeReportItem : Charge
    {
        // day of week
        public string dow { get; set; }
        // YYYYMMDD
        public int date { get; set; }

        public TimeReportItem()
        {
            dow = "";
        }

    }

    // by date or by day of week or week, 
    public class StatByDate : StatResult
    {
        public List<TimeReportItem> ReportByTime { get; set; }

        public StatByDate()
        {
            ReportByTime = new List<TimeReportItem>();
        }
    }

    public class StatByWeek : StatResult
    {
        public List<WeekReportItem> ReportByWeek { get; set; }

        public StatByWeek()
        {
            ReportByWeek = new List<WeekReportItem>();
        }
    }

    public class WeekReportItem : Charge
    {
        public int weekStart { get; set; }
        public int weekEnd { get; set; }
        public string strWeek { get; set; }

        public WeekReportItem()
        {
            weekStart = weekEnd = 0;
            strWeek = "";
        }
    }

    // by device, platform, os
    public class StatByDevice : StatResult
    {
        public List<DeviceReportItem> ReportByDevice { get; set; }

        public StatByDevice()
        {
            ReportByDevice = new List<DeviceReportItem>();
        }
    }

    public class DeviceReportItem : Charge
    {
        public string device { get; set; }

        public DeviceReportItem()
        {
            device = "";
        }
    }
    // request paramter
    public class RequestParameters
    {
        //get parameters, GET methods
        // type -- 1, by location
        // type -- 2, by date
        // type -- 3, by dow
        public int type { get; set; }
        // language, en/zh/jp by default en
        public string language { get; set; }
        // format , json or xml, by default json
        public string format { get; set; }
        // id -- id of ads
        public int id { get; set; }
        // start, and end date
        public string startDate { get; set; }
        public string endDate { get; set; }

        public bool moneyIssue { get; set; }
        public RequestParameters()
        {
            type = 1;
            language = "en";
            format = "json";
            id = 0;
            startDate = "";
            endDate = "";
            moneyIssue = false;
        }
    }
}
