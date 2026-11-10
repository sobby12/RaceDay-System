
using Xunit;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaceDay.API.Models;

namespace RaceDay.API.Tests
{
    public class UnitTest1
    {
        private bool IsCategoryValid(Category category)
        {
            var context = new ValidationContext(category);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(
                category,
                context,
                results,
                true
            );
        }

        [Fact]
        public void Category_ShouldBeValid_WhenDetailsAreCorrect()
        {
            var category = new Category
            {
                EventId = 1,
                CategoryName = "10km",
                CategoryType = "Distance"
            };

            Assert.True(IsCategoryValid(category));
        }

        [Fact]
        public void CategoryName_ShouldBeInvalid_WhenEmpty()
        {
            var category = new Category
            {
                EventId = 1,
                CategoryName = "",
                CategoryType = "Distance"
            };

            Assert.False(IsCategoryValid(category));
        }

        [Fact]
        public void CategoryName_ShouldBeInvalid_WhenExceeds80Characters()
        {
            var category = new Category
            {
                EventId = 1,
                CategoryName = new string('A', 81),
                CategoryType = "Distance"
            };

            Assert.False(IsCategoryValid(category));
        }

        [Fact]
        public void CategoryType_ShouldBeInvalid_WhenEmpty()
        {
            var category = new Category
            {
                EventId = 1,
                CategoryName = "10km",
                CategoryType = ""
            };

            Assert.False(IsCategoryValid(category));
        }

        [Fact]
        public void CategoryType_ShouldBeInvalid_WhenExceeds10Characters()
        {
            var category = new Category
            {
                EventId = 1,
                CategoryName = "10km",
                CategoryType = "VeryLongType"
            };

            Assert.False(IsCategoryValid(category));
        }
    }
}

