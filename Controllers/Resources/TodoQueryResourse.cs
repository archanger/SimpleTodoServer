namespace TodoAPI.Controllers.Resources
{
    public class TodoQueryResourse
    {
        public int Limit { get; set; } = 10;
        public int Offset { get; set; } = 0;
    }
}