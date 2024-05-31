namespace bobii_rework.Extensions
{
    public static class ObjectExtensions
    {
        public static void WriteLineToConsole(this Object sender, string output)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} {sender.GetType().Name}.cs => {output}");
        }
    }
}
