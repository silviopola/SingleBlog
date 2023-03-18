using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingleBlog.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SingleBlog.Entities;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SingleBlog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly SingleBlogDBContext _dbContext;
        private readonly IConfiguration _Configuration;

        const string AllowedImageExtension = ".png";


        public PostsController(SingleBlogDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _Configuration = configuration;
        }
                
        // GET Posts with optional filters (title and/or category and/or tag)
        [HttpGet]
        public async Task<ActionResult<List<ResponsePost>>> GetPostsFilteredAsync([FromQuery] string titleFilter, [FromQuery] string categoryFilter, [FromQuery] string tagFilter)
        {
            var allPosts = await _dbContext.Posts.Include(pe => pe.TagEntities).ToListAsync();

            if (!string.IsNullOrEmpty(titleFilter))
                allPosts = allPosts.Where(post => string.Equals(post.Title, titleFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(categoryFilter))
                allPosts = allPosts.Where(post => post.Category == categoryFilter).ToList();

            if (!string.IsNullOrEmpty(tagFilter))
                allPosts = allPosts.Where(post => post.TagEntities.Any(x => x.Name == tagFilter)).ToList();

            return Ok(allPosts.ConvertAll(pe => new ResponsePost { Id = pe.Id, Title = pe.Title, Author = pe.Author, Category = pe.Category, Content = pe.Content, Tags = pe.TagEntities.ConvertAll(te => te.Name) }));
        }

        // GET a Post by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponsePost>> GetPostAsync(int id)
        {
            var post = await _dbContext.Posts.Include(pe => pe.TagEntities).SingleOrDefaultAsync(pe => pe.Id == id);
            if (post == null)
                return NotFound($"Post Id={id} not found");

            return Ok(new ResponsePost { Id = post.Id, Title = post.Title, Author = post.Author, Category = post.Category, Content = post.Content, Tags = post.TagEntities.ConvertAll(te => te.Name) });
        }

        // POST a new Post
        [HttpPost]
        public async Task<IActionResult> AddPostAsync([FromBody]RequestPost post)
        {
            var validationResult = RequestValidator.ValidateFields(post);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var posEntity = new PostEntity { Title = post.Title, Author = post.Author, Category = post.Category, Content = post.Content };
            await _dbContext.Posts.AddAsync(posEntity);
            await _dbContext.SaveChangesAsync();

            return Ok(posEntity.Id);
        }

        // Full REPLACE a Post by Id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePostAsync(int id, RequestPost post)
        {
            var validationResult = RequestValidator.ValidateFields(post);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var postToUpdate = await _dbContext.Posts.Include(pe => pe.TagEntities).SingleOrDefaultAsync(pe => pe.Id == id);
            if (postToUpdate == null)
                return NotFound($"Post Id={id} not found");

            postToUpdate.Title = post.Title;
            postToUpdate.Author = post.Author;
            postToUpdate.Content = post.Content;
            postToUpdate.Category = post.Category;
            
            _dbContext.Posts.Update(postToUpdate);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        // Partial REPLACE a Post by Id
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchPostAsync(int id, RequestPost post)
        {
            var validationResult = RequestValidator.ValidateFieldsIfNotNull(post);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var postToPatch = await _dbContext.Posts.SingleOrDefaultAsync(post => post.Id == id);
            if (postToPatch == null)
                return NotFound($"Post Id={id} not found");
            
            postToPatch.Title = post.Title ?? postToPatch.Title;
            postToPatch.Author = post.Author ?? postToPatch.Author;
            postToPatch.Content = post.Content ?? postToPatch.Content;
            postToPatch.Category = post.Category ?? postToPatch.Category;

            _dbContext.Posts.Update(postToPatch);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        // DELETE a Post by ID (Need ADMIN Role Token => "ADMIN_TOKEN")
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePostAsync(int id/*, [FromHeader] string roleToken*/)
        {
            Request.Headers.TryGetValue("AdminRoleToken", out var token);
            
            var adminRoleToken = _Configuration.GetValue<string>("AdminRoleToken");
            if (token != adminRoleToken)
                return Unauthorized();

            var postToRemove = await _dbContext.Posts.SingleOrDefaultAsync(pe => pe.Id == id);
            if (postToRemove == null)
                return NotFound($"Post Id={id} not found");

            _dbContext.Posts.Remove(postToRemove);
            await _dbContext.SaveChangesAsync();

            const string allowedImageExtension = ".png";
            var filename = string.Concat(id.ToString(), allowedImageExtension);
            var filePath = Path.Combine(PathUtils.ImagesContentRootPath, filename);
            
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            return Ok();
        }
        
        // Post Image in a Post (allowed only "PNG" to simplify), null imageFile delete current assigned image if already present
        [HttpPost("{id}/Image")]
        public async Task<IActionResult> UpdateImageAsync(int id, IFormFile imagefile)
        {
            var postExist = await _dbContext.Posts.AnyAsync(pe => pe.Id == id);
            if (!postExist)
                return NotFound($"Post Id={id} not found");
            
            if (imagefile == null)
                return BadRequest("Image is empty");
       
            if (!imagefile.FileName.EndsWith(AllowedImageExtension, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest("Image with bad extension, allowed *.png");

            var filePath = GetImageFSPath(id);

            using var filestream = new FileStream(filePath, FileMode.Create);
            await imagefile.CopyToAsync(filestream);

            return Ok();
        }

        // Post Image in a Post (allowed only "PNG" to simplify), null imageFile delete current assigned image if already present
        [HttpDelete("{id}/Image")]
        public async Task<IActionResult> DeleteImageAsync(int id)
        {
            var postExist = await _dbContext.Posts.AnyAsync(pe => pe.Id == id);
            if (!postExist)
                return NotFound($"Post Id={id} not found");

            var filePath = GetImageFSPath(id);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"Image of Post Id={id} not found");
            
            System.IO.File.Delete(filePath);

            return Ok();
        }

        // Post Image in a Post (allowed only "PNG" to simplify), null imageFile delete current assigned image if already present
        [HttpGet("{id}/Image")]
        public async Task<IActionResult> GetImageAsync(int id)
        {
            var filePath = GetImageFSPath(id);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"Image for Post Id={id} not found");

            var imageByteArray = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(imageByteArray, "image/png");
        }

        //POST a new Tag in a POST tags collection
        [HttpPost("{id}/Tags")]
        public async Task<IActionResult> AddtTagToPostAsync(int id, [FromBody] string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return BadRequest("Empty Tag");
            
            var post = await _dbContext.Posts.Include(pe => pe.TagEntities).SingleOrDefaultAsync(pe => pe.Id == id);
            if (post == null)
                return NotFound($"Post Id={id} not found ");

            if (post.TagEntities.Exists(te => te.Name == tag))
                return Ok();

            post.TagEntities.Add(new TagEntity { Name = tag });
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        //DELETE a Tag in a POST tags collection
        [HttpDelete("{id}/Tags/{tag}")]
        public async Task<IActionResult> DeletetTagInPostAsync(int id, string tag)
        {
            var post = await _dbContext.Posts.Include(pe => pe.TagEntities).SingleOrDefaultAsync(pe => pe.Id == id);
            if (post == null)
                return NotFound($"Post Id={id} not found ");

            var tagEntity = post.TagEntities.SingleOrDefault(te => te.Name == tag);
            if (tagEntity == null)
                return NotFound($"Tag \"{tag}\" in Post Id={id} not found");

            post.TagEntities.Remove(tagEntity);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }


        private string GetImageFSPath(int id)
        {
            var filename = string.Concat(id.ToString(), AllowedImageExtension);
            return Path.Combine(PathUtils.ImagesContentRootPath, filename);
        }

    }
}
