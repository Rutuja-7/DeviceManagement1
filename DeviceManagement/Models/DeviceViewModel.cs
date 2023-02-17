using System;
using System.Collections.Generic;

namespace DeviceManagement.Models
{
    public class DeviceViewModel
    {
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public string LastActivityTime { get; set; }
        public string ConnectionStatus { get; set; }
        public DesiredProperties DesiredProperties { get; set; }
        public ReportedProperties ReportedProperties { get; set; }
    }
}
