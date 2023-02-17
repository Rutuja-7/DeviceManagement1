using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace DeviceManagement.Models
{
    public class DeviceTwin : Device
    {
        public int DeviceId { get; set; }
        public TwinCollection DesiredProperties { get; set; }
        public TwinCollection ReportedProperties { get; set; }
    }
}
