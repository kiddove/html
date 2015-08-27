using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSS
{
    // dummy info, page to js.
    class DummyInfo
    {
        // account information that the video belongs to
        public string account { get; set; }
        // subscriber information that who is watching the video(login account, or empty)
        public string viewer { get; set; }

        // account/video property, can one of them in effect at the meantime, the other one must be empty
        // if allows all then use 'all', otherwise, insert allowed account
        public List<string> whitelist { get; set; }
        // if denies all then use 'all', otherwise, insert denied account
        public List<string> blacklist { get; set; }

        public DummyInfo()
        {
            account = viewer = null;
            whitelist = new List<string>();
            blacklist = new List<string>();
        }
    }
}
