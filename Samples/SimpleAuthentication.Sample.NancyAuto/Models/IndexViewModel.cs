namespace SimpleAuthentication.Sample.NancyAuto.Models
{
    public class IndexViewModel
    {
        public string ErrorMessage { get; set; }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(ErrorMessage); }
        }
    }
}