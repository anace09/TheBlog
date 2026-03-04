using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.PostsService.Controllers;
using WebApi.PostsService.Data;
using WebApi.PostsService.Models;

namespace WebApi.PostsService.Tests
{
    public class TagsControllerTests : IDisposable
    {

        private readonly AppDbContext _db;
        private readonly TagsController _controller;

        public TagsControllerTests()
        {
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _controller = new TagsController(_db);

        }

        // --- GetAll ---

        [Fact]
        public async Task GetAll_EmptyDb_ReturnsEmptyList() 
        {

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var tags = Assert.IsAssignableFrom<IEnumerable<Tag>>(ok.Value);

            Assert.Empty(tags);
        
        }

        [Fact]
        public async Task GetAll_WithTags_ReturnsAll() 
        {

            _db.Tags.AddRange(
                new Tag { Name = "C#" },
                new Tag { Name = ".NET" }
            );

            await _db.SaveChangesAsync();

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var tags = Assert.IsAssignableFrom<IEnumerable<Tag>>(ok.Value);

            Assert.Equal(2, tags.Count());
        
        }

        // --- GetById ---

        [Fact]
        public async Task GetById_ExistingTag_ReturnsTag()
        {

            var tag = new Tag { Name = "C#" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();

            var result = await _controller.GetById(tag.Id);
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Tag>(ok.Value);

            Assert.Equal(tag.Id, returned.Id);

        }

        [Fact]
        public async Task GetById_NonExistingTag_ReturnsNotFound()
        {
            var result = await _controller.GetById(999);
            Assert.IsType<NotFoundResult>(result);
        }

        // --- Create ---

        [Fact]
        public async Task Create_ValidName_ReturnsOk() 
        {

            var result = await _controller.Create("Docker");

            Assert.IsType<OkObjectResult>(result);

        }

        [Fact]
        public async Task Create_AddsToDb() 
        {

            await _controller.Create("Docker");

            Assert.Equal(1, await _db.Tags.CountAsync());

        }

        [Fact]
        public async Task Create_ReturnsCreatedTag() 
        {

            var result = await _controller.Create("Docker");
            var ok = Assert.IsType<OkObjectResult>(result);
            var tag = Assert.IsType<Tag>(ok.Value);

            Assert.Equal("Docker", tag.Name);

        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ExistingTag_ReturnsOk() 
        {

            var tag = new Tag { Name = "C#" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(tag.Id);

            Assert.IsType<OkObjectResult>(result);

        }

        [Fact]
        public async Task Delete_NonExistingTag_ReturnsNotFound()
        {

            var result = await _controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);

        }

        [Fact]
        public async Task Delete_RemovesFromDb() {

            var tag = new Tag { Name = "C#" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(tag.Id);

            Assert.Equal(0, await _db.Tags.CountAsync());
        
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
