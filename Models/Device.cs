using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintblocProject.Model
{
    class Device
    {
        public string DeviceId { get; set; }
        public string MachineName { get; set; }
        public int TeamId { get; set; }
        public string IpAddress { get; set; }
        public string Os { get; set; }
        public bool DeviceStatus { get; set; }
        public string Passcode { get; set; }
        public bool IsPasscode { get; set; }
        public string ConnectionId { get; set; }
    }
}
