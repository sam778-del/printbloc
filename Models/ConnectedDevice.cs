using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintblocProject.Model
{
    class ConnectedDevice
    {
        private static ConnectedDevice _instance;
        public static ConnectedDevice Instance => _instance ?? (_instance = new ConnectedDevice());

        public string DeviceId { get; set; }
    }
}
