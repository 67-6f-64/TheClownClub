using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json.Serialization;
using Common;
using Common.Supreme;
using Common.Types;
using Newtonsoft.Json.Linq;
using Supreme.Tasks;

namespace Supreme {
    public class SupremeBot : Bot {
        [Browsable(false)] public MobileStock MobileStock { get; set; }

        private TimeSpan _checkoutDelay;

        [DisplayName("Checkout Delay"), Category("Configuration")]
        public double CheckoutDelay {
            get => _checkoutDelay.TotalMilliseconds;
            set {
                _checkoutDelay = TimeSpan.FromMilliseconds(value);
                OnPropertyChanged();
            }
        }

        [Browsable(false)] public Stopwatch DelayStopwatch;

        private MobileStockProduct _mobileStockProduct;

        [DisplayName("Product Name"), Category("Information")]
        public MobileStockProduct MobileStockProduct {
            get => _mobileStockProduct;
            set {
                _mobileStockProduct = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)] public SupremeProduct Product { get; set; }

        private Style _style;

        [DisplayName("Product Style"), Category("Information"), Newtonsoft.Json.JsonIgnore]
        public Style ProductStyle {
            get => _style;
            set {
                _style = value;
                OnPropertyChanged();
            }
        }

        private Size _size;

        [DisplayName("Product Size"), Category("Information"), JsonIgnore]
        public Size ProductSize {
            get => _size;
            set {
                _size = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)] public string CheckoutHtml { get; set; }
        [Browsable(false)] public string CsrfToken { get; set; }
        [Browsable(false)] public FormUrlEncodedContent CheckoutFormUrlEncodedContent { get; set; }
        [Browsable(false)] public JObject CheckoutJObject { get; set; }
        [Browsable(false)] public string CheckoutSlug { get; set; }
        [Browsable(false), JsonIgnore] public DateTime AtcTime { get; set; }

        [DisplayName("Captcha Bypass (US)"), Category("Configuration")]
        public bool CaptchaBypass { get; set; }

        public SupremeBot() : base(BotType.Supreme, new BillingProfile(), new SearchProduct()) {
            Append(new FindProductTask(this), new FindStyleAndSizeTask(this), new WaitForStockTask(this),
                new AddToCartTask(this),
                new ParseCheckoutTask(this), new CheckoutTask(this), new CheckoutQueueTask(this));
        }

        public SupremeBot(BillingProfile profile, SearchProduct product) : base(BotType.Supreme, profile, product) {
            Append(new FindProductTask(this), new FindStyleAndSizeTask(this), new WaitForStockTask(this),
                new AddToCartTask(this),
                new ParseCheckoutTask(this), new CheckoutTask(this), new CheckoutQueueTask(this));
        }

        public bool ShouldSerializeMobileStock() {
            return SerializeDebug;
        }

        public bool ShouldSerializeCheckoutHtml() {
            return SerializeDebug;
        }

        public bool ShouldSerializeCsrfToken() {
            return SerializeDebug;
        }

        public bool ShouldSerializeCheckoutFormUrlEncodedContent() {
            return SerializeDebug;
        }

        public bool ShouldSerializeCheckoutJObject() {
            return SerializeDebug;
        }

        public bool ShouldSerializeCheckoutSlug() {
            return SerializeDebug;
        }

        public bool ShouldSerializeProduct() {
            return SerializeDebug;
        }

        public bool ShouldSerializeMobileStockProduct() {
            return SerializeDebug;
        }
    }
}