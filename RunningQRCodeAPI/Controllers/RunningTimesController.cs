using RunningQRCodeAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using Result = RunningQRCodeAPI.Models.Result;
using System.IO;
using System.ComponentModel;

namespace RunningQRCodeAPI.Controllers
{
    public class RunningTimesController : Controller
    {
        private static string dateTimeFormat = "HH : mm : ss.fff";
        //private string dateTimeFormatTimeSpan = @"hh \: mm \: ss\.ffff";
        CultureInfo thaiCulture = new CultureInfo("th-TH");
        string fileName = "รายงานสรุป " + DateTime.Now.ToString("dd-MM-yyyy HHmmss");

        public JsonResult GetOverviewJson()
        {
            return Json(Overview(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Overview()
        {
            return View(GetOverviewList());
        }

        public List<RunnerStartEndTime> GetOverviewList()
        {
            var runnerRecords = new List<RunnerScanDateTime>();
            using (RunningDatabaseDataContext rdd = new RunningDatabaseDataContext())
            {
                if (rdd.RunnerScanDateTimes.Any())
                {
                    runnerRecords.AddRange(rdd.RunnerScanDateTimes);
                    runnerRecords.OrderBy(x => x.RunnerIdentification);
                }
            }

            var runnerOverview = new List<RunnerStartEndTime>();
            var grouppedRunnerRecords = runnerRecords.GroupBy(x => x.RunnerIdentification);
            foreach (var runner in grouppedRunnerRecords)
            {
                RunnerStartEndTime runnerStatus = new RunnerStartEndTime();
                runnerStatus.RunnerIdentification = runner.Key;
                var runnerMaxScannedTime = runner.Max(x => x.ScannedDateTime);
                var runnerMinScannedTime = runner.Min(x => x.ScannedDateTime);

                runnerStatus.RunnerStartTime = runnerMinScannedTime.ToString(dateTimeFormat, thaiCulture);

                if (runnerMaxScannedTime == runnerMinScannedTime)
                {
                    runnerStatus.RunnerEndTime = "";
                    runnerStatus.RunnerTotalRunningTime = "";
                }
                else
                {
                    runnerStatus.RunnerEndTime = runnerMaxScannedTime.ToString(dateTimeFormat, thaiCulture);
                    var diffTime = (runnerMaxScannedTime - runnerMinScannedTime);
                    var dateTimeTimeDiff = new DateTime(Math.Abs(diffTime.Ticks));
                    runnerStatus.RunnerTotalRunningTime = dateTimeTimeDiff.ToString(dateTimeFormat, thaiCulture);
                }
                runnerOverview.Add(runnerStatus);
            }
            return runnerOverview;
        }

        public JsonResult GetOverviewSpecificPersonJson(string runnerIdentification)
        {
            return Json(GetOverviewSpecificPerson(runnerIdentification), JsonRequestBehavior.AllowGet);
        }

        public RunnerStartEndTime GetOverviewSpecificPerson(string runnerIdentification)
        {
            var runnerRecords = new List<RunnerScanDateTime>();
            using (RunningDatabaseDataContext rdd = new RunningDatabaseDataContext())
            {
                if (rdd.RunnerScanDateTimes.Any(x => x.RunnerIdentification == runnerIdentification))
                {
                    runnerRecords.AddRange(rdd.RunnerScanDateTimes.Where(x => x.RunnerIdentification == runnerIdentification));
                    runnerRecords.OrderBy(x => x.RunnerIdentification);
                }
            }

            RunnerStartEndTime runnerStatus = new RunnerStartEndTime();
            runnerStatus.RunnerIdentification = runnerIdentification;

            if (!runnerRecords.Any())
            {
                runnerStatus.RunnerStartTime = "";
                runnerStatus.RunnerEndTime = "";
                runnerStatus.RunnerTotalRunningTime = "";
            }
            else
            {
                var runnerMaxScannedTime = runnerRecords.Max(x => x.ScannedDateTime);
                var runnerMinScannedTime = runnerRecords.Min(x => x.ScannedDateTime);

                runnerStatus.RunnerStartTime = runnerMinScannedTime.ToString(dateTimeFormat, thaiCulture);

                if (runnerMaxScannedTime == runnerMinScannedTime)
                {
                    runnerStatus.RunnerEndTime = "";
                    runnerStatus.RunnerTotalRunningTime = "";
                }
                else
                {
                    runnerStatus.RunnerEndTime = runnerMaxScannedTime.ToString(dateTimeFormat, thaiCulture);
                    var diffTime = (runnerMaxScannedTime - runnerMinScannedTime);
                    var dateTimeTimeDiff = new DateTime(Math.Abs(diffTime.Ticks));
                    runnerStatus.RunnerTotalRunningTime = dateTimeTimeDiff.ToString(dateTimeFormat, thaiCulture);
                }
            }
            return runnerStatus;
        }

        public ScanResult RecordByScan(string runnerIdentification)
        {
            if (string.IsNullOrEmpty(runnerIdentification))
            {
                Result result = new Result()
                {
                    IsSuccess = false,
                    Message = "บันทึกเวลาล้มเหลว"
                };

                RunnerStartEndTime record = new RunnerStartEndTime();

                var resultRecord = new ScanResult()
                {
                    RecordTime = record,
                    Result = result
                };

                return resultRecord;
            }
            else
            {
                try
                {
                    char[] runnerIDChars = runnerIdentification.ToCharArray();
                    if(char.IsDigit(runnerIDChars[runnerIDChars.Length - 1]))
                    {
                        if (runnerIDChars[0] == 'S' && runnerIdentification[1] != 'S')
                        {
                            runnerIdentification = runnerIdentification.Replace("S", "โสด");
                        }
                        else if (runnerIDChars[0] == 'S' && runnerIDChars[1] == 'S' && runnerIDChars[2] == 'P')
                        {
                            runnerIdentification = runnerIdentification.Replace("SSP", "โสดSpecial");
                        }
                    }

                    var runnerScanRecord = new RunnerScanDateTime()
                    {
                        RunnerIdentification = runnerIdentification,
                        ScannedDateTime = DateTime.Now.AddHours(7)
                    };

                    using (RunningDatabaseDataContext rdd = new RunningDatabaseDataContext())
                    {
                        rdd.RunnerScanDateTimes.InsertOnSubmit(runnerScanRecord);
                        rdd.SubmitChanges();
                    }

                    Result result = new Result()
                    {
                        IsSuccess = true,
                        Message = "บันทึกเวลาสำเร็จ"
                    };

                    var record = GetOverviewSpecificPerson(runnerIdentification);
                    var resultRecord = new ScanResult()
                    {
                        RecordTime = record,
                        Result = result
                    };

                    return resultRecord;
                }
                catch (Exception e)
                {
                    Result result = new Result()
                    {
                        IsSuccess = false,
                        Message = "บันทึกเวลาล้มเหลว"
                    };

                    RunnerStartEndTime record = new RunnerStartEndTime();

                    var resultRecord = new ScanResult()
                    {
                        RecordTime = record,
                        Result = result
                    };

                    return resultRecord;
                }
            }
        }

        public ActionResult RecordByScanView(string runnerIdentification)
        {
            var result = RecordByScan(runnerIdentification);
            return View(result);
            //return View("~/View/RunningTimes/ScanSuccess.cshtml", result);
        }

        public ActionResult OverviewSpecificPerson(string runnerIdentification)
        {
            var result = GetOverviewSpecificPerson(runnerIdentification);
            return View(result);
        }
        
        public void WriteTsv<T>(IEnumerable<T> data, TextWriter output)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in props)
            {
                output.Write(prop.DisplayName);
                output.Write("\t");
            }
            output.WriteLine();
            foreach (T item in data)
            {
                foreach (PropertyDescriptor prop in props)
                {
                    output.Write(prop.Converter.ConvertToString(prop.GetValue(item)));
                    output.Write("\t");
                }
                output.WriteLine();
            }
        }

        public void ExportListFromTsv()
        {
            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName + ".xls");
            Response.AddHeader("Content-Type", "application/vnd.ms-excel");
            WriteTsv(GetOverviewList(), Response.Output);
            Response.End();
        }
    }
}