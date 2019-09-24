using System.Net.Http;
using System.Threading;
using Common.Supreme;

namespace Common {
    public class SupremeUsBot : Bot {
        public SearchProduct SearchProduct;
        public MobileStock MobileStock;
        public MobileStockProduct MobileStockProduct;
        public Product Product;
        public Style ProductStyle;
        public Size ProductSize;
        public string CheckoutHtml;
        public string CsrfToken;
        public FormUrlEncodedContent CheckoutFormUrlEncodedContent;

        public SupremeUsBot(SearchProduct product, CancellationToken token) : base(BotType.SupremeUs, token) {
            SearchProduct = product;
            Append(new FindProductTask(this), new FindStyleAndSizeTask(this), new AddToCartTask(this),
                new ParseCheckoutTask(this), new CheckoutTask(this));
        }
    }
}