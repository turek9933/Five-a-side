namespace Five_a_side.Exceptions
{
    public class TeamsControllerException : Exception
    {
        public TeamsControllerException()
        {
        }
        public TeamsControllerException(string message) : base(message)
        {
        }
        public TeamsControllerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
