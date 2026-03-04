using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.PostsService.Controllers;
using WebApi.PostsService.Data;
using WebApi.PostsService.DTOs;
using WebApi.PostsService.Models;

namespace WebApi.PostsService.Tests
{
    public class CommentsControllerTests : IDisposable
    {

        private readonly AppDbContext _db;
        private readonly CommentsController _controller;
        private const string TestUserId = "user-123";
        private const int TestPostId = 1;

        public CommentsControllerTests()
        {

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _controller = new CommentsController(_db);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [ 
                        new Claim(ClaimTypes.NameIdentifier, TestUserId)
                        ], "test"))
                }
            };

            _db.Posts.Add(new Post { Id = TestPostId, Title = "Test", Content = "Test", AuthorId = TestUserId });
            _db.SaveChanges();

        }

        // --- GetAll ---

        [Fact]
        public async Task GetAll_NoComments_ReturnsEmptyList() 
        {

            var result = await _controller.GetAll(TestPostId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var comments = Assert.IsAssignableFrom<IEnumerable<Comment>>(ok.Value);

            Assert.Empty(comments);

        }

        [Fact]
        public async Task GetAll_WithComments_ReturnsAll() 
        {

            _db.Comments.AddRange(
                new Comment { Content = "Comment 1", AuthorId = TestUserId, PostId = TestPostId },
                new Comment { Content = "Comment 2", AuthorId = TestUserId, PostId = TestPostId }
            );
            await _db.SaveChangesAsync();

            var result = await _controller.GetAll(TestPostId);
            var ok = Assert.IsType<OkObjectResult>(result);
            var comments = Assert.IsAssignableFrom<IEnumerable<Comment>>(ok.Value);
            Assert.Equal(2, comments.Count());
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyCommentsForPost()
        {
            // Другой пост
            _db.Posts.Add(new Post { Id = 2, Title = "Other", Content = "Other", AuthorId = TestUserId });
            _db.Comments.AddRange(
                new Comment { Content = "Comment 1", AuthorId = TestUserId, PostId = TestPostId },
                new Comment { Content = "Comment 2", AuthorId = TestUserId, PostId = 2 }
            );
            await _db.SaveChangesAsync();

            var result = await _controller.GetAll(TestPostId);
            var ok = Assert.IsType<OkObjectResult>(result);
            var comments = Assert.IsAssignableFrom<IEnumerable<Comment>>(ok.Value);
            Assert.Single(comments);
        }

        // --- Create ---

        [Fact]
        public async Task Create_ValidRequest_ReturnsOk()
        {
            var result = await _controller.Create(TestPostId, new CreateCommentDto("Hello!"));
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Create_NonExistingPost_ReturnsNotFound()
        {
            var result = await _controller.Create(999, new CreateCommentDto("Hello!"));
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_SetsAuthorIdFromToken()
        {
            await _controller.Create(TestPostId, new CreateCommentDto("Hello!"));
            var comment = await _db.Comments.FirstOrDefaultAsync();
            Assert.Equal(TestUserId, comment!.AuthorId);
        }

        [Fact]
        public async Task Create_AddsToDb()
        {
            await _controller.Create(TestPostId, new CreateCommentDto("Hello!"));
            Assert.Equal(1, await _db.Comments.CountAsync());
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_OwnComment_ReturnsOk()
        {
            var comment = new Comment { Content = "Hello", AuthorId = TestUserId, PostId = TestPostId };
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(TestPostId, comment.Id);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistingComment_ReturnsNotFound()
        {
            var result = await _controller.Delete(TestPostId, 999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_OtherUserComment_ReturnsForbid()
        {
            var comment = new Comment { Content = "Hello", AuthorId = "other-user", PostId = TestPostId };
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(TestPostId, comment.Id);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_RemovesFromDb()
        {
            var comment = new Comment { Content = "Hello", AuthorId = TestUserId, PostId = TestPostId };
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            await _controller.Delete(TestPostId, comment.Id);
            Assert.Equal(0, await _db.Comments.CountAsync());
        }

        public void Dispose() => _db.Dispose();
    }
}
