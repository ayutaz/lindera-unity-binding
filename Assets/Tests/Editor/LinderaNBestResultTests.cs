using NUnit.Framework;

using LinderaUnityBinding;

namespace LinderaUnityBinding.Tests
{
    /// <summary>
    /// LinderaNBestPath・LinderaNBestResultのユニットテスト
    /// </summary>
    public class LinderaNBestResultTests
    {
        [Test]
        public void LinderaNBestPath_Constructor_SetsProperties()
        {
            // Arrange
            var tokens = new[]
            {
                new LinderaToken("東京", 0, 6, 0, new[] { "名詞", "固有名詞", "地域", "一般", "*", "*", "東京", "トウキョウ", "トーキョー" }),
                new LinderaToken("都", 6, 9, 1, new[] { "名詞", "接尾", "地域", "*", "*", "*", "都", "ト", "ト" })
            };

            // Act
            var path = new LinderaNBestPath(tokens, -12345);

            // Assert
            Assert.AreEqual(2, path.Tokens.Length);
            Assert.AreEqual(-12345, path.Cost);
            Assert.AreEqual("東京", path.Tokens[0].Surface);
            Assert.AreEqual("都", path.Tokens[1].Surface);
        }

        [Test]
        public void LinderaNBestPath_EmptyTokens()
        {
            // Arrange & Act
            var path = new LinderaNBestPath(System.Array.Empty<LinderaToken>(), 0);

            // Assert
            Assert.AreEqual(0, path.Tokens.Length);
            Assert.AreEqual(0, path.Cost);
        }

        [Test]
        public void LinderaNBestPath_NegativeCost()
        {
            // Arrange
            var tokens = new[] { new LinderaToken("テスト", 0, 9, 0, new[] { "名詞" }) };

            // Act
            var path = new LinderaNBestPath(tokens, -99999);

            // Assert
            Assert.AreEqual(-99999, path.Cost);
        }

        [Test]
        public void LinderaNBestResult_Constructor_SetsProperties()
        {
            // Arrange
            var path1 = new LinderaNBestPath(
                new[] { new LinderaToken("東京都", 0, 9, 0, new[] { "名詞" }) },
                -100
            );
            var path2 = new LinderaNBestPath(
                new[]
                {
                    new LinderaToken("東京", 0, 6, 0, new[] { "名詞" }),
                    new LinderaToken("都", 6, 9, 1, new[] { "名詞" })
                },
                -90
            );

            // Act
            var result = new LinderaNBestResult(new[] { path1, path2 });

            // Assert
            Assert.AreEqual(2, result.Paths.Length);
            Assert.AreEqual(-100, result.Paths[0].Cost);
            Assert.AreEqual(-90, result.Paths[1].Cost);
        }

        [Test]
        public void LinderaNBestResult_EmptyPaths()
        {
            // Arrange & Act
            var result = new LinderaNBestResult(System.Array.Empty<LinderaNBestPath>());

            // Assert
            Assert.AreEqual(0, result.Paths.Length);
        }

        [Test]
        public void LinderaNBestResult_SinglePath()
        {
            // Arrange
            var tokens = new[]
            {
                new LinderaToken("東京", 0, 6, 0, new[] { "名詞" }),
                new LinderaToken("都", 6, 9, 1, new[] { "名詞" })
            };
            var path = new LinderaNBestPath(tokens, -500);

            // Act
            var result = new LinderaNBestResult(new[] { path });

            // Assert
            Assert.AreEqual(1, result.Paths.Length);
            Assert.AreEqual(2, result.Paths[0].Tokens.Length);
            Assert.AreEqual(-500, result.Paths[0].Cost);
        }

        [Test]
        public void LinderaNBestPath_TokenDetailsAccessible()
        {
            // Arrange
            var tokens = new[]
            {
                new LinderaToken("東京", 0, 6, 0, new[] { "名詞", "固有名詞", "地域", "一般", "*", "*", "東京", "トウキョウ", "トーキョー" })
            };

            // Act
            var path = new LinderaNBestPath(tokens, -100);

            // Assert
            Assert.AreEqual("名詞", path.Tokens[0].PartOfSpeech);
            Assert.AreEqual("トウキョウ", path.Tokens[0].Reading);
            Assert.AreEqual("東京", path.Tokens[0].BaseForm);
        }
    }
}
