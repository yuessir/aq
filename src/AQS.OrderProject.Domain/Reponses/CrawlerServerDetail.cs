using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Domain.Reponses
{
    public class CrawlerServerDetail
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Server { get; set; }

        public string Status { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}