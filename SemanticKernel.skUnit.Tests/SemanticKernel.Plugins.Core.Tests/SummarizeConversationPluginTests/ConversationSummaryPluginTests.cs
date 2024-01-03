using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using skUnit;
using skUnit.Scenarios;
using Xunit.Abstractions;

namespace SemanticKernel.Plugins.Core.Tests.SummarizeConversationTests
{
    public class ConversationSummaryPluginTests
    {
        private SemanticKernelAssert SemanticKernelAssert { get; set; }
        private Kernel Kernel { get; set; }
        public ConversationSummaryPluginTests(ITestOutputHelper output)
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                                .Build();

            var appSettings = new AppSettings();
            configuration.Bind(appSettings);

            var apiKey = appSettings.ApiKey;
            var endpoint = appSettings.Endpoint;
            var deploymentName = appSettings.DeploymentName;

            SemanticKernelAssert = new SemanticKernelAssert(deploymentName, endpoint, apiKey, output.WriteLine);

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            builder.Plugins.AddFromType<ConversationSummaryPlugin>();
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            Kernel = builder.Build();
        }

        [Theory]
        [MemberData(nameof(GetScenarios))]
        public async Task SummarizeConversation(string scenarioName)
        {
            var scenarios = await InvocationScenario.LoadFromResourceAsync($"{ScenarioPrefix}.{scenarioName}", Assembly.GetExecutingAssembly());
            var function = Kernel.Plugins["ConversationSummaryPlugin"]["SummarizeConversation"];

            await SemanticKernelAssert.CheckScenarioAsync(Kernel, function, scenarios);
        }

        public static string ScenarioPrefix =
            "SemanticKernel.Plugins.Core.Tests.SummarizeConversationPluginTests.Scenarios";

        public static List<string> GetScenarioNames()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var names = assembly.GetManifestResourceNames()
                                .Where(s => s.StartsWith(ScenarioPrefix))
                                .Select(s => s.Substring(ScenarioPrefix.Length + 1))
                                .ToList();

            return names;
        }

        public static IEnumerable<object[]> GetScenarios()
        {
            var scenarios = GetScenarioNames();
            return scenarios.Select(s => new object[] { s });
        }
    }
}