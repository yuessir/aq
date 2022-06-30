using Navyblue.BaseLibrary;

namespace TFA.AQS.Order.Domain.Requests.V3
{
    public class AqsTranslation
    {
        public string Language { get; set; }

        public string Tournament { get; set; }

        public string HomeTeam { get; set; }

        public string AwayTeam { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}