using System;
using NUnit.Framework;
using Cysharp.Threading.Tasks;

using LinderaUnityBinding;

namespace LinderaUnityBinding.Tests
{
    /// <summary>
    /// LinderaTokenizerFactoryのユニットテスト
    /// </summary>
    public class LinderaTokenizerFactoryTests
    {
        [Test]
        public void IsWebGL_ReturnsFalse_InEditor()
        {
            // エディタ上では常にfalseを返す
            Assert.IsFalse(LinderaTokenizerFactory.IsWebGL);
        }

        [Test]
        public void IsWebGLInitialized_ReturnsFalse_InEditor()
        {
            // エディタ上では常にfalseを返す（WebGL初期化は不要）
            Assert.IsFalse(LinderaTokenizerFactory.IsWebGLInitialized);
        }

        [Test]
        public void Create_ReturnsLinderaTokenizer_InEditor()
        {
            // Act
            using var tokenizer = LinderaTokenizerFactory.Create();

            // Assert
            Assert.IsNotNull(tokenizer);
            Assert.IsInstanceOf<LinderaTokenizer>(tokenizer);
            Assert.IsTrue(tokenizer.IsValid);
        }

        [Test]
        public void Create_ReturnsILinderaTokenizer()
        {
            // Act
            using var tokenizer = LinderaTokenizerFactory.Create();

            // Assert
            Assert.IsInstanceOf<ILinderaTokenizer>(tokenizer);
        }

        [Test]
        public async System.Threading.Tasks.Task InitializeAsync_ReturnsTrue_InEditor()
        {
            // エディタ上では初期化は常に成功する
            var result = await LinderaTokenizerFactory.InitializeAsync();

            Assert.IsTrue(result);
        }

        [Test]
        public async System.Threading.Tasks.Task CreateAsync_ReturnsValidTokenizer()
        {
            // Act
            using var tokenizer = await LinderaTokenizerFactory.CreateAsync();

            // Assert
            Assert.IsNotNull(tokenizer);
            Assert.IsInstanceOf<ILinderaTokenizer>(tokenizer);
            Assert.IsTrue(tokenizer.IsValid);
        }

        [Test]
        public void Create_TokenizerCanTokenize()
        {
            // Arrange
            using var tokenizer = LinderaTokenizerFactory.Create();

            // Act
            var tokens = tokenizer.Tokenize("東京都");

            // Assert
            Assert.IsNotNull(tokens);
            Assert.Greater(tokens.Length, 0);
        }

        [Test]
        public async System.Threading.Tasks.Task CreateAsync_TokenizerCanTokenize()
        {
            // Arrange
            using var tokenizer = await LinderaTokenizerFactory.CreateAsync();

            // Act
            var tokens = tokenizer.Tokenize("東京都");

            // Assert
            Assert.IsNotNull(tokens);
            Assert.Greater(tokens.Length, 0);
        }

        [Test]
        public void Create_TokenizerCanGetReading()
        {
            // Arrange
            using var tokenizer = LinderaTokenizerFactory.Create();

            // Act
            var reading = tokenizer.GetReading("東京都");

            // Assert
            Assert.IsNotNull(reading);
            Assert.IsNotEmpty(reading);
        }

        [Test]
        public async System.Threading.Tasks.Task CreateAsync_TokenizerCanTokenizeAsync()
        {
            // Arrange
            using var tokenizer = await LinderaTokenizerFactory.CreateAsync();

            // Act
            var tokens = await tokenizer.TokenizeAsync("東京都");

            // Assert
            Assert.IsNotNull(tokens);
            Assert.Greater(tokens.Length, 0);
        }

        [Test]
        public async System.Threading.Tasks.Task CreateAsync_TokenizerCanGetReadingAsync()
        {
            // Arrange
            using var tokenizer = await LinderaTokenizerFactory.CreateAsync();

            // Act
            var reading = await tokenizer.GetReadingAsync("東京都");

            // Assert
            Assert.IsNotNull(reading);
            Assert.IsNotEmpty(reading);
        }

        [Test]
        public void Create_MultipleTokenizersAreIndependent()
        {
            // Arrange
            using var tokenizer1 = LinderaTokenizerFactory.Create();
            using var tokenizer2 = LinderaTokenizerFactory.Create();

            // Assert - 別々のインスタンスであること
            Assert.AreNotSame(tokenizer1, tokenizer2);

            // Both should work independently
            var tokens1 = tokenizer1.Tokenize("東京");
            var tokens2 = tokenizer2.Tokenize("大阪");

            Assert.IsNotNull(tokens1);
            Assert.IsNotNull(tokens2);
        }

        [Test]
        public void Create_DisposedTokenizerThrowsException()
        {
            // Arrange
            var tokenizer = LinderaTokenizerFactory.Create();
            tokenizer.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => tokenizer.Tokenize("test"));
        }
    }
}
