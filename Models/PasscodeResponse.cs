using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PrintblocProject.Model
{
    class PasscodeResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public Device Device { get; set; }
        public bool Error { get; set; }
    }
}
