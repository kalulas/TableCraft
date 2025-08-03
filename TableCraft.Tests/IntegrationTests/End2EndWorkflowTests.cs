using TableCraft.Core;
using TableCraft.Core.ConfigElements;
using Xunit.Abstractions;

namespace TableCraft.Tests.IntegrationTests
{
    [Collection("TableCraftTests")]
    public class End2EndWorkflowTests : IDisposable
    {
        private readonly ITestOutputHelper m_Output;
        private readonly string m_TestDataRoot;
        private readonly string m_TempWorkDir;

        public End2EndWorkflowTests(ITestOutputHelper output)
        {
            m_Output = output;
            m_TestDataRoot = Path.Combine(AppContext.BaseDirectory, "TestData");
            m_TempWorkDir = Path.Combine(Path.GetTempPath(), $"TableCraft_Test_{Guid.NewGuid():N}");
            
            // Create temp work directory
            Directory.CreateDirectory(m_TempWorkDir);
            Directory.CreateDirectory(Path.Combine(m_TempWorkDir, "Output"));
            
            m_Output.WriteLine($"Test workspace: {m_TempWorkDir}");
        }

        [Fact]
        public async Task End2End_CsvToCodeGeneration_ShouldWork()
        {
            // Arrange: Setup test environment
            await SetupTestEnvironment();

            // Act & Assert: Test the complete workflow
            await TestCsvParsing();
            await TestJsonGeneration();
            await TestCodeGeneration();
        }

        private Task SetupTestEnvironment()
        {
            // Use shared configuration directly
            var sharedLibEnvPath = Path.Combine(m_TestDataRoot, "TestConfigs", "libenv.json");

            // Copy test CSV to work directory
            var sourceCsv = Path.Combine(m_TestDataRoot, "TestCsv", "Students.csv");
            var targetCsv = Path.Combine(m_TempWorkDir, "Students.csv");
            File.Copy(sourceCsv, targetCsv);

            // Initialize TableCraft configuration only if not already initialized
            if (!Core.Configuration.IsInited)
            {
                Core.Configuration.ReadConfigurationFromJson(sharedLibEnvPath);
            }
            
            m_Output.WriteLine("Test environment setup completed");
            return Task.CompletedTask;
        }

        private Task TestCsvParsing()
        {
            // Test CSV file parsing
            var csvPath = Path.Combine(m_TempWorkDir, "Students.csv");
            var configInfo = ConfigManager.singleton.CreateConfigInfo(csvPath, new string[0]);

            Assert.NotNull(configInfo);
            Assert.Equal("Students", configInfo.ConfigName);
            Assert.NotEmpty(configInfo.AttributeInfos);
            
            // Verify expected fields
            var expectedFields = new[] { "ID", "Name", "Age", "Grade", "Courses" };
            foreach (var expectedField in expectedFields)
            {
                var attr = configInfo.AttributeInfos.FirstOrDefault(a => a.AttributeName == expectedField);
                Assert.NotNull(attr);
                m_Output.WriteLine($"Found field: {attr.AttributeName}");
            }

            m_Output.WriteLine("CSV parsing test passed");
            return Task.CompletedTask;
        }

        private async Task TestJsonGeneration()
        {
            // Test JSON descriptor generation
            var csvPath = Path.Combine(m_TempWorkDir, "Students.csv");
            var jsonPath = Path.Combine(m_TempWorkDir, "Students.json");
            
            var configInfo = ConfigManager.singleton.CreateConfigInfo(csvPath, new string[0]);
            Assert.NotNull(configInfo);

            // Configure field types for better testing
            foreach (var attr in configInfo.AttributeInfos)
            {
                switch (attr.AttributeName)
                {
                    case "ID":
                        attr.ValueType = "int";
                        attr.AddTag("primary");
                        attr.AddUsageInfo(new ConfigAttributeUsageInfo
                        {
                            Usage = "test-usage",
                            FieldName = "ID",
                        });
                        break;
                    case "Name":
                        attr.ValueType = "string";
                        attr.AddTag("required");
                        attr.AddUsageInfo(new ConfigAttributeUsageInfo
                        {
                            Usage = "test-usage",
                            FieldName = "Name",
                        });
                        break;
                    case "Age":
                        attr.ValueType = "int";
                        attr.AddUsageInfo(new ConfigAttributeUsageInfo
                        {
                            Usage = "test-usage",
                            FieldName = "Age",
                        });
                        break;
                    case "Grade":
                        attr.ValueType = "string";
                        attr.AddUsageInfo(new ConfigAttributeUsageInfo
                        {
                            Usage = "test-usage",
                            FieldName = "Grade",
                        });
                        break;
                    case "Courses":
                        attr.ValueType = "string";
                        attr.CollectionType = "array";
                        attr.AddUsageInfo(new ConfigAttributeUsageInfo
                        {
                            Usage = "test-usage",
                            FieldName = "Courses",
                        });
                        break;
                }
                
                // Note: Usage configuration would require additional setup through ConfigUsageInfo
                // For test purposes, we'll skip this complex setup
            }

            // Save JSON descriptor
            var saveResult = ConfigManager.singleton.SaveConfigInfoWithDecorator(configInfo, jsonPath);
            Assert.True(saveResult);
            Assert.True(File.Exists(jsonPath));

            // Verify JSON content
            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            Assert.Contains("\"ConfigName\":\"Students\"", jsonContent.Replace(" ", ""));
            Assert.Contains("test-usage", jsonContent);

            m_Output.WriteLine("JSON generation test passed");
        }

        private async Task TestCodeGeneration()
        {
            // Test code generation from CSV + JSON + Template
            var csvPath = Path.Combine(m_TempWorkDir, "Students.csv");
            var jsonPath = Path.Combine(m_TempWorkDir, "Students.json");
            var outputDir = Path.Combine(m_TempWorkDir, "Output");

            // Load config info with JSON descriptor
            var configInfo = ConfigManager.singleton.CreateConfigInfo(csvPath, new[] { jsonPath });
            Assert.NotNull(configInfo);

            // Generate code
            var generationResult = await ConfigManager.singleton.GenerateCodeForUsage(
                "test-usage", 
                configInfo, 
                outputDir);

            Assert.True(generationResult);

            // Verify generated code file exists
            var expectedCodeFile = Path.Combine(outputDir, "Students_Generated.cs");
            Assert.True(File.Exists(expectedCodeFile));

            // Verify generated code content
            var generatedCode = await File.ReadAllTextAsync(expectedCodeFile);
            Assert.Contains("namespace TestGenerated", generatedCode);
            Assert.Contains("public class Students", generatedCode);
            Assert.Contains("public int ID { get; set; }", generatedCode);
            Assert.Contains("public string Name { get; set; }", generatedCode);

            m_Output.WriteLine("Code generation test passed");
            m_Output.WriteLine($"Generated code preview:");
            m_Output.WriteLine(generatedCode);
        }

        public void Dispose()
        {
            // Cleanup temp directory
            if (Directory.Exists(m_TempWorkDir))
            {
                Directory.Delete(m_TempWorkDir, true);
                m_Output.WriteLine($"Cleaned up test workspace: {m_TempWorkDir}");
            }
        }
    }
}