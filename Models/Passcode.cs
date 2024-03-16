using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintblocProject.Model
{
    class PasscodeModel
    {
        private static PasscodeModel _instance;

        public static PasscodeModel Instance => _instance ?? (_instance = new PasscodeModel());

        public string Passcode { get; set; }
    }
}
