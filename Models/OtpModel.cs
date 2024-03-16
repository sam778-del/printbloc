namespace PrintblocProject.Model
{
    class OtpModel
    {
        private static OtpModel _instance;

        public static OtpModel Instance => _instance ?? (_instance = new OtpModel());

        public string Code { get; set; }
    }
}
