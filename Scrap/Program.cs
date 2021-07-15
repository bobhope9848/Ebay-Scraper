using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace Scrap
{
    class Program
    {
        static List<Listed> listed = new List<Listed>();
        //How many pages to search
        static int pages = 5;
        static string searchterm = "";
        //Display found listings and then hold up program
        static bool displayhold = false;
        //Checks ebay for new listings every X minutes
        static int CheckForUpdate = 30;
        static void Main(string[] args)
        {   
            if (args.Count() > 0)
            {
                //Split argument recieved at = sign (eg. --check=60)
                string[] split = args[0].Split("=");
                //Convert number recieved to int CheckForUpdate and set default 60min on failure
                if (!int.TryParse(split[1], out CheckForUpdate))
                {
                    CheckForUpdate = 60;
                }
                //Continuously run program every X amount of minutes 
                while (split[0].Contains("check"))
                {
                    Grab();
                    Thread.Sleep(CheckForUpdate * 60000);
                }
            }
            
        }
        /// <summary>
        /// Grabs SaleType, Title, Price, Url data from all listings on page 
        /// </summary>
        /// <param name="Link"></param>
        static void Grab()
        {
            int errorcount = 0;
            if (searchterm == null)
            {
                Console.WriteLine("Enter search term");
                searchterm = Console.ReadLine();
            }
            List<HtmlNode> ProductList = new List<HtmlNode>();
            HtmlNode bidbuy;
            HttpClient httpClient = new HttpClient();
            HtmlDocument htmlDocument = new HtmlDocument();
            string html = string.Empty;
            //Grabs listings from specified number of pages
            for (int j = 1; j < pages; j++)
            {
                string Link = $"https://www.ebay.com/sch/i.html?_from=R40&_nkw={searchterm}&_sacat=0&_pgn={j}";
                //Grabs html document from page
                try
                {
                    html = httpClient.GetStringAsync(Link).Result;
                }
                catch (Exception)
                {
                    Thread.Sleep(5000);
                    html = httpClient.GetStringAsync(Link).Result;
                    errorcount++;
                    continue;

                }
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
            //Sends results to DataAccess class
            DataAccess.SaveResults(SearchTerm, listed);
            if (displayhold)
            {
                foreach (var item in listed)
                {
                    
                    Console.WriteLine(item.SaleType);
                    Console.WriteLine(item.Title);
                    Console.WriteLine(item.Price);
                    Console.WriteLine(item.Url);
                }
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine($"Last checked at {DateTime.Now} for {SearchTerm}");
            }
        }
    }
}
