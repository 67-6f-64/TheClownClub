using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Shopify {

    public partial class ProductInfo {
        [JsonProperty("product")] public Product Product { get; set; }
    }

    public partial class Product {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("body_html")] public string BodyHtml { get; set; }

        [JsonProperty("vendor")] public string Vendor { get; set; }

        [JsonProperty("product_type")] public string ProductType { get; set; }

        [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("handle")] public string Handle { get; set; }

        [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("published_at")] public DateTimeOffset PublishedAt { get; set; }

        [JsonProperty("template_suffix")] public string TemplateSuffix { get; set; }

        [JsonProperty("tags")] public string Tags { get; set; }

        [JsonProperty("published_scope")] public string PublishedScope { get; set; }

        [JsonProperty("variants")] public List<Variant> Variants { get; set; }

        [JsonProperty("options")] public List<Option> Options { get; set; }

        [JsonProperty("images")] public List<Image> Images { get; set; }

        [JsonProperty("image")] public Image Image { get; set; }
    }

    public partial class Image {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("product_id")] public long ProductId { get; set; }

        [JsonProperty("position")] public long Position { get; set; }

        [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("alt")] public string Alt { get; set; }

        [JsonProperty("width")] public long Width { get; set; }

        [JsonProperty("height")] public long Height { get; set; }

        [JsonProperty("src")] public Uri Src { get; set; }

        [JsonProperty("variant_ids")] public List<object> VariantIds { get; set; }
    }

    public partial class Option {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("product_id")] public long ProductId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("position")] public long Position { get; set; }

        [JsonProperty("values")] public List<string> Values { get; set; }
    }

    public partial class Variant {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("product_id")] public long ProductId { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("price")] public string Price { get; set; }

        [JsonProperty("sku")] public string Sku { get; set; }

        [JsonProperty("position")] public long Position { get; set; }

        [JsonProperty("inventory_policy")] public string InventoryPolicy { get; set; }

        [JsonProperty("compare_at_price")] public string CompareAtPrice { get; set; }

        [JsonProperty("fulfillment_service")] public string FulfillmentService { get; set; }

        [JsonProperty("inventory_management")] public string InventoryManagement { get; set; }

        [JsonProperty("option1")] public string Option1 { get; set; }

        [JsonProperty("option2")] public string Option2 { get; set; }

        [JsonProperty("option3")] public string Option3 { get; set; }

        [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("taxable")] public bool Taxable { get; set; }

        [JsonProperty("barcode")] public string Barcode { get; set; }

        [JsonProperty("grams")] public long Grams { get; set; }

        [JsonProperty("image_id")] public object ImageId { get; set; }

        [JsonProperty("weight")] public long Weight { get; set; }

        [JsonProperty("weight_unit")] public string WeightUnit { get; set; }

        [JsonProperty("inventory_item_id")] public long InventoryItemId { get; set; }

        [JsonProperty("inventory_quantity")] public long InventoryQuantity { get; set; }

        [JsonProperty("old_inventory_quantity")]
        public long OldInventoryQuantity { get; set; }

        [JsonProperty("tax_code")] public string TaxCode { get; set; }

        [JsonProperty("requires_shipping")] public bool RequiresShipping { get; set; }
    }

    public partial class ProductInfo {
        public static ProductInfo FromJson(string json) => JsonConvert.DeserializeObject<ProductInfo>(json);
    }
}