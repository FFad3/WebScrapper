namespace WebScrapper
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            var scrapper = new WebScrapper();
            scrapper.Start();
            Console.ReadKey();
        }
    }
}