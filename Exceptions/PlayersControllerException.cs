namespace Five_a_side.Exceptions
{
    public class PlayersControllerException : Exception
    {
        public PlayersControllerException()
        {
        }
        public PlayersControllerException(string message) : base(message)
        {
        }
        public PlayersControllerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
