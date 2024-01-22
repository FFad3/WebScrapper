using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace WebScrapper
{
    internal class PginationManager
    {
        private const string _PreviousXPath = "//ul[contains(@class, 'pagination')]/li[1]";
        private const string _NextXPath = "//ul[contains(@class, 'pagination')]/li[last()]";

        private readonly IWebDriver _driver;

        private PaginationButton _Next;
        private PaginationButton _Previous;

        public PginationManager(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException();
            _Next = GetNextButton();
            _Previous = GetPreviousButton();
        }

        public void SetPageSizeToMax()
        {
            IWebElement webElement = _driver.FindElement(By.Name("maintable_length"));
            SelectElement tableSizeDropDownBox = new(webElement);
            tableSizeDropDownBox.SelectByIndex(tableSizeDropDownBox.Options.Count - 1);

            Update();
        }

        public bool NextPage()
        {
            if (!_Next.Click())
                return false;

            Update();
            return true;
        }

        public bool Previous()
        {
            if (!_Previous.Click())
                return false;

            Update();
            return true;
        }

        public void Update()
        {
            WaitForLoading();
            _Next = GetNextButton();
            _Previous = GetPreviousButton();
        }

        private PaginationButton GetNextButton() => GetButton("Next", _NextXPath);

        private PaginationButton GetPreviousButton() => GetButton("Previous", _PreviousXPath);

        private PaginationButton GetButton(string buttonName, string xPath)
        {
            var element = _driver.FindElement(By.XPath(xPath))
                ?? throw new ArgumentNullException(buttonName, $"was null for {xPath}");
            return new PaginationButton(element);
        }

        private void WaitForLoading()
        {
            var element = _driver.FindElement(By.Id("modalWait"));
            if (!element.Displayed)
            {
                return;
            }
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(4));
            wait.Until(driver => !driver.FindElement(By.Id("modalWait")).Displayed);
        }

        private class PaginationButton
        {
            private readonly IWebElement _Parent;
            private readonly IWebElement _NextButton;

            public PaginationButton(IWebElement webElement)
            {
                _Parent = webElement;
                _NextButton = webElement.FindElement(By.XPath("./a"))
                    ?? throw new ArgumentNullException("Button");
            }

            public bool IsDisabled => _Parent.GetDomAttribute("class").Contains("disabled");

            public bool Click()
            {
                if (IsDisabled)
                {
                    return false;
                }
                _NextButton.Click();
                return true;
            }
        }
    }
}