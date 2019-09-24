using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Supreme {
    public class SearchProduct {
        public string Category;
        public List<string> ProductKeywords;
        public bool AnyStyle;
        public List<string> StyleKeywords;
        public bool AnySize;
        public string SizeKeyword;

        public SearchProduct(List<string> product) {
            Category = "new";
            ProductKeywords = product;
            AnyStyle = true;
            AnySize = true;
        }

        public SearchProduct(List<string> product, List<string> style) {
            Category = "new";
            ProductKeywords = product;
            StyleKeywords = style;
            AnySize = true;
        }

        public SearchProduct(List<string> product, string size) {
            Category = "new";
            ProductKeywords = product;
            SizeKeyword = size;
            AnySize = true;
        }

        public SearchProduct(List<string> product, List<string> style, string size) {
            Category = "new";
            ProductKeywords = product;
            StyleKeywords = style;
            SizeKeyword = size;
            AnyStyle = false;
            AnySize = false;
        }

        public SearchProduct(string category, List<string> product) {
            Category = category;
            ProductKeywords = product;
            AnyStyle = true;
            AnySize = true;
        }

        public SearchProduct(string category, List<string> product, List<string> style) {
            Category = category;
            ProductKeywords = product;
            StyleKeywords = style;
            AnySize = true;
        }

        public SearchProduct(string category, List<string> product, string size) {
            Category = category;
            ProductKeywords = product;
            SizeKeyword = size;
            AnySize = true;
        }

        public SearchProduct(string category, List<string> product, List<string> style, string size) {
            Category = category;
            ProductKeywords = product;
            StyleKeywords = style;
            SizeKeyword = size;
            AnyStyle = false;
            AnySize = false;
        }
    }
}
