using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Linq;

namespace Scrap
{
    class Program
    {
        string CurrentLink;

        static void Main(string[] args)
        {

            Grab("https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=ps3&_sacat=0&LH_TitleDesc=0&_odkw=pa3&_osacat=0");
        }
        static void Grab(string Link)
        {
            HttpClient httpClient = new HttpClient();
            string html = httpClient.GetStringAsync(Link).Result;

            HtmlDocument htmlDocument = new HtmlDocument();
            
            htmlDocument.LoadHtml(html);

            var ProductList = htmlDocument.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Equals("s-item__title")).ToList();
            foreach (var item in ProductList)
            {
                
                Console.WriteLine(item.InnerText.Replace("New Listing", ""));
            }
            Console.ReadLine();

        }
    }
}
