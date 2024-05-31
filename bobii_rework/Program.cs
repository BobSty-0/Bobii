namespace  bobii_rework
{
    public class Program
    {
        public static void Main(string[] args)
            => new App().Init().GetAwaiter().GetResult();
    }
}


