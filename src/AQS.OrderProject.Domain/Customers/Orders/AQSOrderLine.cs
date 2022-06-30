using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public class AqsOrderLine
    {
        public long Handicap { get; set; }

        public long Price { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}