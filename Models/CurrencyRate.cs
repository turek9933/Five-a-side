using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Five_a_side.Exceptions;

namespace Five_a_side.Models
{
    public class CurrencyRate
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("mid")]
        public decimal Mid { get; set; }

        public CurrencyRate(string currency, string code, decimal mid)
        {
            Currency = currency;
            Code = code;
            Mid = mid;
        }
    }
}