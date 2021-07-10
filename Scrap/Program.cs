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
            public string SaleType {get; set;}
            public String Title { get; set; }
            public String Url { get; set; }
            public decimal Price { get; set; }
            public Listed(string saletype, string title, string url, decimal price)
            {
                SaleType = saletype;
                Title = title;
                Url = url;
                Price = price;
            }

        }
        static List<Listed> listed = new List<Listed>();
        static void Main(string[] args)
        {

            Grab("https://www.ebay.com/sch/i.html?_from=R40&_nkw=ps3&_sacat=0&LH_TitleDesc=0&rt=nc&LH_All=1");
        }
        /// <summary>
        /// Grabs SaleType, Title, Price, Url data from all listings on page 
        /// </summary>
        /// <param name="Link"></param>
        static void Grab(string Link)
        {
            HttpClient httpClient = new HttpClient();
            //Grabs html document from page
            string html = httpClient.GetStringAsync(Link).Result;
            HtmlDocument htmlDocument = new HtmlDocument();

            //Load document into htmlagilitypack document editor
            htmlDocument.LoadHtml(html);
            //Grab all container listings on page
            var ProductList = htmlDocument.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Equals("srp-results srp-list clearfix")).First().Descendants().Where(node => node.GetAttributeValue("class", "").Contains("s-item__wrapper clearfix")).ToList();

            HtmlNode bidbuy;

            for (int i = 0; i < ProductList.Count; i++)
            {
                //Grabs class on current listing that matches "s-item__bids s-item__bidCount"
                bidbuy = ProductList[i].Descendants().Where(node => node.GetAttributeValue("class", "").Equals("s-item__bids s-item__bidCount")).FirstOrDefault();
                //Creation of Listed object to add to "listed" list
                listed.Add(new Listed(
                    //If null means s-item__bids... class not exist and thus must be buy listing.
                    bidbuy == null? "Buy":"Bid",
                    //Grabs title from s-item__title class and removes non-unicode char, "New Listing" and whitespace prefixing
                    $"{Regex.Replace(ProductList[i].Descendants().Where(node => node.GetAttributeValue("class", "").Equals("s-item__title")).First().InnerText.Replace("New Listing", ""), @"[^\u0000-\u007F]+", string.Empty).TrimStart()}",
                    //Gets url of listing from s-item__link class
                    $"{ProductList[i].Descendants().Where(node => node.GetAttributeValue("class", "").Equals("s-item__link")).First().GetAttributeValue("href", "")}", 
                    //Gets pricing data from s-item__price class and removes $, whitespace then splits string at "to" (ex. "$140 to $160") and takes first split finally parsing result to decimal 
                    Decimal.Parse($"{ProductList[i].Descendants().Where(node => node.GetAttributeValue("class", "").Equals("s-item__price")).First().InnerText.Replace("$","").Replace(" ", "").Split("to").First()}", System.Globalization.CultureInfo.InvariantCulture)));
            }
            foreach (var item in listed)
            {
                Console.WriteLine(item.SaleType);
                Console.WriteLine(item.Title);
                Console.WriteLine(item.Price);
                Console.WriteLine(item.Url);
            }
            
            Console.ReadLine();

        }
    }
}
