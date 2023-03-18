using System.Collections.Generic;

namespace SingleBlog.Dto
{
    public class ResponsePost : RequestPost
    {
        public ResponsePost()
        {
            Tags = new List<string>();
        }

        public int Id { get; set; }
        public List<string> Tags { get; set; }
    }
}
