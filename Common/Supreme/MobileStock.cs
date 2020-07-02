using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Common.Supreme {
    public partial class MobileStock {
        [JsonProperty("unique_image_url_prefixes")]
        public List<object> UniqueImageUrlPrefixes { get; set; }

        [JsonProperty("products_and_categories")]
        public Dictionary<string, List<MobileStockProduct>> ProductsAndCategories { get; set; }

        [JsonProperty("last_mobile_api_update")]
        public DateTimeOffset LastMobileApiUpdate { get; set; }

        [JsonProperty("release_date")] public string ReleaseDate { get; set; }

        [JsonProperty("release_week")] public string ReleaseWeek { get; set; }

        public object Clone() {
            return MemberwiseClone();
        }
    }

    public class Style {
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

        public override string ToString() {
            return Name;
        }
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

        public override string ToString() {
            return Name;
        }
    }

    public class MobileStockProduct {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("image_url")] public string ImageUrl { get; set; }

        [JsonProperty("image_url_hi")] public string ImageUrlHi { get; set; }

        [JsonProperty("price")] public long Price { get; set; }

        [JsonProperty("sale_price")] public long SalePrice { get; set; }

        [JsonProperty("new_item")] public bool NewItem { get; set; }

        [JsonProperty("position")] public long Position { get; set; }

        [JsonProperty("category_name")] public string CategoryName { get; set; }

        [JsonProperty("price_euro")] public long? PriceEuro { get; set; }

        [JsonProperty("sale_price_euro")] public long? SalePriceEuro { get; set; }

        public override string ToString() {
            return Name;
        }
    }

    public partial class MobileStock {
        public static MobileStock FromJson(string json) => JsonConvert.DeserializeObject<MobileStock>(json,
            new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Ignore});
    }
}
