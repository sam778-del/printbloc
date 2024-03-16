using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintblocProject.Model
{
    class ProfileSettingInstance
    {
        private static ProfileSettingInstance _instance;
        public static ProfileSettingInstance Instance => _instance ?? (_instance = new ProfileSettingInstance());

        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
