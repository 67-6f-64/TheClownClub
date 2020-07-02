using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Shopify {
    namespace Shopify {
        using System;
        using System.Collections.Generic;

        using System.Globalization;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Converters;

        public partial class ShopifyCart {
            [JsonProperty("token")] public string Token { get; set; }

            [JsonProperty("note")] public object Note { get; set; }

            [JsonProperty("attributes")] public /* Attributes */ object Attributes { get; set; }

            [JsonProperty("original_total_price")] public long OriginalTotalPrice { get; set; }

            [JsonProperty("total_price")] public long TotalPrice { get; set; }

            [JsonProperty("total_discount")] public long TotalDiscount { get; set; }

            [JsonProperty("total_weight")] public double TotalWeight { get; set; }

            [JsonProperty("item_count")] public long ItemCount { get; set; }

            [JsonProperty("items")] public List<Item> Items { get; set; }

            [JsonProperty("requires_shipping")] public bool RequiresShipping { get; set; }

            [JsonProperty("currency")] public string Currency { get; set; }

            [JsonProperty("items_subtotal_price")] public long ItemsSubtotalPrice { get; set; }

            [JsonProperty("cart_level_discount_applications")]
            public List<object> CartLevelDiscountApplications { get; set; }
        }

        public partial class Attributes { }

        public partial class Item {
            [JsonProperty("id")] public long Id { get; set; }

            [JsonProperty("properties")] public object Properties { get; set; }

            [JsonProperty("quantity")] public long Quantity { get; set; }

            [JsonProperty("variant_id")] public long VariantId { get; set; }

            [JsonProperty("key")] public string Key { get; set; }

            [JsonProperty("title")] public string Title { get; set; }

            [JsonProperty("price")] public long Price { get; set; }

            [JsonProperty("original_price")] public long OriginalPrice { get; set; }

            [JsonProperty("discounted_price")] public long DiscountedPrice { get; set; }

            [JsonProperty("line_price")] public long LinePrice { get; set; }

            [JsonProperty("original_line_price")] public long OriginalLinePrice { get; set; }

            [JsonProperty("total_discount")] public long TotalDiscount { get; set; }

            [JsonProperty("discounts")] public List<object> Discounts { get; set; }

            [JsonProperty("sku")] public string Sku { get; set; }

            [JsonProperty("grams")] public long Grams { get; set; }

            [JsonProperty("vendor")] public string Vendor { get; set; }

            [JsonProperty("taxable")] public bool Taxable { get; set; }

            [JsonProperty("product_id")] public long ProductId { get; set; }

            [JsonProperty("product_has_only_default_variant")]
            public bool ProductHasOnlyDefaultVariant { get; set; }

            [JsonProperty("gift_card")] public bool GiftCard { get; set; }

            [JsonProperty("final_price")] public long FinalPrice { get; set; }

            [JsonProperty("final_line_price")] public long FinalLinePrice { get; set; }

            [JsonProperty("url")] public string Url { get; set; }

            [JsonProperty("featured_image")] public FeaturedImage FeaturedImage { get; set; }

            [JsonProperty("image")] public Uri Image { get; set; }

            [JsonProperty("handle")] public string Handle { get; set; }

            [JsonProperty("requires_shipping")] public bool RequiresShipping { get; set; }

            [JsonProperty("product_type")] public string ProductType { get; set; }

            [JsonProperty("product_title")] public string ProductTitle { get; set; }

            [JsonProperty("product_description")] public string ProductDescription { get; set; }

            [JsonProperty("variant_title")] public string VariantTitle { get; set; }

            [JsonProperty("variant_options")] public List<string> VariantOptions { get; set; }

            [JsonProperty("options_with_values")] public List<OptionsWithValue> OptionsWithValues { get; set; }

            [JsonProperty("line_level_discount_allocations")]
            public List<object> LineLevelDiscountAllocations { get; set; }
        }

        public partial class FeaturedImage {
            [JsonProperty("url")] public Uri Url { get; set; }

            [JsonProperty("aspect_ratio")] public long AspectRatio { get; set; }

            [JsonProperty("alt")] public string Alt { get; set; }
        }

        public partial class OptionsWithValue {
            [JsonProperty("name")] public string Name { get; set; }

            [JsonProperty("value")] public string Value { get; set; }
        }

        public partial class ShopifyCart {
            public static ShopifyCart FromJson(string json) => JsonConvert.DeserializeObject<ShopifyCart>(json);
        }
    }
}
