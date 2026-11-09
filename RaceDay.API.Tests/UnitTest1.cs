
using Xunit;

namespace RaceDay.API.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void CategoryName_ShouldNotBeEmpty()
        {
            // Arrange
            string categoryName = "10km";

            // Act
            bool isValid = !string.IsNullOrWhiteSpace(categoryName);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void CategoryName_ShouldBeInvalid_WhenEmpty()
        {
            // Arrange
            string categoryName = "";

            // Act
            bool isValid = !string.IsNullOrWhiteSpace(categoryName);

            // Assert
            Assert.False(isValid);
        }
    }
}

