using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintblocProject.Model
{
    class AuthResult
    {
        private static AuthResult _instance;
        public static AuthResult Instance => _instance ?? (_instance = new AuthResult());

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public User User { get; set; }
        public bool Success { get; set; }
        public string Errors { get; set; }
    }
}
