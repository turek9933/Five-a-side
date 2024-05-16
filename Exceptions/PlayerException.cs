namespace Five_a_side.Exceptions
{
    public class PlayerException : Exception
    {
        public PlayerException()
        {
        }
        public PlayerException(string message) : base(message)
        {
        }
        public PlayerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
