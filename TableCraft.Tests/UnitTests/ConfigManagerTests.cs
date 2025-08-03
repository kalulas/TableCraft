using TableCraft.Core;
using TableCraft.Core.ConfigElements;
using Xunit.Abstractions;

namespace TableCraft.Tests.UnitTests
{
    [Collection("TableCraftTests")]
    public class ConfigManagerTests : IDisposable
    {
        private readonly ITestOutputHelper m_Output;
        private readonly string m_TestDataRoot;
        private readonly string m_TempDir;

        public ConfigManagerTests(ITestOutputHelper output)
        {
            m_Output = output;
            m_TestDataRoot = Path.Combine(AppContext.BaseDirectory, "TestData");
            m_TempDir = Path.Combine(Path.GetTempPath(), $"ConfigManager_Test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(m_TempDir);

            // Initialize configuration only if not already initialized
            var libEnvPath = Path.Combine(m_TestDataRoot, "TestConfigs", "libenv.json");
            if (!Core.Configuration.IsInited)
            {
                Core.Configuration.ReadConfigurationFromJson(libEnvPath);
            }
        }

        [Fact]
        public void CreateConfigInfo_WithValidCsv_ShouldReturnConfigInfo()
        {
            // Arrange
            var csvPath = Path.Combine(m_TestDataRoot, "TestCsv", "Students.csv");

            // Act
            var configInfo = ConfigManager.singleton.CreateConfigInfo(csvPath, new string[0]);

            // Assert
            Assert.NotNull(configInfo);
            Assert.Equal("Students", configInfo.ConfigName);
            Assert.NotEmpty(configInfo.AttributeInfos);
            Assert.Equal(5, configInfo.AttributeInfos.Count()); // ID, Name, Age, Grade, Courses
        }

        [Fact]
        public void CreateConfigInfo_WithInvalidPath_ShouldThrowException()
        {
            // Arrange
            var invalidPath = Path.Combine(m_TempDir, "nonexistent.csv");

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => ConfigManager.singleton.CreateConfigInfo(invalidPath, new string[0]));
        }

        [Fact]
        public void SaveConfigInfoWithDecorator_WithValidConfig_ShouldCreateJsonFile()
        {
            // Arrange
            var csvPath = Path.Combine(m_TestDataRoot, "TestCsv", "Students.csv");
            var jsonPath = Path.Combine(m_TempDir, "test_output.json");
            var configInfo = ConfigManager.singleton.CreateConfigInfo(csvPath, new string[0]);
            
            Assert.NotNull(configInfo);

            // Act
            var result = ConfigManager.singleton.SaveConfigInfoWithDecorator(configInfo, jsonPath);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(jsonPath));
            
            var jsonContent = File.ReadAllText(jsonPath);
            Assert.Contains("Students", jsonContent);
            Assert.Contains("Attributes", jsonContent);
        }

        [Theory]
        [InlineData("ID", "int")]
        [InlineData("Name", "string")]
        [InlineData("Age", "int")]
        public void ConfigInfo_AttributeConfiguration_ShouldWorkCorrectly(string attributeName, string valueType)
        {
            // Arrange
            var csvPath = Path.Combine(m_TestDataRoot, "TestCsv", "Students.csv");
            var configInfo = ConfigManager.singleton.CreateConfigInfo(csvPath, new string[0]);
            
            Assert.NotNull(configInfo);

            // Act
            var attribute = configInfo.AttributeInfos.FirstOrDefault(a => a.AttributeName == attributeName);
            Assert.NotNull(attribute);
            
            attribute.ValueType = valueType;
            
            // Create and add usage info
            var usageInfo = new ConfigAttributeUsageInfo
            {
                Usage = "test-usage",
                FieldName = attributeName
            };
            attribute.AddUsageInfo(usageInfo);

            // Assert
            Assert.Equal(valueType, attribute.ValueType);
            Assert.True(attribute.HasUsage("test-usage"));
            Assert.Equal(attributeName, attribute.GetUsageFieldName("test-usage"));
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