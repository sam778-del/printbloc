using System.Text;
using System.Threading.Tasks;

namespace PrintblocProject.Model
{
    class Status
    {
        private static Status _instance;
        public static Status Instance => _instance ?? (_instance = new Status());

        public bool Online { get; set; }
    }
}
