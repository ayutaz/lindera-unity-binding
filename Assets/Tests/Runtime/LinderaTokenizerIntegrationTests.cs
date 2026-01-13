using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lindera.Tests
{
    /// <summary>
    /// LinderaTokenizerの統合テスト（ネイティブライブラリが必要）
    /// </summary>
    /// <remarks>
    /// これらのテストはネイティブライブラリ(lindera_ffi)がビルドされて
    /// Assets/Plugins/に配置されている場合のみ動作します。
    /// </remarks>
    public class LinderaTokenizerIntegrationTests
    {
        private bool IsNativeLibraryAvailable()
        {
            try
            {
                // ネイティブライブラリが利用可能かチェック
                var handle = NativeMethods.lindera_tokenizer_create();
                if (handle != IntPtr.Zero)
                {
                    NativeMethods.lindera_tokenizer_destroy(handle);
                    return true;
                }
                return false;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            catch (EntryPointNotFoundException)
            {
                return false;
            }
        }

        [Test]
        public void Tokenizer_Create_Succeeds_WhenLibraryAvailable()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange & Act
            using var tokenizer = new LinderaTokenizer();

            // Assert
            Assert.IsTrue(tokenizer.IsValid);
        }

        [Test]
        public void Tokenize_ReturnsTokens_ForJapaneseText()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "東京都に住んでいます";

            // Act
            var tokens = tokenizer.Tokenize(text);

            // Assert
            Assert.IsNotNull(tokens);
            Assert.Greater(tokens.Length, 0);
        }

        [Test]
        public void Tokenize_ReturnsCorrectSurface()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "東京";

            // Act
            var tokens = tokenizer.Tokenize(text);

            // Assert
            Assert.AreEqual(1, tokens.Length);
            Assert.AreEqual("東京", tokens[0].Surface);
        }

        [Test]
        public void Tokenize_ReturnsReading_ForIPADIC()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "東京";

            // Act
            var tokens = tokenizer.Tokenize(text);

            // Assert
            Assert.AreEqual("トウキョウ", tokens[0].Reading);
        }

        [Test]
        public void Tokenize_ReturnsEmptyArray_ForEmptyString()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var tokens = tokenizer.Tokenize("");

            // Assert
            Assert.IsNotNull(tokens);
            Assert.AreEqual(0, tokens.Length);
        }

        [Test]
        public void Tokenize_ReturnsEmptyArray_ForNullString()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var tokens = tokenizer.Tokenize(null);

            // Assert
            Assert.IsNotNull(tokens);
            Assert.AreEqual(0, tokens.Length);
        }

        [Test]
        public void GetReading_ReturnsKatakana()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "東京";

            // Act
            var reading = tokenizer.GetReading(text);

            // Assert
            Assert.AreEqual("トウキョウ", reading);
        }

        [UnityTest]
        public IEnumerator TokenizeAsync_ReturnsTokens() => UniTask.ToCoroutine(async () =>
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "非同期テスト";

            // Act
            var tokens = await tokenizer.TokenizeAsync(text);

            // Assert
            Assert.IsNotNull(tokens);
            Assert.Greater(tokens.Length, 0);
        });

        [UnityTest]
        public IEnumerator GetReadingAsync_ReturnsKatakana() => UniTask.ToCoroutine(async () =>
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "日本語";

            // Act
            var reading = await tokenizer.GetReadingAsync(text);

            // Assert
            Assert.AreEqual("ニホンゴ", reading);
        });

        [Test]
        public void Tokenizer_ThrowsObjectDisposedException_AfterDispose()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            var tokenizer = new LinderaTokenizer();
            tokenizer.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => tokenizer.Tokenize("test"));
        }

        [Test]
        public void Tokenizer_IsValid_ReturnsFalse_AfterDispose()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            var tokenizer = new LinderaTokenizer();

            // Act
            tokenizer.Dispose();

            // Assert
            Assert.IsFalse(tokenizer.IsValid);
        }
    }
}
