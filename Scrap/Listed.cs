using System;
using System.Collections.Generic;
using System.Text;

namespace Scrap
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
}
