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
    public class ReactionsControllerTests : IDisposable
    {

        private readonly AppDbContext _db;
        private readonly ReactionsController _controller;
        private const string TestUserId = "user-123";
        private const int TestPostId = 1;

        public ReactionsControllerTests()
        {

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new AppDbContext(options);
            _controller = new ReactionsController(_db);

            _controller.ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext 
                {
                    User = new ClaimsPrincipal( new ClaimsIdentity(
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
        public async Task GetAll_NoReactions_ReturnsEmptyList() 
        {

            var result = await _controller.GetAll(TestPostId);
            var ok = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(ok.Value);
        
        }

        [Fact]
        public async Task GetAll_WithReactions_ReturnsGrouped() 
        {

            _db.Reactions.AddRange(
                new Reaction { PostId = TestPostId, UserId = "user-1", Type = ReactionType.Like },
                new Reaction { PostId = TestPostId, UserId = "user-2", Type = ReactionType.Like },
                new Reaction { PostId = TestPostId, UserId = "user-3", Type = ReactionType.Like }
            );

            await _db.SaveChangesAsync();

            var result = await _controller.GetAll(TestPostId);
            var ok = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(ok.Value);

        }

        // --- Upsert ---

        [Fact]
        public async Task Upsert_NewReaction_AddToDb() 
        {

            var result = await _controller.Upsert(TestPostId, new UpsertReactionDto(ReactionType.Like));
            Assert.IsType<OkResult>(result);

            Assert.Equal(1, _db.Reactions.Count());
        
        }

        [Fact]
        public async Task Upsert_ExistingReaction_UpdatesType() 
        {

            _db.Reactions.Add(new Reaction
            {
                PostId = TestPostId,
                UserId = TestUserId,
                Type = ReactionType.Like
            });
            await _db.SaveChangesAsync();

            await _controller.Upsert(TestPostId, new UpsertReactionDto(ReactionType.Love));

            var reaction = await _db.Reactions.FirstOrDefaultAsync();

            Assert.Equal(ReactionType.Love, reaction!.Type);
        
        }

        [Fact]
        public async Task Upsert_ExistingReaction_DoesNotDuplicate() 
        {

            _db.Reactions.Add(new Reaction
            {
                PostId = TestPostId,
                UserId = TestUserId,
                Type = ReactionType.Like
            });
            await _db.SaveChangesAsync();

            await _controller.Upsert(TestPostId, new UpsertReactionDto(ReactionType.Haha));

            Assert.Equal(1, await _db.Reactions.CountAsync());
        
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ExistingReaction_ReturnsNoContent()
        {

            _db.Reactions.Add(new Reaction
            {
                PostId = TestPostId,
                UserId = TestUserId,
                Type = ReactionType.Like
            });
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(TestPostId);

            Assert.IsType<NoContentResult>(result);
        
        }

        [Fact]
        public async Task Delete_NonExistingReaction_ReturnsNotFound() 
        {

            var result = await _controller.Delete(TestPostId);

            Assert.IsType<NotFoundResult>(result);
        
        }

        [Fact]
        public async Task Delete_RemovesFromDb() 
        {

            _db.Reactions.Add(new Reaction
            {
                PostId=TestPostId,
                UserId = TestUserId,
                Type = ReactionType.Like
            });
            await _db.SaveChangesAsync();

            await _controller.Delete(TestPostId);

            Assert.Equal(0, await _db.Reactions.CountAsync());

        }

        [Fact]
        public async Task Delete_OtherUserReaction_ReturnsNotFound()
        {

            _db.Reactions.Add(new Reaction
            {
                PostId = TestPostId,
                UserId = "other-user",
                Type = ReactionType.Like
            });
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(TestPostId);

            Assert.IsType<NotFoundResult>(result);

        }

        public void Dispose()
        {

            _db.Dispose();

        }
    }
}
