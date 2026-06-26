using DynamicTranslate.Attribute;
using DynamicTranslate.Demo.DTO;
using DynamicTranslate.Demo.Infrastructure;
using GTranslate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace DynamicTranslate.Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DemoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("categories-with-details-with-auto-translation")]
        public async Task<IActionResult> GetAllCategoriesWithDetails([FromQuery] string lang) //just to pass language from swagger
        {
            Request.Headers.AcceptLanguage = lang;

            var categories = await _context.LookupCategories
                .Include(m => m.LookupMasters)
                .ThenInclude(d => d.LookupDetails)
                .Select(m => new LookupCategoryDTO
                {
                    Id = m.Id,
                    Name = m.Name, // Will be translated
                    Lookups = m.LookupMasters.Select(d => new LookupMasterDTO
                    {
                        Id = d.Id,
                        Name = d.Name, // Will be translated
                        Details = d.LookupDetails.Select(d => new LookupDetailDTO
                        {
                            Id = d.Id,
                            Name = d.Name // Will be translated
                        }).ToArray(),
                    }).ToArray()
                })
                .ToListAsync();

            return Ok(categories);
        }

        //demonstrate translated business validation message
        [HttpPost("create-order-with-dynamic-business-error-translation")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] Order order, [FromQuery] string lang) //just to pass language from swagger
        {
            Request.Headers.AcceptLanguage = lang;

            var businessErrors = new List<(string Code,  string Error)>(
             [
                new("order_not_found", "the requested order does not exist"),
                new("order_already_cancelled", "this order has already been cancelled"),
                new("order_already_shipped", "cannot modify an order that has already been shipped"),
                new("order_already_delivered", "cannot modify an order that has already been delivered"),
                new("order_payment_pending", "order cannot be processed due to pending payment"),
                new("order_payment_failed", "payment for this order has failed"),
                new("order_payment_expired", "payment window has expired for this order"),
                new("order_quantity_exceeds_stock", "requested quantity exceeds available stock"),
                new("order_quantity_below_minimum", "order quantity is below the minimum required"),
                new("order_quantity_above_maximum", "order quantity exceeds the maximum allowed"),
                new("order_item_not_available", "one or more items in the order are no longer available"),
                new("order_invalid_status_transition", "invalid status transition for this order"),
                new("order_customer_not_verified", "customer account must be verified to place an order"),
                new("order_shipping_address_invalid", "shipping address is invalid or incomplete"),
                new("order_billing_address_invalid", "billing address is invalid or incomplete"),
                new("order_total_amount_mismatch", "order total amount does not match calculated total"),
                new("order_discount_invalid", "discount code is invalid or expired"),
                new("order_currency_mismatch", "currency does not match the store's base currency"),
                new("order_customer_exceeds_credit_limit", "customer has exceeded their credit limit"),
                new("order_duplicate_order", "duplicate order detected, already exists within the last 24 hours"),
                new("order_restricted_item", "order contains restricted or prohibited items"),
                new("order_age_restricted", "order contains age-restricted items and customer age cannot be verified"),
                new("order_delivery_unavailable", "delivery is not available for the specified location"),
                new("order_tax_calculation_error", "error calculating taxes for this order"),
                new("order_shipping_method_unavailable", "selected shipping method is not available"),
                new("order_promotion_conflict", "promotion cannot be applied with existing discounts"),
                new("order_gift_wrap_unavailable", "gift wrap service is currently unavailable"),
                new("order_rush_delivery_unavailable", "rush delivery is not available for this order"),
                new("order_partial_cancellation_denied", "partial cancellation is not allowed for this order"),
                new("order_modification_window_closed", "order modification window has closed"),   ]);

            var selectedError = businessErrors[Random.Shared.Next(0, businessErrors.Count)];
            return Ok(new ApiResponse(false, selectedError.Code,selectedError.Error));
        }





    }

    public class Order
    {
        public string Customer { get; set; }
        public string Item { get; set; }
        public int Qty { get; set; }
    }

    public class ApiResponse
    {
        public ApiResponse(bool isSuccess, string code, string error)
        {
            IsSuccess = isSuccess;
            Code = code;
            Error = error;
        }

        public bool IsSuccess { get; set; }

        [JsonIgnore]
        public string Code { get; set; }

        [Translate(nameof(Code))]
        public string Error { get; set; }
    }
}
