using Xunit;

namespace AccountService.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void SimpleTest_ShouldPass()
        {
            // Arrange
            var expected = 2;
            var actual = 1 + 1;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AccountService_ShouldBeTestable()
        {
            // This test verifies that we can create basic tests
            // and that the testing infrastructure is working
            Assert.True(true);
        }
    }
} 