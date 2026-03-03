
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using WebApi.PostsService.Controllers;
using WebApi.PostsService.Data;
using WebApi.PostsService.DTOs;
using WebApi.PostsService.Models;
using WebApi.PostsService.Services;

namespace WebApi.PostsService.Tests
{

    public class PostsControllerTests : IDisposable
    {

        private readonly AppDbContext _db;
        private readonly PostsController _controller;
        private const string TestUserId = "user-123";

        public PostsControllerTests()
        {
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _controller = new PostsController(_db);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                                new Claim(ClaimTypes.NameIdentifier, TestUserId)
                            ], "test"))

                }
            };
        }

        // --- GetAll ---

        [Fact]
        public async Task GetAll_EmptyDb_ReturnsEmptyList() 
        {

            var result = await _controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result);
            var posts = Assert.IsAssignableFrom<IEnumerable<Post>>(ok.Value);

            Assert.Empty(posts);

        }

        [Fact]
        public async Task GetAll_WithPosts_ReturnsAllPosts() 
        {

            _db.Posts.AddRange(
                    new Post { Title = "Post 1", Content = "Content 1", AuthorId = TestUserId, },
                    new Post { Title = "Post 2", Content = "Content 2", AuthorId = TestUserId, }
                );

            await _db.SaveChangesAsync();

            var result = await _controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result);
            var posts = Assert.IsAssignableFrom<IEnumerable<Post>>(ok.Value);

            Assert.Equal(2, posts.Count());

        }

        // -- GetById ---

        [Fact]
        public async Task GetById_ExistingPost_ReturnsPost() 
        {

            var post = new Post { Title = "Post", Content = "Content", AuthorId = TestUserId, };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var result = await _controller.GetById(post.Id);
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Post>(ok.Value);

            Assert.Equal(post.Id, returned.Id);

        }

        [Fact]
        public async Task GetById_NonExistingPost_ReturnsNotFound() 
        {

            var result = await _controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);

        }

        // --- Create ---

        [Fact]
        public async Task Create_ValidDto_ReturnsCreated() 
        {

            var dto = new CreatePostDto("Title", "Content", null, [], [], []);
            var result = await _controller.Create(dto);

            Assert.IsType<CreatedAtActionResult>(result);
        
        }

        [Fact]
        public async Task Create_SetsAuthorIdFromToken() 
        {

            var dto = new CreatePostDto("Title", "Content", null, [], [], []);
            await _controller.Create(dto);

            var post = await _db.Posts.FirstOrDefaultAsync();
            Assert.NotNull(post);

            Assert.Equal(TestUserId, post.AuthorId);

        }

        // --- Update ---

        [Fact]
        public async Task Update_OwnPost_ReturnsOk() 
        {

            var post = new Post { Title = "Old", Content = "Old content", AuthorId = TestUserId };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var result = await _controller.Update(post.Id, new UpdatePostDto("New Title", "New Content"));

            Assert.IsType<OkObjectResult>(result);

        }

        [Fact]
        public async Task Update_NonExistingPost_ReturnsNotFound()
        {
            var result = await _controller.Update(999, new UpdatePostDto("Title", "Content"));
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OtherUserPost_ReturnsForbid()
        {
            var post = new Post { Title = "Title", Content = "Content", AuthorId = "other-user" };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var result = await _controller.Update(post.Id, new UpdatePostDto("New", "New"));
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Update_UpdatesFieldsCorrectly()
        {
            var post = new Post { Title = "Old", Content = "Old", AuthorId = TestUserId };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            await _controller.Update(post.Id, new UpdatePostDto("New Title", "New Content"));

            var updated = await _db.Posts.FindAsync(post.Id);
            Assert.Equal("New Title", updated!.Title);
            Assert.Equal("New Content", updated.Content);
            Assert.NotNull(updated.UpdatedAt);
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_OwnPost_ReturnsNoContent()
        {
            var post = new Post { Title = "Title", Content = "Content", AuthorId = TestUserId };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(post.Id);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistingPost_ReturnsNotFound()
        {
            var result = await _controller.Delete(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_OtherUserPost_ReturnsForbid()
        {
            var post = new Post { Title = "Title", Content = "Content", AuthorId = "other-user" };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(post.Id);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_OwnPost_RemovesFromDb()
        {
            var post = new Post { Title = "Title", Content = "Content", AuthorId = TestUserId };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            await _controller.Delete(post.Id);

            var deleted = await _db.Posts.FindAsync(post.Id);
            Assert.Null(deleted);
        }

        // --- UploadImage ---

        [Fact]
        public async Task UploadImage_NullFile_ReturnsBadRequest()
        {

            var blobMock = new Mock<IBlobStorageService>();

            var result = await _controller.UploadImage(null!, blobMock.Object);
            Assert.IsType<BadRequestObjectResult>(result);

        }

        [Fact]
        public async Task UploadImage_InvalidExtension_ReturnsBadRequest()
        {

            var blobMock = new Mock<IBlobStorageService>();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file.exe");
            fileMock.Setup(f => f.Length).Returns(1024);

            var result = await _controller.UploadImage(fileMock.Object, blobMock.Object);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadImage_FileTooLarge_ReturnsBadRequest()
        {

            var blobMock = new Mock<IBlobStorageService>();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("image.jpg");
            fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024);

            var result = await _controller.UploadImage(fileMock.Object, blobMock.Object);
            Assert.IsType<BadRequestObjectResult>(result);

        }

        public void Dispose()
        {
            _db.Dispose();
        }

    }

}

