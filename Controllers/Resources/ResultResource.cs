using System.Collections.Generic;

namespace TodoAPI.Controllers.Resources
{
    public class ResultResource<T>
    {
        public long TotalItems { get; set; }
        public IEnumerable<T> data { get; set; }
    }
}