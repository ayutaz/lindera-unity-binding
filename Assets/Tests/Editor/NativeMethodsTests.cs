using System;
using System.Text;
using NUnit.Framework;

namespace Lindera.Tests
{
    /// <summary>
    /// NativeMethodsユーティリティのユニットテスト
    /// </summary>
    public class NativeMethodsTests
    {
        [Test]
        public unsafe void PtrToStringUTF8_ReturnsNull_WhenPointerIsNull()
        {
            // Act
            var result = NativeMethods.PtrToStringUTF8(null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public unsafe void PtrToStringUTF8_ReturnsEmptyString_WhenFirstByteIsNull()
        {
            // Arrange
            var bytes = new byte[] { 0 };

            fixed (byte* ptr = bytes)
            {
                // Act
                var result = NativeMethods.PtrToStringUTF8(ptr);

                // Assert
                Assert.AreEqual(string.Empty, result);
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_ReturnsAsciiString()
        {
            // Arrange
            var expected = "Hello";
            var bytes = Encoding.UTF8.GetBytes(expected + "\0");

            fixed (byte* ptr = bytes)
            {
                // Act
                var result = NativeMethods.PtrToStringUTF8(ptr);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_ReturnsJapaneseString()
        {
            // Arrange
            var expected = "こんにちは";
            var bytes = Encoding.UTF8.GetBytes(expected + "\0");

            fixed (byte* ptr = bytes)
            {
                // Act
                var result = NativeMethods.PtrToStringUTF8(ptr);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_ReturnsKanjiString()
        {
            // Arrange
            var expected = "東京都渋谷区";
            var bytes = Encoding.UTF8.GetBytes(expected + "\0");

            fixed (byte* ptr = bytes)
            {
                // Act
                var result = NativeMethods.PtrToStringUTF8(ptr);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_ReturnsMixedString()
        {
            // Arrange
            var expected = "Hello世界123";
            var bytes = Encoding.UTF8.GetBytes(expected + "\0");

            fixed (byte* ptr = bytes)
            {
                // Act
                var result = NativeMethods.PtrToStringUTF8(ptr);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_HandlesEmoji()
        {
            // Arrange
            var expected = "テスト🎉";
            var bytes = Encoding.UTF8.GetBytes(expected + "\0");

            fixed (byte* ptr = bytes)
            {
                // Act
                var result = NativeMethods.PtrToStringUTF8(ptr);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_WithMaxLength_ReturnsString_WhenWithinLimit()
        {
            // Arrange
            var expected = "Hello";
            var bytes = Encoding.UTF8.GetBytes(expected + "\0");

            fixed (byte* ptr = bytes)
            {
                // Act
                var result = NativeMethods.PtrToStringUTF8(ptr, 100);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_WithMaxLength_ThrowsException_WhenExceedsLimit()
        {
            // Arrange
            var text = "Hello World";
            var bytes = Encoding.UTF8.GetBytes(text + "\0");

            fixed (byte* ptr = bytes)
            {
                // Act & Assert
                InvalidOperationException exception = null;
                try
                {
                    NativeMethods.PtrToStringUTF8(ptr, 5); // Limit smaller than string
                }
                catch (InvalidOperationException ex)
                {
                    exception = ex;
                }
                Assert.IsNotNull(exception, "Expected InvalidOperationException was not thrown");
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_WithMaxLength_ReturnsEmpty_WhenMaxLengthIsZero()
        {
            // Arrange
            var bytes = Encoding.UTF8.GetBytes("Hello\0");

            fixed (byte* ptr = bytes)
            {
                // Act
                var result = NativeMethods.PtrToStringUTF8(ptr, 0);

                // Assert
                Assert.AreEqual(string.Empty, result);
            }
        }

        [Test]
        public unsafe void PtrToStringUTF8_WithMaxLength_ReturnsNull_WhenPointerIsNull()
        {
            // Act
            var result = NativeMethods.PtrToStringUTF8(null, 100);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public unsafe void PtrToStringUTF8_WithMaxLength_HandlesExactLength()
        {
            // Arrange
            var expected = "12345";
            var bytes = Encoding.UTF8.GetBytes(expected + "\0");

            fixed (byte* ptr = bytes)
            {
                // Act - maxLength is exactly string length + 1 for null terminator
                var result = NativeMethods.PtrToStringUTF8(ptr, 6);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }
    }
}
