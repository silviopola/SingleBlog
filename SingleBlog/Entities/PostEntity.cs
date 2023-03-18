using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleBlog.Entities
{
    [Table("Posts")]
    public class PostEntity
    {
        public PostEntity()
        {
            TagEntities = new List<TagEntity>();
        }
        
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }        
        public string Category { get; set; }
        public List<TagEntity> TagEntities { get; set; }
    }
}
