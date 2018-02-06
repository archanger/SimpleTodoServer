namespace TodoAPI.Errors
{
    public class ErrorWithMessage
    {
        public string Error { get; set; }
    }

    public class ErrorGeneric<T>
    {
        public T Error { get; set; }
    }
}