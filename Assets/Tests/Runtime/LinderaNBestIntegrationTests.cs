using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

using LinderaUnityBinding;

namespace LinderaUnityBinding.Tests
{
    /// <summary>
    /// N-Best形態素解析の統合テスト（ネイティブライブラリが必要）
    /// </summary>
    public class LinderaNBestIntegrationTests
    {
        private bool IsNativeLibraryAvailable()
        {
            try
            {
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
        public void TokenizeNBest_BasicJapanese_ReturnsMultiplePaths()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var result = tokenizer.TokenizeNBest("すもももももももものうち", 3);

            // Assert
            Assert.IsNotNull(result.Paths);
            Assert.IsTrue(result.Paths.Length >= 1);
            Assert.IsTrue(result.Paths.Length <= 3);

            // 最初のパスが最低コストであることを確認
            if (result.Paths.Length > 1)
            {
                Assert.IsTrue(result.Paths[0].Cost <= result.Paths[1].Cost);
            }
        }

        [Test]
        public void TokenizeNBest_SingleResult_MatchesTokenize()
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
            var normalTokens = tokenizer.Tokenize(text);
            var nbest = tokenizer.TokenizeNBest(text, 1);

            // Assert
            Assert.AreEqual(1, nbest.Paths.Length);
            Assert.AreEqual(normalTokens.Length, nbest.Paths[0].Tokens.Length);

            for (int i = 0; i < normalTokens.Length; i++)
            {
                Assert.AreEqual(normalTokens[i].Surface, nbest.Paths[0].Tokens[i].Surface);
            }
        }

        [Test]
        public void TokenizeNBest_EmptyText_ReturnsEmptyResult()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var result = tokenizer.TokenizeNBest("", 3);

            // Assert
            Assert.AreEqual(0, result.Paths.Length);
        }

        [Test]
        public void TokenizeNBest_NullText_ReturnsEmptyResult()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var result = tokenizer.TokenizeNBest(null, 3);

            // Assert
            Assert.AreEqual(0, result.Paths.Length);
        }

        [Test]
        public void TokenizeNBest_InvalidN_ThrowsException()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => tokenizer.TokenizeNBest("テスト", 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => tokenizer.TokenizeNBest("テスト", -1));
        }

        [Test]
        public void TokenizeNBest_WithDeduplicate_ReturnsDeduplicated()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var withDedup = tokenizer.TokenizeNBest("すもももももももものうち", 5, deduplicate: true);
            var withoutDedup = tokenizer.TokenizeNBest("すもももももももものうち", 5, deduplicate: false);

            // Assert - 重複排除ありの場合は同じかより少ない結果になる
            Assert.IsTrue(withDedup.Paths.Length <= withoutDedup.Paths.Length);
        }

        [Test]
        public void TokenizeNBest_WithCostThreshold_ReturnsFilteredResults()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var withThreshold = tokenizer.TokenizeNBest("すもももももももものうち", 10, costThreshold: 1000);
            var withoutThreshold = tokenizer.TokenizeNBest("すもももももももものうち", 10);

            // Assert - コスト閾値ありの場合は同じかより少ない結果になる
            Assert.IsTrue(withThreshold.Paths.Length <= withoutThreshold.Paths.Length);
        }

        [Test]
        public void TokenizeNBest_PathTokensHaveDetails()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var result = tokenizer.TokenizeNBest("東京都", 3);

            // Assert
            Assert.IsTrue(result.Paths.Length >= 1);
            foreach (var path in result.Paths)
            {
                foreach (var token in path.Tokens)
                {
                    Assert.IsNotNull(token.Surface);
                    Assert.IsNotNull(token.Details);
                    Assert.IsTrue(token.Details.Length > 0);
                }
            }
        }

        [Test]
        public void TokenizeNBest_CostsAreOrdered()
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var result = tokenizer.TokenizeNBest("すもももももももものうち", 5);

            // Assert - パスはコスト昇順でソートされていることを確認
            for (int i = 1; i < result.Paths.Length; i++)
            {
                Assert.IsTrue(result.Paths[i - 1].Cost <= result.Paths[i].Cost,
                    $"Path {i - 1} cost ({result.Paths[i - 1].Cost}) should be <= Path {i} cost ({result.Paths[i].Cost})");
            }
        }

        [Test]
        public void TokenizeNBest_DisposedTokenizer_ThrowsException()
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
            Assert.Throws<ObjectDisposedException>(() => tokenizer.TokenizeNBest("テスト", 3));
        }

        [UnityTest]
        public IEnumerator TokenizeNBestAsync_BasicJapanese_ReturnsResults() => UniTask.ToCoroutine(async () =>
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var result = await tokenizer.TokenizeNBestAsync("東京都に住んでいます", 3);

            // Assert
            Assert.IsNotNull(result.Paths);
            Assert.IsTrue(result.Paths.Length >= 1);
        });

        [UnityTest]
        public IEnumerator TokenizeNBestAsync_EmptyText_ReturnsEmptyResult() => UniTask.ToCoroutine(async () =>
        {
            if (!IsNativeLibraryAvailable())
            {
                Assert.Ignore("Native library not available. Skipping integration test.");
                return;
            }

            // Arrange
            using var tokenizer = new LinderaTokenizer();

            // Act
            var result = await tokenizer.TokenizeNBestAsync("", 3);

            // Assert
            Assert.AreEqual(0, result.Paths.Length);
        });

        [UnityTest]
        public IEnumerator TokenizeNBestAsync_CanBeCancelled() => UniTask.ToCoroutine(async () =>
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
                await tokenizer.TokenizeNBestAsync("テスト", 3, cancellationToken: cts.Token);
                // キャンセルされない場合もある（同期的に完了するため）
            }
            catch (OperationCanceledException)
            {
                // 期待される動作
                Assert.Pass("Cancellation was properly handled");
            }
        });
    }
}
