using TableCraft.Core;
using TableCraft.Core.ConfigElements;
using Xunit.Abstractions;

namespace TableCraft.Tests.UnitTests
{
    [Collection("TableCraftTests")]
    public class CodeGenerationTests : IDisposable
    {
        private readonly ITestOutputHelper m_Output;
        private readonly string m_TestDataRoot;
        private readonly string m_TempDir;

        public CodeGenerationTests(ITestOutputHelper output)
        {
            m_Output = output;
            m_TestDataRoot = Path.Combine(AppContext.BaseDirectory, "TestData");
            m_TempDir = Path.Combine(Path.GetTempPath(), $"CodeGen_Test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(m_TempDir);

            // Setup test environment
            SetupTestEnvironment();
        }

        private void SetupTestEnvironment()
        {
            // Use shared configuration path directly - no copying needed
            var sharedLibEnvPath = Path.Combine(m_TestDataRoot, "TestConfigs", "libenv.json");
            
            // Initialize configuration only if not already initialized
            if (!Core.Configuration.IsInited)
            {
                Core.Configuration.ReadConfigurationFromJson(sharedLibEnvPath);
            }
        }

        [Fact]
        public async Task GenerateCodeForUsage_WithValidConfig_ShouldGenerateCode()
        {
            // Arrange
            var configInfo = CreateTestConfigInfo();
            var outputDir = Path.Combine(m_TempDir, "output");
            Directory.CreateDirectory(outputDir);

            // Act
            var result = await ConfigManager.singleton.GenerateCodeForUsage(
                "test-usage", 
                configInfo, 
                outputDir);

            // Assert
            Assert.True(result);
            
            var expectedFile = Path.Combine(outputDir, "TestConfig_Generated.cs");
            Assert.True(File.Exists(expectedFile));

            var generatedCode = await File.ReadAllTextAsync(expectedFile);
            Assert.Contains("namespace TestGenerated", generatedCode);
            Assert.Contains("public class TestConfig", generatedCode);
        }

        [Fact]
        public async Task GenerateCodeForUsage_WithInvalidUsage_ShouldFail()
        {
            // Arrange
            var configInfo = CreateTestConfigInfo();
            var outputDir = Path.Combine(m_TempDir, "output_invalid");
            Directory.CreateDirectory(outputDir);

            // Act
            var result = await ConfigManager.singleton.GenerateCodeForUsage(
                "nonexistent-usage", 
                configInfo, 
                outputDir);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GenerateCodeForUsage_WithDifferentFieldTypes_ShouldGenerateCorrectTypes()
        {
            // Arrange
            var configInfo = CreateTestConfigInfo();
            
            // Configure different field types
            var attributes = configInfo.AttributeInfos.ToList();
            attributes[0].ValueType = "int";
            attributes[1].ValueType = "string";
            attributes[2].ValueType = "float";

            var outputDir = Path.Combine(m_TempDir, "output_types");
            Directory.CreateDirectory(outputDir);

            // Act
            var result = await ConfigManager.singleton.GenerateCodeForUsage(
                "test-usage", 
                configInfo, 
                outputDir);

            // Assert
            Assert.True(result);
            
            var expectedFile = Path.Combine(outputDir, "TestConfig_Generated.cs");
            var generatedCode = await File.ReadAllTextAsync(expectedFile);
            
            Assert.Contains("public int", generatedCode);
            Assert.Contains("public string", generatedCode);
            Assert.Contains("public float", generatedCode);
        }

        [Fact]
        public async Task GenerateCodeForUsageGroup_WithValidGroup_ShouldGenerateMultipleFiles()
        {
            // Arrange
            var configInfo = CreateTestConfigInfo();
            var outputDir = Path.Combine(m_TempDir, "output_group");
            Directory.CreateDirectory(outputDir);

            // Act
            var result = await ConfigManager.singleton.GenerateCodeForUsageGroup(
                "test-group", 
                configInfo, 
                new[] { outputDir });

            // Assert
            Assert.True(result);
            
            var expectedFile = Path.Combine(outputDir, "TestConfig_Generated.cs");
            Assert.True(File.Exists(expectedFile));
        }

        [Theory]
        [InlineData("primary")]
        [InlineData("required")]
        public void ConfigInfo_WithTags_ShouldMaintainTags(string tag)
        {
            // Arrange
            var configInfo = CreateTestConfigInfo();
            var firstAttribute = configInfo.AttributeInfos.First();

            // Act
            firstAttribute.AddTag(tag);

            // Assert
            Assert.Contains(tag, firstAttribute.Tags);
        }

        private ConfigInfo CreateTestConfigInfo()
        {
            // Create a test CSV file to use with ConfigManager
            var testCsvContent = @"ID,Name,Value
Identifier Field,Name Field,Value Field
1,Test,10.5";
            var testCsvPath = Path.Combine(m_TempDir, "TestConfig.csv");
            File.WriteAllText(testCsvPath, testCsvContent);
            
            // Use ConfigManager to create proper ConfigInfo
            var configInfo = ConfigManager.singleton.CreateConfigInfo(testCsvPath, new string[0]);
            
            // Configure attributes with usage information
            var attributes = configInfo.AttributeInfos.ToList();
            if (attributes.Count >= 3)
            {
                // Configure first attribute
                attributes[0].ValueType = "int";
                var usageInfo1 = new ConfigAttributeUsageInfo
                {
                    Usage = "test-usage",
                    FieldName = "ID"
                };
                attributes[0].AddUsageInfo(usageInfo1);
                
                // Configure second attribute
                attributes[1].ValueType = "string";
                var usageInfo2 = new ConfigAttributeUsageInfo
                {
                    Usage = "test-usage",
                    FieldName = "Name"
                };
                attributes[1].AddUsageInfo(usageInfo2);
                
                // Configure third attribute
                attributes[2].ValueType = "float";
                var usageInfo3 = new ConfigAttributeUsageInfo
                {
                    Usage = "test-usage",
                    FieldName = "Value"
                };
                attributes[2].AddUsageInfo(usageInfo3);
            }
            
            return configInfo;
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