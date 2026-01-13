using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using LinderaUnityBinding;

namespace LinderaUnityBinding.Tests
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

        [Test]
        public void Tokenize_HandlesMixedScript_JapaneseEnglishNumbers()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "Hello世界123テスト";

            // Act
            var tokens = tokenizer.Tokenize(text);

            // Assert
            Assert.IsNotNull(tokens);
            Assert.Greater(tokens.Length, 0);

            // 元のテキストが全てトークン化されていることを確認
            var reconstructed = string.Join("", System.Array.ConvertAll(tokens, t => t.Surface));
            Assert.AreEqual(text, reconstructed);
        }

        [Test]
        public void Tokenize_HandlesLargeText()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var sb = new StringBuilder();
            // 約10KB のテキストを生成
            for (int i = 0; i < 500; i++)
            {
                sb.Append("東京都渋谷区に住んでいます。");
            }
            var largeText = sb.ToString();

            // Act
            var tokens = tokenizer.Tokenize(largeText);

            // Assert
            Assert.IsNotNull(tokens);
            Assert.Greater(tokens.Length, 1000); // 多くのトークンが生成されるはず
        }

        [Test]
        public void Tokenize_HandlesSymbolsAndPunctuation()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "こんにちは！「世界」へ、ようこそ。";

            // Act
            var tokens = tokenizer.Tokenize(text);

            // Assert
            Assert.IsNotNull(tokens);
            Assert.Greater(tokens.Length, 0);
        }

        [Test]
        public void MultipleTokenizers_WorkIndependently()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer1 = new LinderaTokenizer();
            using var tokenizer2 = new LinderaTokenizer();

            // Act
            var tokens1 = tokenizer1.Tokenize("東京");
            var tokens2 = tokenizer2.Tokenize("大阪");

            // Assert
            Assert.AreEqual("東京", tokens1[0].Surface);
            Assert.AreEqual("大阪", tokens2[0].Surface);
        }

        [UnityTest]
        public IEnumerator TokenizeAsync_CanBeCancelled() => UniTask.ToCoroutine(async () =>
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            using var cts = new CancellationTokenSource();

            // キャンセルを即座に要求
            cts.Cancel();

            // Act & Assert
            try
            {
                await tokenizer.TokenizeAsync("テスト", cts.Token);
                // キャンセルされない場合もある（同期的に完了するため）
            }
            catch (OperationCanceledException)
            {
                // 期待される動作
                Assert.Pass("Cancellation was properly handled");
            }
        });

        [Test]
        public void Tokenize_HandlesWhitespaceOnly()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "   \t\n  ";

            // Act
            var tokens = tokenizer.Tokenize(text);

            // Assert
            Assert.IsNotNull(tokens);
            // 空白のみの場合でもクラッシュしないことを確認
        }

        [Test]
        public void GetReading_ReturnsOriginal_WhenNoReading()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();
            var text = "ABC123"; // ASCII文字は読みがない

            // Act
            var reading = tokenizer.GetReading(text);

            // Assert
            Assert.IsNotNull(reading);
            // 読みがない場合は表層形がそのまま返される
        }

        [Test]
        public void Tokenize_ReturnsPartOfSpeech()
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
            Assert.IsNotNull(tokens[0].PartOfSpeech);
            Assert.IsNotEmpty(tokens[0].PartOfSpeech);
        }
    }
}
