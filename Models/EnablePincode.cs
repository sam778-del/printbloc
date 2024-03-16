using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintblocProject.Model
{
    class EnablePincodeInstance
    {
        private static EnablePincodeInstance _instance;
        public static EnablePincodeInstance Instance => _instance ?? (_instance = new EnablePincodeInstance());

        public string NewPincode { get; set; }
        public string ConfirmPincode { get; set; }
    }
}
