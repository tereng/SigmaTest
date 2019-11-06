using System;
using System.Collections.Generic;
using System.Text;

namespace DownloadSigmaBlob
{
    public abstract class SensorData
    {
        public List<double> MeasurementData { get; set; }
        public List<DateTime> MeasurementTime { get; set; }
        public DateTime MeasurementDay { get; set; }
    }
}
