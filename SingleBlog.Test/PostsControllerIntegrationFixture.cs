using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using SingleBlog.Dto;
using System.IO;
using SingleBlog.Entities;
using System.Collections.Generic;
using System;
using System.Collections;

namespace SingleBlog.Test
{
    [TestFixture]
    public class PostsControllerIntegrationFixture
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;

        [SetUp]
        public void GivenARequestToTheController()
        {
            //Destroy Database and Images
            DestroyDbAndImages();

            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();

            //Destroy Database and Images
            DestroyDbAndImages();
        }

        //POST Tests
        [Test]
        public async Task PostWithEmptyTitleReturnsBadRequest()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            requestPost.Title = string.Empty;
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Title is empty"));
        }

        [Test]
        public async Task PostWithEmptyAuthorReturnsBadRequest()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            requestPost.Author = string.Empty;
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Author is empty"));
        }

        [Test]
        public async Task PostWithEmptyContentReturnsBadRequest()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            requestPost.Content = string.Empty;
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Content is empty"));
        }

        [Test]
        public async Task PostWithContentExceeding1024CharsReturnsBadRequest()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            requestPost.Content = new string('A', 1024);
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            requestPost.Content = new string('A', 1025);
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Content exceed the max length of 1024 chars"));
        }

        [Test]
        public async Task PostAdGetTheOnlyInsertedPost()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("1"));

            response = await _client.GetAsync("/Posts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));

            Assert.That(posts[0].Id, Is.EqualTo(1));
            Assert.That(posts[0].Title, Is.EqualTo(requestPost.Title));
            Assert.That(posts[0].Author, Is.EqualTo(requestPost.Author));
            Assert.That(posts[0].Content, Is.EqualTo(requestPost.Content));
            Assert.That(posts[0].Category, Is.EqualTo(requestPost.Category));
        }

        //GET Tests

        [Test]
        public async Task GetPostsWhenDbIsEmptyReturnsEmptyPostsCollection()
        {
            var response = await _client.GetAsync("/Posts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetFilteringByTitleReturnsMatchedPosts()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            requestPost = new RequestPost { Title = "Title2", Author = "Author2", Content = "Content2", Category = "Category2" };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            requestPost = new RequestPost { Title = "Title1", Author = "Author3", Content = "Content3", Category = "Category3" };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync("/Posts?titlefilter=Title1");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetFilteringByCategoryReturnsMatchedPosts()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            requestPost = new RequestPost { Title = "Title2", Author = "Author2", Content = "Content2", Category = null };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content3", Category = "Category1" };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync("/Posts?categoryfilter=Category1");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetFilteringByTagReturnsMatchedPosts()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Bad"));


            response = await _client.GetAsync("/Posts?tagfilter=Good");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetFilteringByTagReturnsMatchedPostsWithMoreThanOneTaginEachPost()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Bad"));

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Bad"));
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));


            response = await _client.GetAsync("/Posts?tagfilter=Good");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(2));

            response = await _client.GetAsync("/Posts?tagfilter=Bad");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetFilteringByManyFiltersReturnsMatchedPostsUsingANDClausule()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            requestPost = new RequestPost { Title = "Title2", Author = "Author1", Content = "Content1", Category = "Category1" };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category2" };
            response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync("/Posts?tagFilter=Good&titleFilter=Title1&categoryFilter=Category1");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetPostByIdRetunsNotFoundIfitIsNoPresent()
        {
            var response = await _client.GetAsync("/Posts/123");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetPostByIdReturnsPostWithTheRelatedId()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync($"/Posts/{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var post = JsonConvert.DeserializeObject<ResponsePost>(await response.Content.ReadAsStringAsync());

            Assert.That(post.Id, Is.EqualTo(id));
            Assert.That(post.Title, Is.EqualTo(requestPost.Title));
            Assert.That(post.Author, Is.EqualTo(requestPost.Author));
            Assert.That(post.Content, Is.EqualTo(requestPost.Content));
            Assert.That(post.Category, Is.EqualTo(requestPost.Category));
            Assert.That(post.Tags.Count, Is.EqualTo(1));
            Assert.That(post.Tags[0], Is.EqualTo("Good"));
        }

        //PUT Tests

        [Test]
        public async Task UpdatePostReplacesTheWholePost()
        {
            var requestPost = new RequestPost { Title = "Title5", Author = "Author7", Content = "Content5", Category = "Category9" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var newRequestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            await _client.PutAsync($"/Posts/{id}", GetStringContent(newRequestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync($"/Posts/{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var post = JsonConvert.DeserializeObject<ResponsePost>(await response.Content.ReadAsStringAsync());

            Assert.That(post.Id, Is.EqualTo(id));
            Assert.That(post.Title, Is.EqualTo(newRequestPost.Title));
            Assert.That(post.Author, Is.EqualTo(newRequestPost.Author));
            Assert.That(post.Content, Is.EqualTo(newRequestPost.Content));
            Assert.That(post.Category, Is.EqualTo(newRequestPost.Category));
            Assert.That(post.Tags.Count, Is.EqualTo(1));
            Assert.That(post.Tags[0], Is.EqualTo("Good"));
        }

        [Test]
        public async Task UpdateWithNotPresentIdReturnsNotFound()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PutAsync($"/Posts/999", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Post Id=999 not found"));
        }

        [Test]
        public async Task UpdateWithEmptyTitleReturnsBadRequest()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync($"/Posts/", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            requestPost.Title = string.Empty;
            response = await _client.PutAsync($"/Posts/{id}", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Title is empty"));
        }

        [Test]
        public async Task UpdateWithEmptyAuthorReturnsBadRequest()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync($"/Posts/", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            requestPost.Author = string.Empty;
            response = await _client.PutAsync($"/Posts/{id}", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Author is empty"));
        }

        [Test]
        public async Task UpdateWithEmptyContentReturnsBadRequest()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync($"/Posts/", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            requestPost.Content = string.Empty;
            response = await _client.PutAsync($"/Posts/{id}", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Content is empty"));
        }

        //PATCH Tests

        [Test]
        public async Task PartialUpdateWithNotPresentIdReturnBadRequest()
        {
            var requestPost = new RequestPost { Title = "", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PatchAsync($"/Posts/999", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task PartialUpdateWithNotPresentIdReturnsNotFound()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PatchAsync($"/Posts/999", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Post Id=999 not found"));
        }

        [Test]
        public async Task PartialUpdateUpdatesOnlyNotNullFields_TitleCase()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync($"/Posts/", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            var newRequestPost = new RequestPost { Title = "NewTitle", Author = null, Content = null, Category = null };
            response = await _client.PatchAsync($"/Posts/{id}", GetStringContent(newRequestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync($"/Posts?{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));

            Assert.That(posts[0].Id, Is.EqualTo(id));
            Assert.That(posts[0].Title, Is.EqualTo(newRequestPost.Title));
            Assert.That(posts[0].Author, Is.EqualTo(requestPost.Author));
            Assert.That(posts[0].Content, Is.EqualTo(requestPost.Content));
            Assert.That(posts[0].Category, Is.EqualTo(requestPost.Category));
        }

        [Test]
        public async Task PartialUpdateUpdatesOnlyNotNullFields_AuthorCase()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync($"/Posts/", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            var newRequestPost = new RequestPost { Title = null, Author = "NewAuthor", Content = null, Category = null };
            response = await _client.PatchAsync($"/Posts/{id}", GetStringContent(newRequestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync($"/Posts?{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));

            Assert.That(posts[0].Id, Is.EqualTo(id));
            Assert.That(posts[0].Title, Is.EqualTo(requestPost.Title));
            Assert.That(posts[0].Author, Is.EqualTo(newRequestPost.Author));
            Assert.That(posts[0].Content, Is.EqualTo(requestPost.Content));
            Assert.That(posts[0].Category, Is.EqualTo(requestPost.Category));
        }

        [Test]
        public async Task PartialUpdateUpdatesOnlyNotNullFields_ContentCase()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync($"/Posts/", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            var newRequestPost = new RequestPost { Title = null, Author = null, Content = "NewContent", Category = null };
            response = await _client.PatchAsync($"/Posts/{id}", GetStringContent(newRequestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync($"/Posts?{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));

            Assert.That(posts[0].Id, Is.EqualTo(id));
            Assert.That(posts[0].Title, Is.EqualTo(requestPost.Title));
            Assert.That(posts[0].Author, Is.EqualTo(requestPost.Author));
            Assert.That(posts[0].Content, Is.EqualTo(newRequestPost.Content));
            Assert.That(posts[0].Category, Is.EqualTo(requestPost.Category));
        }

        [Test]
        public async Task PartialUpdateUpdatesOnlyNotNullFields_CategoryCase()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync($"/Posts/", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            var newRequestPost = new RequestPost { Title = null, Author = null, Content = null, Category = "NewCategory" };
            response = await _client.PatchAsync($"/Posts/{id}", GetStringContent(newRequestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync($"/Posts?{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));

            Assert.That(posts[0].Id, Is.EqualTo(id));
            Assert.That(posts[0].Title, Is.EqualTo(requestPost.Title));
            Assert.That(posts[0].Author, Is.EqualTo(requestPost.Author));
            Assert.That(posts[0].Content, Is.EqualTo(requestPost.Content));
            Assert.That(posts[0].Category, Is.EqualTo(newRequestPost.Category));
        }

        //DELETE Tests
        [Test]
        public async Task DeleteReturnsUnauthorizedIfAdminTokenIsNotInRequestHeader()
        {
            var response = await _client.DeleteAsync($"/Posts/999");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task DeleteReturnsNotFoundIfPostIsNotPresent()
        {
            _client.DefaultRequestHeaders.Add("AdminRoleToken", "ADMIN_TOKEN");
            var response = await _client.DeleteAsync($"/Posts/999");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task DeleteReturnsOKAfterDeletion()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync($"/Posts/", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            _client.DefaultRequestHeaders.Add("AdminRoleToken", "ADMIN_TOKEN");
            response = await _client.DeleteAsync($"/Posts/{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync("/Posts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteRemoveTagsInCascadeAndImage()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var byteArrayContent = new ByteArrayContent(File.ReadAllBytes("Image.png"));
            using var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(byteArrayContent, "imageFile", "Image.png");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");
            var postResponse = await _client.PostAsync($"/Posts/{id}/Image", multipartContent);

            var imagefileName = Path.Join(PathUtils.ImagesContentRootPath, $"{id}.png");
            Assert.True(File.Exists(imagefileName));

            _client.DefaultRequestHeaders.Add("AdminRoleToken", "ADMIN_TOKEN");
            response = await _client.DeleteAsync($"/Posts/{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync("/Posts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(0));

            Assert.False(File.Exists(imagefileName));
        }

        // POST Image
        [Test]
        public async Task PostImageReturnsNotFoundIfPostIsNotPresent()
        {
            var notPresentId = 99;
            using var byteArrayContent = new ByteArrayContent(File.ReadAllBytes("Image.png"));
            using var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(byteArrayContent, "imageFile", "Image.png");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");
            var postResponse = await _client.PostAsync($"/Posts/{notPresentId}/Image", multipartContent);

            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }


        [Test]
        public async Task PostImageReturnsBadRequestIfNotPng()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());


            using var byteArrayContent = new ByteArrayContent(File.ReadAllBytes("Image.jpg"));
            using var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(byteArrayContent, "imageFile", "Image.jpg");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");
            var postResponse = await _client.PostAsync($"/Posts/{id}/Image", multipartContent);

            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        // GET Image

        [Test]
        public async Task GetImageReturnsNotFoundIfPostIsNotPresent()
        {
            var response = await _client.GetAsync("/Posts/99/Image");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetImageReturnsNotFoundIfImageOfThePostIsNotPresent()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            response = await _client.GetAsync($"/Posts/{id}/Image");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task UpdateImageReturnsBadRequestIfImageIsNull()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
           
            var postResponse = await _client.PostAsync($"/Posts/{id}/Image", null);
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetImageReturnsOkIfImageIsRetrievedAndGetTheSamePostedByteArray()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            using var byteArrayContent = new ByteArrayContent(File.ReadAllBytes("Image.png"));
            using var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(byteArrayContent, "imageFile", "Image.png");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");
            var postResponse = await _client.PostAsync($"/Posts/{id}/Image", multipartContent);
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync($"/Posts/{id}/Image");
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Compare Posted and Get images bytearrays
            Assert.True(ByteArrayCompare(await byteArrayContent.ReadAsByteArrayAsync(), await response.Content.ReadAsByteArrayAsync()));
        }

        // DELETE Image

        [Test]
        public async Task DeleteImageReturnsNotFoundIfPostIsNotPresent()
        {
            var notPresentId = 99;
            var postResponse = await _client.DeleteAsync($"/Posts/{notPresentId}/Image");
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task DeleteImageReturnsNotFoundIfPImageIsNotPresent()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            var postResponse = await _client.DeleteAsync($"/Posts/{id}/Image");
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task DeleteImageReturnsOkAfterDeletion()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            using var byteArrayContent = new ByteArrayContent(File.ReadAllBytes("Image.png"));
            using var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(byteArrayContent, "imageFile", "Image.png");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");
            var postResponse = await _client.PostAsync($"/Posts/{id}/Image", multipartContent);
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var imagefileName = Path.Join(PathUtils.ImagesContentRootPath, $"{id}.png");
            Assert.True(File.Exists(imagefileName));

            postResponse = await _client.DeleteAsync($"/Posts/{id}/Image");
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.False(File.Exists(imagefileName));
        }

        //POST Tag

        [Test]
        public async Task PostTagOnANotPresentPostReturnNotFound()
        {
            var id = 99;
            var response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task PostEmptyTagOnPostReturnsBadRequest()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent(""));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task PostTagOnPostReturnsOk()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task PostTheSameTagOnPostDontIncreaseTheNumberOfTags()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            response = await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync("/Posts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));
            Assert.That(posts[0].Tags.Count, Is.EqualTo(1));
        }

        // DELETE TAG

        [Test]
        public async Task DeleteTagOnANotPresentPostReturnNotFound()
        {
            var id = 99;
            var response = await _client.DeleteAsync($"/Posts/{id}/Tags/Pippo");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task DeleteNotPresentTagOnAPostReturnNotFound()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());

            response = await _client.DeleteAsync($"/Posts/{id}/Tags/Pippo");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task DeletePresentTagReturnOk()
        {
            var requestPost = new RequestPost { Title = "Title1", Author = "Author1", Content = "Content1", Category = "Category1" };
            var response = await _client.PostAsync("/Posts", GetStringContent(requestPost));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Good"));
            await _client.PostAsync($"/Posts/{id}/Tags", GetStringContent("Bad"));

            response = await _client.DeleteAsync($"/Posts/{id}/Tags/Good");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            response = await _client.GetAsync($"/Posts?{id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var posts = JsonConvert.DeserializeObject<List<ResponsePost>>(await response.Content.ReadAsStringAsync());
            Assert.That(posts.Count, Is.EqualTo(1));

            Assert.That(posts[0].Tags.Count, Is.EqualTo(1));
        }

        private StringContent GetStringContent(object requestPost)
        {
            var jsonPost = JsonConvert.SerializeObject(requestPost);
            return new StringContent(jsonPost, UnicodeEncoding.UTF8, "application/json");
        }

        private static void DestroyDbAndImages()
        {
            if (File.Exists(PathUtils.DbFilePath))
                File.Delete(PathUtils.DbFilePath);

            if (Directory.Exists(PathUtils.ImagesDirName))
                Directory.Delete(PathUtils.ImagesDirName, true);
        }

        private bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }
    }
}

