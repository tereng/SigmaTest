using System;
using System.Collections.Generic;
using System.Text;

namespace DownloadSigmaBlob
{
    public class Sensor
    {
        public string SensorName { get; set; }
        public List<SensorData> AggregatedSensorData { get; set; }
    }
}
