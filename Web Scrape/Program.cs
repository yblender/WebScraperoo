using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Web_Scrape;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Customsearch.v1;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml;


namespace RequestURL
{

    class Program
    {

        static void Main(string[] args)
        {
           

            string homeUrl1 = "http://www.reddit.com";
            string homeUrl2 = "http://www.vadacom.co.nz/";
            string homeUrl3 = "http://www.yfu.com.au/";
            string homeUrl4 = "http://www.smartair.asia";
            string homeUrl5 = "http://www.codeproject.com";

            string[] sites = new string[5] { homeUrl1, homeUrl2, homeUrl3, homeUrl4, homeUrl5 };

            foreach (string url in sites)
            {
                WebScrape site = new WebScrape(url);
                site.quickSearch();
                site.showfoundEmails();
            }

            Console.ReadKey();



        }
    }
}
