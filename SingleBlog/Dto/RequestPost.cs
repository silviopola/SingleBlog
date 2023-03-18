using System.Collections.Generic;

namespace SingleBlog.Dto
{
    public class RequestPost
    {  
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
    }
}
