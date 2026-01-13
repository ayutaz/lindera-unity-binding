using NUnit.Framework;

namespace Lindera.Tests
{
    /// <summary>
    /// LinderaTokenのユニットテスト
    /// </summary>
    public class LinderaTokenTests
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var surface = "東京";
            var byteStart = 0u;
            var byteEnd = 6u;
            var position = 0u;
            var details = new[] { "名詞", "固有名詞", "地域", "一般", "*", "*", "東京", "トウキョウ", "トーキョー" };

            // Act
            var token = new LinderaToken(surface, byteStart, byteEnd, position, details);

            // Assert
            Assert.AreEqual(surface, token.Surface);
            Assert.AreEqual(byteStart, token.ByteStart);
            Assert.AreEqual(byteEnd, token.ByteEnd);
            Assert.AreEqual(position, token.Position);
            Assert.AreEqual(details, token.Details);
        }

        [Test]
        public void PartOfSpeech_ReturnsFirstDetail()
        {
            // Arrange
            var details = new[] { "名詞", "固有名詞", "地域", "一般", "*", "*", "東京", "トウキョウ", "トーキョー" };
            var token = new LinderaToken("東京", 0, 6, 0, details);

            // Act
            var partOfSpeech = token.PartOfSpeech;

            // Assert
            Assert.AreEqual("名詞", partOfSpeech);
        }

        [Test]
        public void PartOfSpeech_ReturnsNull_WhenDetailsEmpty()
        {
            // Arrange
            var token = new LinderaToken("test", 0, 4, 0, System.Array.Empty<string>());

            // Act
            var partOfSpeech = token.PartOfSpeech;

            // Assert
            Assert.IsNull(partOfSpeech);
        }

        [Test]
        public void PartOfSpeech_ReturnsNull_WhenDetailsNull()
        {
            // Arrange
            var token = new LinderaToken("test", 0, 4, 0, null);

            // Act
            var partOfSpeech = token.PartOfSpeech;

            // Assert
            Assert.IsNull(partOfSpeech);
        }

        [Test]
        public void Reading_ReturnsIndex7_ForIPADIC()
        {
            // Arrange - IPADICフォーマット: 品詞,品詞細分類1,品詞細分類2,品詞細分類3,活用型,活用形,原形,読み,発音
            var details = new[] { "名詞", "固有名詞", "地域", "一般", "*", "*", "東京", "トウキョウ", "トーキョー" };
            var token = new LinderaToken("東京", 0, 6, 0, details);

            // Act
            var reading = token.Reading;

            // Assert
            Assert.AreEqual("トウキョウ", reading);
        }

        [Test]
        public void Reading_ReturnsNull_WhenDetailsHasLessThan8Elements()
        {
            // Arrange
            var details = new[] { "名詞", "一般" };
            var token = new LinderaToken("test", 0, 4, 0, details);

            // Act
            var reading = token.Reading;

            // Assert
            Assert.IsNull(reading);
        }

        [Test]
        public void BaseForm_ReturnsIndex6_ForIPADIC()
        {
            // Arrange
            var details = new[] { "動詞", "自立", "*", "*", "五段・カ行イ音便", "連用タ接続", "書く", "カイ", "カイ" };
            var token = new LinderaToken("書い", 0, 6, 0, details);

            // Act
            var baseForm = token.BaseForm;

            // Assert
            Assert.AreEqual("書く", baseForm);
        }

        [Test]
        public void BaseForm_ReturnsNull_WhenDetailsHasLessThan7Elements()
        {
            // Arrange
            var details = new[] { "名詞", "一般" };
            var token = new LinderaToken("test", 0, 4, 0, details);

            // Act
            var baseForm = token.BaseForm;

            // Assert
            Assert.IsNull(baseForm);
        }

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var details = new[] { "名詞", "固有名詞", "地域", "一般", "*", "*", "東京", "トウキョウ", "トーキョー" };
            var token = new LinderaToken("東京", 0, 6, 0, details);

            // Act
            var result = token.ToString();

            // Assert
            Assert.AreEqual("東京 [名詞]", result);
        }

        [Test]
        public void ToString_HandlesNullPartOfSpeech()
        {
            // Arrange
            var token = new LinderaToken("test", 0, 4, 0, null);

            // Act
            var result = token.ToString();

            // Assert
            Assert.AreEqual("test []", result);
        }
    }
}
