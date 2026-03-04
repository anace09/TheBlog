using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.PostsService.Controllers;
using WebApi.PostsService.Data;
using WebApi.PostsService.Models;
using Xunit;

namespace WebApi.PostsService.Tests
{
    public class CategoriesControllerTests : IDisposable
    {
        private readonly AppDbContext _db;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _controller = new CategoriesController(_db);
        }

        // --- GetAll ---

        [Fact]
        public async Task GetAll_EmptyDb_ReturnsEmptyList()
        {
            var result = await _controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result);
            var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(ok.Value);
            Assert.Empty(categories);
        }

        [Fact]
        public async Task GetAll_WithCategories_ReturnsAll()
        {
            _db.Categories.AddRange(
                new Category { Name = "Tech" },
                new Category { Name = "Science" }
            );
            await _db.SaveChangesAsync();

            var result = await _controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result);
            var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(ok.Value);
            Assert.Equal(2, categories.Count());
        }

        // --- GetById ---

        [Fact]
        public async Task GetById_ExistingCategory_ReturnsCategory()
        {
            var category = new Category { Name = "Tech" };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var result = await _controller.GetById(category.Id);
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Category>(ok.Value);
            Assert.Equal(category.Id, returned.Id);
        }

        [Fact]
        public async Task GetById_NonExistingCategory_ReturnsNotFound()
        {
            var result = await _controller.GetById(999);
            Assert.IsType<NotFoundResult>(result);
        }

        // --- Create ---

        [Fact]
        public async Task Create_ValidName_ReturnsOk()
        {
            var result = await _controller.Create("Tech");
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Create_AddsToDb()
        {
            await _controller.Create("Tech");
            Assert.Equal(1, await _db.Categories.CountAsync());
        }

        [Fact]
        public async Task Create_ReturnsCreatedCategory()
        {
            var result = await _controller.Create("Tech");
            var ok = Assert.IsType<OkObjectResult>(result);
            var category = Assert.IsType<Category>(ok.Value);
            Assert.Equal("Tech", category.Name);
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ExistingCategory_ReturnsNoContent()
        {
            var category = new Category { Name = "Tech" };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var result = await _controller.Delete(category.Id);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistingCategory_ReturnsNotFound()
        {
            var result = await _controller.Delete(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_RemovesFromDb()
        {
            var category = new Category { Name = "Tech" };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            await _controller.Delete(category.Id);
            Assert.Equal(0, await _db.Categories.CountAsync());
        }

        public void Dispose() => _db.Dispose();
    }
}