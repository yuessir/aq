namespace AQS.OrderProject.Domain
{
    public enum OrderTaskStatus
    {
		Unknown=0,

        Ship=1,

        Suspend=3,

        Replied=6,

        Closed=7,

        CustomerCanceled=8
    }
}