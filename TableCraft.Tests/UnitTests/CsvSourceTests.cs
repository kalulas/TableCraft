using TableCraft.Core.ConfigElements;
using TableCraft.Core.Source;
using Xunit.Abstractions;

namespace TableCraft.Tests.UnitTests
{
    [Collection("TableCraftTests")]
    public class CsvSourceTests : IDisposable
    {
        private readonly ITestOutputHelper m_Output;
        private readonly string m_TempDir;

        public CsvSourceTests(ITestOutputHelper output)
        {
            m_Output = output;
            m_TempDir = Path.Combine(Path.GetTempPath(), $"CsvSource_Test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(m_TempDir);

            // Use shared configuration from TestData
            var testDataRoot = Path.Combine(AppContext.BaseDirectory, "TestData");
            var sharedConfigPath = Path.Combine(testDataRoot, "TestConfigs", "libenv.json");
            
            // Initialize configuration only if not already initialized
            if (!Core.Configuration.IsInited)
            {
                Core.Configuration.ReadConfigurationFromJson(sharedConfigPath);
            }
        }

        [Fact]
        public void CsvSource_ReadSimpleCsv_ShouldParseCorrectly()
        {
            // Arrange
            var csvContent = @"Name,Age,City
Name Field,Age Field,City Field
Alice,25,New York
Bob,30,London";
            var csvPath = Path.Combine(m_TempDir, "simple.csv");
            File.WriteAllText(csvPath, csvContent);

            // Act
            var csvSource = new CsvSource(csvPath);
            var configInfo = new ConfigInfo("TestConfig");
            csvSource.Fill(configInfo);
            var attributes = configInfo.AttributeInfos.ToList();

            // Assert
            Assert.NotNull(attributes);
            Assert.Equal(3, attributes.Count);
            Assert.Contains(attributes, a => a.AttributeName == "Name");
            Assert.Contains(attributes, a => a.AttributeName == "Age");
            Assert.Contains(attributes, a => a.AttributeName == "City");
        }

        [Fact]
        public void CsvSource_ReadCsvWithComments_ShouldHandleCommentsCorrectly()
        {
            // Arrange
            var csvContent = @"ID,Name,Score
Identifier,Name Field,Score Field
1,Alice,95.5
2,Bob,87.2";
            var csvPath = Path.Combine(m_TempDir, "with_comments.csv");
            File.WriteAllText(csvPath, csvContent);

            // Act
            var csvSource = new CsvSource(csvPath);
            var configInfo = new ConfigInfo("TestConfig");
            csvSource.Fill(configInfo);
            var attributes = configInfo.AttributeInfos.ToList();

            // Assert
            Assert.NotNull(attributes);
            Assert.Equal(3, attributes.Count);
            
            var nameAttr = attributes.FirstOrDefault(a => a.AttributeName == "Name");
            Assert.NotNull(nameAttr);
            Assert.Equal("Name Field", nameAttr.Comment);
        }

        [Fact]
        public void CsvSource_WithInvalidFile_ShouldHandleGracefully()
        {
            // Arrange
            var invalidPath = Path.Combine(m_TempDir, "nonexistent.csv");
            var csvSource = new CsvSource(invalidPath);
            var configInfo = new ConfigInfo("TestConfig");

            // Act & Assert
            var result = csvSource.Fill(configInfo);
            Assert.Null(result);
        }

        [Fact]
        public void CsvSource_WithEmptyFile_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyPath = Path.Combine(m_TempDir, "empty.csv");
            File.WriteAllText(emptyPath, "");

            // Act
            var csvSource = new CsvSource(emptyPath);
            var configInfo = new ConfigInfo("TestConfig");
            csvSource.Fill(configInfo);
            var attributes = configInfo.AttributeInfos.ToList();

            // Assert
            Assert.NotNull(attributes);
            Assert.Empty(attributes);
        }

        [Theory]
        [InlineData("Name,Age\nAlice,25", 2)]
        [InlineData("ID\n1", 1)]
        [InlineData("A,B,C,D,E\n1,2,3,4,5", 5)]
        public void CsvSource_VariousColumnCounts_ShouldParseCorrectly(string csvContent, int expectedColumns)
        {
            // Arrange
            var csvPath = Path.Combine(m_TempDir, $"test_{expectedColumns}_cols.csv");
            File.WriteAllText(csvPath, csvContent);

            // Act
            var csvSource = new CsvSource(csvPath);
            var configInfo = new ConfigInfo("TestConfig");
            csvSource.Fill(configInfo);
            var attributes = configInfo.AttributeInfos.ToList();

            // Assert
            Assert.Equal(expectedColumns, attributes.Count);
        }

        public void Dispose()
        {
            if (Directory.Exists(m_TempDir))
            {
                Directory.Delete(m_TempDir, true);
            }
        }
    }
}