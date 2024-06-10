using System.Text.Json.Serialization;

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

        public override string ToString()
        {
            return $"{Currency} ({Code}): {Mid}";
        }

        /*
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (CurrencyRate)obj;
            return Currency == other.Currency &&
                   Code == other.Code &&
                   Mid == other.Mid;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Currency, Code, Mid);
        }
        */
    }
}
