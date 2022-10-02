using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ServenteDaYasmim.Exceptions;

namespace ServenteDaYasmim.Services
{
    public class UnimedService
    {
        public void FaturarGuia(string numberOfGuia)
        {
            var options = new ChromeOptions();

            //options.AddArgument("headless");

            var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://solus.unimedaraguaia.coop.br/prestador/index.php");

            Login(driver);

            Thread.Sleep(10000);

            EnterOnFaturamentoSeriadoPage(driver);

            Thread.Sleep(2000);

            FindGuia(numberOfGuia, driver);

            SetBiometria(driver);

            SetDateAndTime(driver);

            SetPrestadorExecutante(driver);

            ClickFaturar(driver);

            driver.Close();
        }

        private void SetPrestadorExecutante(ChromeDriver driver)
        {
            try
            {
                var selectMotivo = new SelectElement(driver.FindElement(By.Id("pres_ptu0")));

                if (selectMotivo is null)
                    return;

                selectMotivo.SelectByText("YASMIN BARBOSA RODRIGUES");

                Thread.Sleep(3000);
            }
            catch
            {
                return;
            }
        }

        private void ClickFaturar(ChromeDriver driver)
        {
            var buttonFaturar = driver.FindElement(By.Id("apresentar"));

            buttonFaturar.Click();

            Thread.Sleep(2000);
        }

        private void Login(ChromeDriver driver)
        {
            var inputLogin = driver.FindElement(By.Id("operador"));
            var inputPassword = driver.FindElement(By.Id("senha"));

            inputLogin.SendKeys("eduarda");
            inputPassword.SendKeys("123456");

            var buttonLogin = driver.FindElement(By.Id("entrar"));

            buttonLogin.Click();
        }

        private void EnterOnFaturamentoSeriadoPage(ChromeDriver driver)
        {
            var menus = driver.FindElements(By.ClassName("fontAcess"));

            var buttonSeriado = menus[19];

            var seriadoLink = buttonSeriado.GetAttribute("href");

            driver.Navigate().GoToUrl(seriadoLink);
        }

        private void FindGuia(string numberOfGuia, ChromeDriver driver)
        {
            var inputGuia = driver.FindElement(By.Id("guia"));
            inputGuia.SendKeys(numberOfGuia);

            var buttonPesquisarGuia = driver.FindElement(By.Id("enviar"));
            buttonPesquisarGuia.Click();

            try
            {
                var alertError = driver.FindElement(By.ClassName("alert"));

                if (alertError is not null)
                {
                    driver.Close();
                    throw new FindGuiaException();
                }
            }
            catch (NoSuchElementException)
            {
                return;
            }
        }

        private void SetBiometria(ChromeDriver driver)
        {
            var buttonJustificarBiometria = driver.FindElement(By.Id("capturar2"));

            buttonJustificarBiometria.Click();

            var selectMotivo = new SelectElement(driver.FindElement(By.Id("motivo")));
            Thread.Sleep(2000);
            selectMotivo.SelectByText("PACIENTE COM PROBLEMAS PARA CAPTURAR A DIGITAL");

            var buttonFechar = driver.FindElement(By.Id("fechar"));

            buttonFechar.Click();
        }

        private void SetDateAndTime(ChromeDriver driver)
        {
            var executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("document.getElementById('procD0').click()");

            var inputDataRelogio = driver.FindElement(By.Id("dataP0"));
            var inputHoraInicialRelogio = driver.FindElement(By.Id("horaP0"));
            var inputHoraFinalRelogio = driver.FindElement(By.Id("horafP0"));
            var inputHoraInicial = driver.FindElement(By.Id("inicio"));
            var inputHoraFinal = driver.FindElement(By.Id("fim"));
            
            var randomHora = new Random().NextInt64(7, 18);
            var horaInicial = $"{randomHora}:00:00";
            var horaFinal = $"{randomHora}:30:00";

            inputHoraInicial.SendKeys(horaInicial);
            Thread.Sleep(1000);
            inputHoraFinal.SendKeys(horaFinal);
            Thread.Sleep(1000);

            inputDataRelogio.SendKeys(DateTime.Now.ToString("dd/MM/yyyy"));
            inputHoraInicialRelogio.SendKeys(horaInicial);
            inputHoraFinalRelogio.SendKeys(horaFinal);



            //TODO: Setar valores da data e horario
        }
    }
}
