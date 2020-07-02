using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common.Supreme {
    public class SearchProduct : INotifyPropertyChanged {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        [DisplayName("Category"), ReadOnly(true)]
        public string Category { get; set; }
        private List<string> _productKeywords;
        [DisplayName("Product Keywords")]
        public List<string> ProductKeywords {
            get => _productKeywords;
            set {
                _productKeywords = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Any Style?")]
        public bool AnyStyle { get; set; }

        private List<string> _styleKeywords;
        [DisplayName("Style Keywords")]
        public List<string> StyleKeywords {
            get => _styleKeywords;
            set {
                _styleKeywords = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Any Size?")]
        public bool AnySize { get; set; }
        
        private string _sizeKeyword;

        [DisplayName("Size Keyword")]
        public string SizeKeyword {
            get => _sizeKeyword;
            set {
                _sizeKeyword = value;
                OnPropertyChanged();
            }
        }

        public SearchProduct() {

        }

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

        public override string ToString() {
            if (ProductKeywords.Count().Equals(1)) return ProductKeywords.First();
            
            var stringBuilder = new StringBuilder();
            foreach (var keyword in ProductKeywords) {
                stringBuilder.Append(ProductKeywords.Last().Equals(keyword) ? $"{keyword}" : $"{keyword}, ");
            }

            return stringBuilder.ToString();
        }
    }
}
