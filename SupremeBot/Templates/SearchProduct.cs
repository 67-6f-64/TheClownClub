using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupremeBot.Templates {
    public class SearchProduct {
        public bool Delete = true;
        public List<string> ProductKeywords = new List<string>();
        public List<string> StyleKeywords = new List<string>();
        public List<string> SizeKeywords = new List<string>();

        public SearchProduct(List<string> product, List<string> style, List<string> size, bool deleteWhenFound = true) {
            ProductKeywords = product;
            StyleKeywords = style;
            SizeKeywords = size;
            Delete = deleteWhenFound;
        }
    }
}
