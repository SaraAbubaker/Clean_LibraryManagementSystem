namespace Library.Common.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message) : base(message) { }
    }
}
