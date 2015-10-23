using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using WebKit;
using System.Windows.Forms;
using System.Threading;

namespace Web_Scrape
{
    public class WebScrape
    {
        public string url { get; set; }
        public string page { get; set; }

        private List<string> links = new List<string>();
        private List<string> pagesVisited = new List<string>();
        public string currentPage {get; set;}
        public string currentURL { get; set; }

        private List<KeyValuePair<string,string>> emailsFound = new List<KeyValuePair<string,string>>();
        private List<KeyValuePair<string, string>> addressesFound = new List<KeyValuePair<string, string>>();
 

        public delegate void getHTML(Object source, EventArgs e);
        
        public WebScrape(string url)
        {
            this.url = url;
            page = Download(url);
            links = getUrls(page);
            

        }        
        private List<string> getUrls(string page)
        {
            //String pattern = "href=\".*\"";
                       
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(page);
            HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//a");
            foreach(HtmlNode link in collection)
            {
                if (link.HasAttributes && link.Attributes.Contains("href"))
                {
                    links.Add(link.Attributes["href"].Value);
                }
            }
            
            return links;
        }
        public void showPages()
        {
            if(links.Count!=0)
            {
                foreach (string link in links)
                {
                    Console.WriteLine(link);
                }
            }
        }
        public bool hasPage(string page)
        {
            bool hasPage = false;

            foreach(string link in links)
            {
                if(link.Contains(page))
                {
                    hasPage = true;
                }
            }
            return hasPage;
        }
        public string getPage(string search)
        {
            
            foreach(string link in links)
            {
                if(link.Contains(search))
                {
                    if (checkContactUrl(link))
                    {
                        currentURL = link;
                        break;                        
                    }
                }
            }
            if (currentURL != null)
            {
                if (!currentURL.Contains("http"))
                {
                    currentURL = url + currentURL;
                }

                Thread t = new Thread(new ThreadStart(WebBrowserThread));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
            }
            
            return currentPage;
        }
        private void WebBrowserThread()
        {
            WebBrowser wb = new WebBrowser();
            wb.Navigate(currentURL);

            wb.DocumentCompleted +=
                new WebBrowserDocumentCompletedEventHandler(
                    wb_DocumentCompleted);

            while (wb.ReadyState != WebBrowserReadyState.Complete)                
                Application.DoEvents();

            //Added this line, because the final HTML takes a while to show up
            
            currentPage = wb.Document.Body.InnerHtml;
           

            wb.Dispose();
        }

        private void wb_DocumentCompleted(object sender,
            WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser wb = (WebBrowser)sender;
            currentPage = wb.Document.Body.InnerHtml;
            //This can be toggled to adjust process time
            Thread.Sleep(300);
        }
       
       public void canParse(string page)
        {
            HtmlAgilityPack.HtmlDocument resultat = new HtmlAgilityPack.HtmlDocument();
            resultat.LoadHtml(page);
            List<HtmlNode> toftitle = resultat.DocumentNode.Descendants().Where
            (x => (x.Name == "li")).ToList();

           
        }
        
        public string cleanString(string stringtoClean)
       {           
           
           

           return stringtoClean;
       }

        public List<List<HtmlNode>> getNodes(HtmlAgilityPack.HtmlDocument page)
        {
            List<List<HtmlNode>> searchNodes = new List<List<HtmlNode>>();
            List<HtmlNode> listNodes = page.DocumentNode.Descendants().Where
            (x => (x.Name == "li")).ToList();
            List<HtmlNode> pNodes = page.DocumentNode.Descendants().Where
            (x => (x.Name == "p")).ToList();
            List<HtmlNode> aNodes = page.DocumentNode.Descendants().Where
            (x => (x.Name == "a")).ToList();
            List<HtmlNode> tdNodes = page.DocumentNode.Descendants().Where
            (x => (x.Name == "td")).ToList();
            searchNodes.Add(listNodes);
            searchNodes.Add(pNodes);
            searchNodes.Add(aNodes);
            searchNodes.Add(tdNodes);

            return searchNodes;

        }

        public void getEmails()
        {            
            HtmlAgilityPack.HtmlDocument resultat = new HtmlAgilityPack.HtmlDocument();
            if (currentPage != null)
            {
                Match email = null;
                string emailPattern = @"\w+[.|@]\w+[.|@][A-Za-z.]+";
                
                resultat.LoadHtml(currentPage);
                List<List<HtmlNode>> searchNodes = getNodes(resultat);
                    
                foreach(List<HtmlNode> nodeList in searchNodes)
                {
                    if(nodeList.Count > 0)
                    {
                        foreach (HtmlNode listItem in nodeList)
                        {
                            if (foundEmail(listItem.InnerText))
                            {
                                email = Regex.Match(listItem.InnerText, emailPattern);
                                emailsFound.Add(new KeyValuePair<string, string>(email.Value, currentURL));

                            }
                        }
                    }
                }

            }
            else
            {
                Console.WriteLine("page not available");
            }
                       
        }
        public void getAddresses()
        {
            HtmlAgilityPack.HtmlDocument resultat = new HtmlAgilityPack.HtmlDocument();
            if (currentPage != null)
            {
                Match address = null;                
                string addressPattern = @"\d{1,5}\s\w+\s\w+";
                resultat.LoadHtml(currentPage);
                List<List<HtmlNode>> searchNodes = getNodes(resultat);

                foreach (List<HtmlNode> nodeList in searchNodes)
                {
                    if (nodeList.Count > 0)
                    {
                        foreach (HtmlNode listItem in nodeList)
                        {
                            if (foundEmail(listItem.InnerText))
                            {
                                address = Regex.Match(listItem.InnerText, addressPattern);
                                addressesFound.Add(new KeyValuePair<string, string>(address.Value, currentURL));

                            }
                        }
                    }
                }

            }
            else
            {
                Console.WriteLine("page not available");
            }
        }
        public void learnContactUrl(bool yesno, string url)
        {

        }
        public bool checkContactUrl(string url)
        {
            bool isSensible = true;
            string appropriate = "";
            
            Console.WriteLine("is this url: " + url + " appropriate? (y/n)");
            appropriate = Console.ReadLine();
            if (appropriate.Equals("n"))
            {
                isSensible = false; 
            }
                
            
            
            return isSensible;
            
        }

        public void showfoundEmails()
        {
            
            StreamWriter emailFileUpdate = File.AppendText("emails.txt");
            
            if(emailsFound.Count > 0)
            {
                foreach(KeyValuePair<string,string> kvp in emailsFound)
                {
                    Console.WriteLine(kvp);
                    emailFileUpdate.WriteLine(kvp);
                }
            }
            else
            {
                Console.WriteLine("no emails found");                
                emailFileUpdate.WriteLine("No emails found");
                if (currentURL != null) { Console.WriteLine(currentURL); }
            }
            Console.WriteLine("------------------------------");
            emailFileUpdate.WriteLine("-----------------------------");
            emailFileUpdate.Close();
        }
        private bool foundEmail(string email)
        {
            bool found = false;
            string emailPattern = @"\w+@\w+[.]\w+";
            found = Regex.IsMatch(email, emailPattern);           

            return found;
        }
        public void quickSearch()
        {
            getPage("contact");
            getEmails();
        }
        private static string Download(string url)
        {
            
            string responseFromServer = "";

            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                          
                

                responseFromServer = reader.ReadToEnd();
               

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return responseFromServer;

        }
    }
}
