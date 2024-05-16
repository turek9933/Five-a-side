namespace Five_a_side.Exceptions
{
    public class CurrencyControllerException : Exception
    {
        public CurrencyControllerException()
        {
        }
        public CurrencyControllerException(string message) : base(message)
        {
        }
        public CurrencyControllerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
