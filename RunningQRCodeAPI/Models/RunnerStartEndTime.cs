using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RunningQRCodeAPI.Models
{
    public class RunnerStartEndTime
    {
        public string RunnerIdentification { get; set; }
        public string RunnerStartTime { get; set; }
        public string RunnerEndTime { get; set; }
        public string RunnerTotalRunningTime { get; set; }
    }
}