using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Scrap
{
    class Program
    {

        struct Listed
        {
            public String Title { get; set; }
            public String Url { get; set; }
            public double Price { get; set; }
            public Listed(string title, string url, double price)
            {
                Title = title;
                Url = url;
                Price = price;
            }

        }
        static List<Listed> listed = new List<Listed>();
        static void Main(string[] args)
        {

            Grab("https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=ps3&_sacat=0&LH_TitleDesc=0&_odkw=pa3&_osacat=0");
        }
        static void Grab(string Link)
        {
            listed.Add(new Listed("thing", "thing", 48.5));
            HttpClient httpClient = new HttpClient();
            string html = httpClient.GetStringAsync(Link).Result;
            HtmlDocument htmlDocument = new HtmlDocument();
            
            htmlDocument.LoadHtml(html);
            var ProductList = htmlDocument.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Equals("srp-results srp-list clearfix")).First().Descendants().Where(node => node.GetAttributeValue("class", "").Contains("s-item--watch-at-corner")).ToList();
            var ProductTitle = htmlDocument.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Equals("s-item__title")).ToList();
            var ProductPrice = htmlDocument.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Equals("s-item__price")).ToList();
            var ProductUrl = htmlDocument.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Contains("s-item__link")).ToList();
            for (int i = 0; i < ProductList.Count; i++)
            {
                Console.WriteLine(ProductList[i].GetAttributeValue("class", ""));
                /*
                Console.WriteLine(Regex.Replace(ProductTitle[i].InnerText.Replace("New Listing", ""), @"[^\u0000-\u007F]+", string.Empty).TrimStart());
                Console.WriteLine(ProductPrice[i].InnerText);
                Console.WriteLine(ProductUrl[i].GetAttributeValue("href", ""));
                */
            }
            Console.ReadLine();

        }
    }
}
