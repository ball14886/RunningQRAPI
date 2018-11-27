using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RunningQRCodeAPI.Models
{
    public class ScanResult
    {
        public Result Result { get; set; }
        public RunnerStartEndTime RecordTime { get; set; }
    }
}