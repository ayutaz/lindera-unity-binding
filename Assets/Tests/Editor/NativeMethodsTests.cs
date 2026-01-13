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
    }
}
