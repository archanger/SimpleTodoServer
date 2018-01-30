using System.Collections.Generic;

namespace TodoAPI.Core.Models
{
    public class Result<T>
    {
        public long TotalItems { get; set; }
        public IEnumerable<T> data { get; set; }
    }
}