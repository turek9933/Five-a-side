namespace Five_a_side.Exceptions
{
    public class TeamException : Exception
    {
        public TeamException()
        {
        }
        public TeamException(string message) : base(message)
        {
        }
        public TeamException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
