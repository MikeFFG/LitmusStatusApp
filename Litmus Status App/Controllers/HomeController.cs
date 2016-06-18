using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LitmusStatus.Models;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace LitmusStatus.Controllers
{
    public class HomeController : Controller
    {
        private const string URL = "https://mikedburke.litmus.com";
        private const string BrowserUrlParameters = "/tests/29308020.xml";
        private const string EmailUrlParameters = "/tests/29362080.xml";
        public string Time { get; private set; }
        public string Status { get; private set; }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Browsers()
        {
            List<Client> clients = new List<Client>();

            // Make request
            string xml = await GetTestStatus(BrowserUrlParameters);

            // Parse and don't sort (requirements stipulate sorting
            // e-mail clients and not browsers specifically).
            clients = ParseXML(xml);

            return View(clients);
        }

        public async Task<ActionResult> EmailClients()
        {
            List<Client> clients = new List<Client>();

            // Make request
            string xml = await GetTestStatus(EmailUrlParameters);

            // Parse and Sort
            clients = ParseXML(xml);
            clients = Sort(clients);

            return View(clients);
        }

        public List<Client> Sort(List<Client> clients)
        {
            // Sort by Platform Name per requirements
            IEnumerable<Client> ordered = from client in clients
                                          orderby client.PlatformName
                                          select client;

            // Cast back to List<Client>
            clients = ordered.Cast<Client>().ToList();

            return clients;
        }

        public List<Client> ParseXML(string xmlString)
        {
            List<Client> clients = new List<Client>();
            XDocument doc;

            // Load XML
            using (StringReader s = new StringReader(xmlString))
            {
                doc = XDocument.Load(s);
            }

            // Browse by testing_application
            var applications = doc.Descendants("testing_application");

            foreach (var application in applications)
            {
                // Check if we have a value for Average Time
                if (application.Element("average_time_to_process").Value == "")
                {
                    Time = "Unavailable";
                }
                else
                {
                    Time = application.Element("average_time_to_process").Value;
                }

                // Translate Status to words
                if (application.Element("status").Value == "0")
                {
                    Status = "Available";
                }
                else if (application.Element("status").Value == "1")
                {
                    Status = "Some Delays";
                }
                else
                {
                    Status = "Unavailable";
                }

                // Parse XML and Add to new Client Object
                clients.Add(new Client()
                {
                    TimeInS = Time,
                    AppCode = application.Element("application_code").Value,
                    ClientName = application.Element("application_long_name").Value,
                    CurrentStatus = Status,
                    PlatformName = application.Element("platform_name").Value
                });
            }

            return clients;
        }

        public async Task<string> GetTestStatus(string UrlParams)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Add Authorization header.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", "bWlrZS5kLmJ1cmtlQGdtYWlsLmNvbTpCYWRQYXNzd29yZA==");

            // Get response.
            HttpResponseMessage response = client.GetAsync(UrlParams).Result;
            if (response.IsSuccessStatusCode)
            {
                String responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            else
            {
                // Not currently using this for any proper error checking...
                return response.ReasonPhrase;
            }
        }
    }
}