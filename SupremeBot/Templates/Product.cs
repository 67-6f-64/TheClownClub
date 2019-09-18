using Newtonsoft.Json;
using System.Collections.Generic;

namespace SupremeBot.Templates {
    public partial class Product {
        [JsonProperty("styles")]
        public List<Style> Styles { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("can_add_styles")]
        public bool CanAddStyles { get; set; }

        [JsonProperty("can_buy_multiple")]
        public bool CanBuyMultiple { get; set; }

        [JsonProperty("ino")]
        public string Ino { get; set; }

        [JsonProperty("cod_blocked")]
        public bool CodBlocked { get; set; }

        [JsonProperty("canada_blocked")]
        public bool CanadaBlocked { get; set; }

        [JsonProperty("purchasable_qty")]
        public long PurchasableQty { get; set; }

        [JsonProperty("new_item")]
        public bool NewItem { get; set; }

        [JsonProperty("apparel")]
        public bool Apparel { get; set; }

        [JsonProperty("handling")]
        public long Handling { get; set; }

        [JsonProperty("no_free_shipping")]
        public bool NoFreeShipping { get; set; }

        [JsonProperty("can_buy_multiple_with_limit")]
        public long CanBuyMultipleWithLimit { get; set; }
    }

    public partial class Style {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("image_url_hi")]
        public string ImageUrlHi { get; set; }

        [JsonProperty("swatch_url")]
        public string SwatchUrl { get; set; }

        [JsonProperty("swatch_url_hi")]
        public string SwatchUrlHi { get; set; }

        [JsonProperty("mobile_zoomed_url")]
        public string MobileZoomedUrl { get; set; }

        [JsonProperty("mobile_zoomed_url_hi")]
        public string MobileZoomedUrlHi { get; set; }

        [JsonProperty("bigger_zoomed_url")]
        public string BiggerZoomedUrl { get; set; }

        [JsonProperty("sizes")]
        public List<Size> Sizes { get; set; }

        [JsonProperty("additional")]
        public List<Additional> Additional { get; set; }
    }

    public partial class Additional {
        [JsonProperty("swatch_url")]
        public string SwatchUrl { get; set; }

        [JsonProperty("swatch_url_hi")]
        public string SwatchUrlHi { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("image_url_hi")]
        public string ImageUrlHi { get; set; }

        [JsonProperty("zoomed_url")]
        public string ZoomedUrl { get; set; }

        [JsonProperty("zoomed_url_hi")]
        public string ZoomedUrlHi { get; set; }

        [JsonProperty("bigger_zoomed_url")]
        public string BiggerZoomedUrl { get; set; }
    }

    public partial class Size {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("stock_level")]
        public long StockLevel { get; set; }
    }

    public partial class Product {
        public static Product FromJson(string json) => JsonConvert.DeserializeObject<Product>(json);
    }
}
