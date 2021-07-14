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
        static List<Listed> listed = new List<Listed>();
        static int pages = 5;
        static string searchterm;
        static void Main(string[] args)
        {
            Grab();
        }
        /// <summary>
        /// Grabs SaleType, Title, Price, Url data from all listings on page 
        /// </summary>
        /// <param name="Link"></param>
        static void Grab()
        {
            if (searchterm == null)
            {
                Console.WriteLine("Enter search term");
                searchterm = Console.ReadLine();
            }
            List<HtmlNode> ProductList = new List<HtmlNode>();
            HtmlNode bidbuy;
            HttpClient httpClient = new HttpClient();
            HtmlDocument htmlDocument = new HtmlDocument();
            string html;
            //Grabs listings from specified number of pages
            for (int j = 1; j < pages; j++)
            {
                string Link = $"https://www.ebay.com/sch/i.html?_from=R40&_nkw={searchterm}&_sacat=0&_pgn={j}";
                //Grabs html document from page
                html = httpClient.GetStringAsync(Link).Result;
                //Load document into htmlagilitypack document editor
                htmlDocument.LoadHtml(html);
                //Very important never remove
                ProductList.Clear();
                //Grab all container listings on page
                ProductList.AddRange(htmlDocument.DocumentNode.Descendants("ul").Where(node => node.GetAttributeValue("class", "").Equals("srp-results srp-list clearfix")).First().Descendants().Where(node => node.GetAttributeValue("class", "").Contains("s-item__wrapper clearfix")).ToList());
                
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
                        Decimal.Parse(ProductList[i].Descendants().Where(node => node.GetAttributeValue("class", "").Equals("s-item__price")).First().InnerText.Replace("$","").Replace(" ", "").Split("to").First(), System.Globalization.CultureInfo.InvariantCulture)));
                }
            }
            
            //Grabs search term for database naming later
            string SearchTerm = htmlDocument.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Equals("gh-tb ui-autocomplete-input")).First().ChildAttributes("value").First().Value;
            foreach (var item in listed)
            {
                
                Console.WriteLine(item.SaleType);
                Console.WriteLine(item.Title);
                Console.WriteLine(item.Price);
                Console.WriteLine(item.Url);
            }
            //Sends results to DataAccess class
            DataAccess.SaveResults(SearchTerm, listed);
            
            Console.ReadLine();

        }
    }
}
