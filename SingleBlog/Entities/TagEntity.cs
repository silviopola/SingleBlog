using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleBlog.Entities
{

    [Table("Tags")]
    public class TagEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
