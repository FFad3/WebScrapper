using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WebScrapper
{
    internal partial class Program
    {
        public class WebScrapper : IDisposable
        {
            private const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
            private readonly IWebDriver _driver;
            private readonly PginationManager _paginationManager;

            public WebScrapper()
            {
                _driver = CreateDriver(opt =>
                {
                    opt.AddArgument($"--user-agent={userAgent}");
                    //opt.AddArgument("--headless");
                    opt.AddArgument("no-sandbox");
                });
                _driver.Navigate().GoToUrl("https://rpkip.knf.gov.pl");
                _paginationManager = new PginationManager(_driver);
            }

            public void Start()
            {
                _paginationManager.SetPageSizeToMax();
                using (var writer = new StreamWriter("output.csv", false, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(GetRowDataList());
                }
            }

            private List<RowData> GetRowDataList()
            {
                var rowDataList = new List<RowData>();

                do
                {
                    var doc = GetTableHtml();
                    var rows = GetRows(doc);

                    foreach (var row in rows)
                    {
                        var rowData = RowData.Create(row);
                        Console.WriteLine(row.InnerText);
                        rowDataList.Add(rowData);
                    }
                } while (_paginationManager.NextPage());

                return rowDataList;
            }

            private IList<HtmlNode> GetRows(HtmlDocument doc) => doc.QuerySelectorAll("tr");

            private HtmlDocument GetTableHtml()
            {
                HtmlDocument document = new HtmlDocument();
                IWebElement tbody = _driver.FindElement(By.XPath("//table[@id='maintable']/tbody"));
                document.LoadHtml(tbody.GetAttribute("innerHTML"));
                return document;
            }

            private static IWebDriver CreateDriver(Action<ChromeOptions> configure)
            {
                var opt = new ChromeOptions();
                configure(opt);

                IWebDriver driver = new ChromeDriver(opt);
                //driver.Manage().Window.Minimize();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(8);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
                return driver;
            }

            public class RowData
            {
                [Name("Id")]
                public int Id { get; set; }

                [Name("RegisterType")]
                public string RegisterType { get; set; } = string.Empty;

                [Name("NumberInRegistry")]
                public string NumberInRegistry { get; set; } = string.Empty;

                [Name("CompanyName")]
                public string CompanyName { get; set; } = string.Empty;

                [Name("Status")]
                public string Status { get; set; } = string.Empty;

                [Name("City")]
                public string City { get; set; } = string.Empty;

                [Name("NIP")]
                public string NIP { get; set; } = string.Empty;

                public static RowData Create(HtmlNode htmlRow) => new()
                {
                    Id = int.Parse(htmlRow.ChildNodes[0].InnerHtml),
                    RegisterType = htmlRow.ChildNodes[1].InnerHtml,
                    NumberInRegistry = htmlRow.ChildNodes[2].InnerHtml,
                    CompanyName = htmlRow.ChildNodes[3].InnerHtml,
                    Status = htmlRow.ChildNodes[4].InnerHtml,
                    City = htmlRow.ChildNodes[5].InnerHtml,
                    NIP = htmlRow.ChildNodes[6].InnerHtml
                };
            }

            public void Dispose()
            {
                _driver.Dispose();
                _driver.Quit();
                GC.SuppressFinalize(this);
            }
        }
    }
}