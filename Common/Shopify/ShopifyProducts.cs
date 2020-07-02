using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Common.Shopify {
    public partial class ShopifyProducts {
        [JsonProperty("products")]
        public List<ShopifyProduct> ProductsList { get; set; }
    }

    public partial class ShopifyProduct {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("product_type")]
        public string ProductType { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("variants")]
        public List<ShopifyVariant> Variants { get; set; }

        [JsonProperty("images")]
        public List<ShopifyImage> Images { get; set; }

        [JsonProperty("options")]
        public List<ShopifyOption> Options { get; set; }
    }

    public partial class ShopifyImage {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        [JsonProperty("variant_ids")]
        public List<object> VariantIds { get; set; }

        [JsonProperty("src")]
        public Uri Src { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }
    }

    public partial class ShopifyOption {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("values")]
        public List<string> Values { get; set; }
    }

    public partial class ShopifyVariant {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("option1")]
        public string Option1 { get; set; }

        [JsonProperty("option2")]
        public string Option2 { get; set; }

        [JsonProperty("option3")]
        public string Option3 { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("requires_shipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("taxable")]
        public bool Taxable { get; set; }

        [JsonProperty("featured_image")]
        public object FeaturedImage { get; set; }

        [JsonProperty("available")]
        public bool Available { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("grams")]
        public long Grams { get; set; }

        [JsonProperty("compare_at_price")]
        public string CompareAtPrice { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public partial class ShopifyProducts {
        public static ShopifyProducts FromJson(string json) => JsonConvert.DeserializeObject<ShopifyProducts>(json);
    }
}
