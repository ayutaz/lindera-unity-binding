using System;
using NUnit.Framework;

using LinderaUnityBinding;

namespace LinderaUnityBinding.Tests
{
    /// <summary>
    /// LinderaExceptionのユニットテスト
    /// </summary>
    public class LinderaExceptionTests
    {
        [Test]
        public void Constructor_WithMessage_SetsMessage()
        {
            // Arrange
            var message = "Test error message";

            // Act
            var exception = new LinderaException(message);

            // Assert
            Assert.AreEqual(message, exception.Message);
        }

        [Test]
        public void Constructor_WithMessageAndInnerException_SetsBoth()
        {
            // Arrange
            var message = "Outer error";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new LinderaException(message, innerException);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }

        [Test]
        public void Exception_IsOfCorrectType()
        {
            // Arrange & Act
            var exception = new LinderaException("Test");

            // Assert
            Assert.IsInstanceOf<Exception>(exception);
            Assert.IsInstanceOf<LinderaException>(exception);
        }
    }
}
