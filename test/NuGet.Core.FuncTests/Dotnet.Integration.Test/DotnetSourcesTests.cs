// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuGet.Configuration;
using NuGet.Test.Utility;
using Xunit;

namespace Dotnet.Integration.Test
{
    [Collection(DotnetIntegrationCollection.Name)]
    public class DotnetSourcesTests
    {
        private readonly MsbuildIntegrationTestFixture _fixture;

        public DotnetSourcesTests(MsbuildIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WhenAddingSource_GotAdded()
        {
            using (var pathContext = new SimpleTestPathContext())
            {
                var workingPath = pathContext.WorkingDirectory;
                var settings = pathContext.Settings;

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "add",
                    "source",
                    "https://source.test",
                    "--name",
                    "test_source",
                    "--configfile",
                    settings.ConfigPath
                };

                // Act
                var result = _fixture.RunDotnet(workingPath, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.ExitCode == 0);
                var loadedSettings = Settings.LoadDefaultSettings(root: workingPath, configFileName: null, machineWideSettings: null);
                var packageSourcesSection = loadedSettings.GetSection("packageSources");
                var sourceItem = packageSourcesSection?.GetFirstItemWithAttribute<SourceItem>("key", "test_source");
                Assert.Equal("https://source.test", sourceItem.GetValueAsPath());
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WhenAddingSourceWithCredentials_CredentialsWereAddedAndEncrypted()
        {
            using (var pathContext = new SimpleTestPathContext())
            {
                var workingPath = pathContext.WorkingDirectory;
                var settings = pathContext.Settings;

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "add",
                    "source",
                    "https://source.test",
                    "--name",
                    "test_source",
                    "--username",
                    "test_user_name",
                    "--password",
                    "test_password",
                    "--configfile",
                    settings.ConfigPath
                };

                // Act
                var result = _fixture.RunDotnet(workingPath, string.Join(" ", args), ignoreExitCode: true);


                // Assert
                Assert.True(result.Success, result.Output + " " + result.Errors);

                var loadedSettings = Settings.LoadDefaultSettings(root: workingPath, configFileName: null, machineWideSettings: null);

                var packageSourcesSection = loadedSettings.GetSection("packageSources");
                var sourceItem = packageSourcesSection?.GetFirstItemWithAttribute<SourceItem>("key", "test_source");
                Assert.Equal("https://source.test", sourceItem.GetValueAsPath());

                var sourceCredentialsSection = loadedSettings.GetSection("packageSourceCredentials");
                var credentialItem = sourceCredentialsSection?.Items.First(c => string.Equals(c.ElementName, "test_source", StringComparison.OrdinalIgnoreCase)) as CredentialsItem;
                Assert.NotNull(credentialItem);

                Assert.Equal("test_user_name", credentialItem.Username);

                var password = EncryptionUtility.DecryptString(credentialItem.Password);
                Assert.Equal("test_password", password);
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WarnWhenAddingHttpSource()
        {
            using (var pathContext = new SimpleTestPathContext())
            {
                var workingPath = pathContext.WorkingDirectory;
                var settings = pathContext.Settings;

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "add",
                    "source",
                    "http://source.test",
                    "--name",
                    "test_source",
                    "--configfile",
                    settings.ConfigPath
                };

                // Act
                var result = _fixture.RunDotnet(workingPath, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.Success, result.Output + " " + result.Errors);

                var loadedSettings = Settings.LoadDefaultSettings(root: workingPath, configFileName: null, machineWideSettings: null);

                var packageSourcesSection = loadedSettings.GetSection("packageSources");
                var sourceItem = packageSourcesSection?.GetFirstItemWithAttribute<SourceItem>("key", "test_source");
                Assert.Equal("http://source.test", sourceItem.GetValueAsPath());
                Assert.True(result.Output.Contains("warn : NU1803: You are running the 'add source' operation with an 'http' source, 'http://source.test'. Support for 'http' sources will be removed in a future version."));
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WarnWhenUpdatingHttpSource()
        {
            using (var configFileDirectory = _fixture.CreateTestDirectory())
            {
                var configFileName = "nuget.config";
                var configFilePath = Path.Combine(configFileDirectory, configFileName);

                var nugetConfig =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""test_source"" value=""http://source.test"" />
  </packageSources>
</configuration>";
                CreateXmlFile(configFilePath, nugetConfig);

                var settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                var packageSourceProvider = new PackageSourceProvider(settings);
                var sources = packageSourceProvider.LoadPackageSources().ToList();
                Assert.Single(sources);

                var source = sources.Single();
                Assert.Equal("test_source", source.Name);
                Assert.Equal("http://source.test", source.Source);

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "update",
                    "source",
                    "test_source",
                    "--source",
                    "http://source2.test",
                    "--configfile",
                    configFilePath
                };

                // Act
                var result = _fixture.RunDotnet(configFileDirectory, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.Success, result.Output + " " + result.Errors);

                var loadedSettings = Settings.LoadDefaultSettings(root: configFileDirectory, configFileName: null, machineWideSettings: null);

                var packageSourcesSection = loadedSettings.GetSection("packageSources");
                var sourceItem = packageSourcesSection?.GetFirstItemWithAttribute<SourceItem>("key", "test_source");
                Assert.Equal("http://source2.test", sourceItem.GetValueAsPath());
                Assert.True(result.Output.Contains("warn : NU1803: You are running the 'update source' operation with an 'http' source, 'http://source2.test'. Support for 'http' sources will be removed in a future version."));
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WarnWhenListHttpSource()
        {
            using (var configFileDirectory = _fixture.CreateTestDirectory())
            {
                var configFileName = "nuget.config";
                var configFilePath = Path.Combine(configFileDirectory, configFileName);

                var nugetConfig =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""test_source"" value=""http://source.test"" />
  </packageSources>
</configuration>";
                CreateXmlFile(configFilePath, nugetConfig);

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "list",
                    "source",
                };

                // Act
                var settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                var packageSourceProvider = new PackageSourceProvider(settings);
                var sources = packageSourceProvider.LoadPackageSources().ToList();
                Assert.Single(sources);

                var source = sources.Single();
                Assert.Equal("test_source", source.Name);
                Assert.Equal("http://source.test", source.Source);

                // Act
                var result = _fixture.RunDotnet(configFileDirectory, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.Success, result.Output + " " + result.Errors);
                Assert.True(result.Output.Contains("warn : NU1803: A 'http' source, 'http://source.test', was found. Support for 'http' sources will be removed in a future version."));
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WarnWhenEnableHttpSource()
        {
            using (var configFileDirectory = _fixture.CreateTestDirectory())
            {
                var configFileName = "nuget.config";
                var configFilePath = Path.Combine(configFileDirectory, configFileName);

                var nugetConfig =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""test_source"" value=""http://source.test"" />
  </packageSources>
  <disabledPackageSources>
    <add key=""test_source"" value=""true"" />
  </disabledPackageSources>
</configuration>";
                CreateXmlFile(configFilePath, nugetConfig);

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "enable",
                    "source",
                    "test_source",
                };

                // Act
                var settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                var packageSourceProvider = new PackageSourceProvider(settings);
                var sources = packageSourceProvider.LoadPackageSources().ToList();
                Assert.Single(sources);

                var source = sources.Single();
                Assert.Equal("test_source", source.Name);
                Assert.Equal("http://source.test", source.Source);
                Assert.False(source.IsEnabled);

                // Act
                var result = _fixture.RunDotnet(configFileDirectory, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.ExitCode == 0);
                Assert.True(result.Success, result.Output + " " + result.Errors);

                settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                packageSourceProvider = new PackageSourceProvider(settings);
                sources = packageSourceProvider.LoadPackageSources().ToList();

                var testSources = sources.Where(s => s.Name == "test_source");
                Assert.Single(testSources);
                source = testSources.Single();

                Assert.Equal("test_source", source.Name);
                Assert.Equal("http://source.test", source.Source);
                Assert.True(result.Output.Contains("warn : NU1803: You are running the 'enable source' operation with an 'http' source, 'http://source.test'. Support for 'http' sources will be removed in a future version."));
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WarnWhenDisableHttpSource()
        {
            using (var configFileDirectory = _fixture.CreateTestDirectory())
            {
                var configFileName = "nuget.config";
                var configFilePath = Path.Combine(configFileDirectory, configFileName);

                var nugetConfig =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""test_source"" value=""http://source.test"" />
  </packageSources>
</configuration>";
                CreateXmlFile(configFilePath, nugetConfig);

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "disable",
                    "source",
                    "test_source",
                };

                // Act
                var settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                var packageSourceProvider = new PackageSourceProvider(settings);
                var sources = packageSourceProvider.LoadPackageSources().ToList();
                Assert.Single(sources);

                var source = sources.Single();
                Assert.Equal("test_source", source.Name);
                Assert.Equal("http://source.test", source.Source);
                Assert.True(source.IsEnabled);

                // Act
                var result = _fixture.RunDotnet(configFileDirectory, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.ExitCode == 0);
                Assert.True(result.Success, result.Output + " " + result.Errors);

                settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                packageSourceProvider = new PackageSourceProvider(settings);
                sources = packageSourceProvider.LoadPackageSources().ToList();

                var testSources = sources.Where(s => s.Name == "test_source");
                Assert.Single(testSources);
                source = testSources.Single();

                Assert.Equal("test_source", source.Name);
                Assert.Equal("http://source.test", source.Source);
                Assert.True(result.Output.Contains("warn : NU1803: You are running the 'disable source' operation with an 'http' source, 'http://source.test'. Support for 'http' sources will be removed in a future version."));
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WhenAddingSourceWithCredentialsInClearText_CredentialsWereAddedAndNotEncrypted()
        {
            using (var pathContext = new SimpleTestPathContext())
            {
                var workingPath = pathContext.WorkingDirectory;
                var settings = pathContext.Settings;

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "add",
                    "source",
                    "https://source.test",
                    "--name",
                    "test_source",
                    "--username",
                    "test_user_name",
                    "--password",
                    "test_password",
                    "--store-password-in-clear-text",
                    "--configfile",
                    settings.ConfigPath
                };

                // Act
                var result = _fixture.RunDotnet(workingPath, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.Success, result.Output + " " + result.Errors);

                var loadedSettings = Settings.LoadDefaultSettings(root: workingPath, configFileName: null, machineWideSettings: null);

                var packageSourcesSection = loadedSettings.GetSection("packageSources");
                var sourceItem = packageSourcesSection?.GetFirstItemWithAttribute<SourceItem>("key", "test_source");
                Assert.Equal("https://source.test", sourceItem.GetValueAsPath());

                var sourceCredentialsSection = loadedSettings.GetSection("packageSourceCredentials");
                var credentialItem = sourceCredentialsSection?.Items.First(c => string.Equals(c.ElementName, "test_source", StringComparison.OrdinalIgnoreCase)) as CredentialsItem;
                Assert.NotNull(credentialItem);

                Assert.Equal("test_user_name", credentialItem.Username);
                Assert.Equal("test_password", credentialItem.Password);
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WhenAddingSourceWithCredentialsToUserConfigFile_CredentialsWereAddedAndEncryptedInUserConfigFile()
        {
            // Arrange
            using (var configFileDirectory = _fixture.CreateTestDirectory())
            {
                var configFileName = "nuget.config";
                var configFilePath = Path.Combine(configFileDirectory, configFileName);

                string nugetConfig =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";
                CreateXmlFile(configFilePath, nugetConfig);

                var args = new string[]
                {
                    "nuget",
                    "add",
                    "source",
                    "https://source.test",
                    "--name",
                    "test_source",
                    "--username",
                    "test_user_name",
                    "--password",
                    "test_password",
                    "--configfile",
                    configFilePath
                };

                // Act
                var result = _fixture.RunDotnet(configFileDirectory, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.Success, result.AllOutput);

                var settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                var packageSourcesSection = settings.GetSection("packageSources");
                var sourceItem = packageSourcesSection?.GetFirstItemWithAttribute<SourceItem>("key", "test_source");
                Assert.Equal("https://source.test", sourceItem.GetValueAsPath());

                var sourceCredentialsSection = settings.GetSection("packageSourceCredentials");
                var credentialItem = sourceCredentialsSection?.Items.First(c => string.Equals(c.ElementName, "test_source", StringComparison.OrdinalIgnoreCase)) as CredentialsItem;
                Assert.NotNull(credentialItem);

                Assert.Equal("test_user_name", credentialItem.Username);

                var password = EncryptionUtility.DecryptString(credentialItem.Password);
                Assert.Equal("test_password", password);
            }
        }

        private static void CreateXmlFile(string configFilePath, string nugetConfigString)
        {
            using (var stream = File.OpenWrite(configFilePath))
            {
                var nugetConfigXDoc = XDocument.Parse(nugetConfigString);
                ProjectFileUtils.WriteXmlToFile(nugetConfigXDoc, stream);
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WhenEnablingADisabledSource_SourceBecameEnabled()
        {
            // Arrange
            using (var configFileDirectory = _fixture.CreateTestDirectory())
            {
                var configFileName = "nuget.config";
                var configFilePath = Path.Combine(configFileDirectory, configFileName);

                var nugetConfig =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""test_source"" value=""https://source.test"" />
  </packageSources>
  <disabledPackageSources>
    <add key=""test_source"" value=""true"" />
    <add key=""Microsoft and .NET"" value=""true"" />
  </disabledPackageSources>
</configuration>";
                CreateXmlFile(configFilePath, nugetConfig);

                var args = new string[]
                {
                    "nuget",
                    "enable",
                    "source",
                    "TEST_source", // this should work in a case sensitive manner
                    "--configfile",
                    configFilePath
                };

                // Act
                var settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    machineWideSettings: null);
                var packageSourceProvider = new PackageSourceProvider(settings);
                var sources = packageSourceProvider.LoadPackageSources().ToList();
                Assert.Single(sources);

                var source = sources.Single();
                Assert.Equal("test_source", source.Name);
                Assert.Equal("https://source.test", source.Source);
                Assert.False(source.IsEnabled);

                // Main Act
                var result = _fixture.RunDotnet(configFileDirectory, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.ExitCode == 0);

                settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                var disabledSourcesSection = settings.GetSection("disabledPackageSources");
                var disabledSources = disabledSourcesSection?.Items.Select(c => c as AddItem).Where(c => c != null).ToList();
                Assert.Single(disabledSources);
                var disabledSource = disabledSources.Single();
                Assert.Equal("Microsoft and .NET", disabledSource.Key);

                packageSourceProvider = new PackageSourceProvider(settings);
                sources = packageSourceProvider.LoadPackageSources().ToList();

                var testSources = sources.Where(s => s.Name == "test_source");
                Assert.Single(testSources);
                source = testSources.Single();

                Assert.Equal("test_source", source.Name);
                Assert.Equal("https://source.test", source.Source);
                Assert.True(source.IsEnabled, "Source is not enabled");
            }
        }

        [PlatformFact(Platform.Windows)]
        public void Sources_WhenDisablingAnEnabledSource_SourceBecameDisabled()
        {
            // Arrange
            using (var configFileDirectory = _fixture.CreateTestDirectory())
            {
                var configFileName = "nuget.config";
                var configFilePath = Path.Combine(configFileDirectory, configFileName);

                var nugetConfig =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""test_source"" value=""https://source.test"" />
  </packageSources>
</configuration>";
                CreateXmlFile(configFilePath, nugetConfig);

                var args = new string[]
                {
                    "nuget",
                    "disable",
                    "source",
                    "TEST_source",
                    "--configfile",
                    configFilePath
                };

                // Act
                var settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                var packageSourceProvider = new PackageSourceProvider(settings);
                var sources = packageSourceProvider.LoadPackageSources().ToList();
                Assert.Single(sources);

                var source = sources.Single();
                Assert.Equal("test_source", source.Name);
                Assert.Equal("https://source.test", source.Source);
                Assert.True(source.IsEnabled);

                // Main Act
                var result = _fixture.RunDotnet(configFileDirectory, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.ExitCode == 0);

                settings = Settings.LoadDefaultSettings(
                    configFileDirectory,
                    configFileName,
                    null);

                packageSourceProvider = new PackageSourceProvider(settings);
                sources = packageSourceProvider.LoadPackageSources().ToList();

                var testSources = sources.Where(s => s.Name == "test_source");
                Assert.Single(testSources);
                source = testSources.Single();

                Assert.Equal("test_source", source.Name);
                Assert.Equal("https://source.test", source.Source);
                Assert.False(source.IsEnabled, "Source is not disabled");
            }
        }

        [PlatformTheory(Platform.Windows)]
        [InlineData("list source --foo", 2)]
        [InlineData("add source foo bar", 3)]
        [InlineData("remove source a b", 3)]
        [InlineData("remove a b c", 1)]
        [InlineData("add source B a --configfile file.txt --name x --source y", 3)]
        [InlineData("list source --configfile file.txt B a", 4)]
        public void Sources_WhenPassingInvalidArguments_ProperErrorsAreRaised(string cmd, int badParam)
        {
            // all of these commands need to start with "nuget ", and need to adjust bad param to account for those 2 new params
            TestCommandInvalidArguments("nuget " + cmd, badParam + 1);
        }

        [Fact(Skip = "cutting verbosity Quiet for now. #6374 covers fixing it for `dotnet add package` too.")]
        public void TestVerbosityQuiet_DoesNotShowInfoMessages()
        {
            using (var pathContext = new SimpleTestPathContext())
            {
                var workingPath = pathContext.WorkingDirectory;
                var settings = pathContext.Settings;

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "add",
                    "source",
                    "https://source.test",
                    "--name",
                    "test_source",
                    "--verbosity",
                    "Quiet",
                    "--configfile",
                    settings.ConfigPath
                };

                // Act
                var result = _fixture.RunDotnet(workingPath, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.ExitCode == 0);
                // Ensure that no messages are shown with Verbosity as Quiet
                Assert.Equal(string.Empty, result.Output);
                var loadedSettings = Settings.LoadDefaultSettings(root: workingPath, configFileName: null, machineWideSettings: null);
                var packageSourcesSection = loadedSettings.GetSection("packageSources");
                var sourceItem = packageSourcesSection?.GetFirstItemWithAttribute<SourceItem>("key", "test_source");

                Assert.Equal("https://source.test", sourceItem.GetValueAsPath());
            }
        }

        [PlatformFact(Platform.Windows)]
        public void List_Sources_LocalizatedPackagesourceKeys_ConsideredDiffererent()
        {
            using (var pathContext = new SimpleTestPathContext())
            {
                var workingPath = pathContext.WorkingDirectory;
                var settings = pathContext.Settings;
                var nugetConfig =
    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""encyclopaedia"" value=""https://source.test1"" />
    <add key=""Encyclopaedia"" value=""https://source.test2"" />
    <add key=""encyclopædia"" value=""https://source.test3"" />
    <add key=""Encyclopædia"" value=""https://source.test4"" />
  </packageSources>
</configuration>";
                CreateXmlFile(settings.ConfigPath, nugetConfig);

                // Arrange
                var args = new string[]
                {
                    "nuget",
                    "list",
                    "source",
                    "--configfile",
                    settings.ConfigPath
                };

                // Act
                var result = _fixture.RunDotnet(workingPath, string.Join(" ", args), ignoreExitCode: true);

                // Assert
                Assert.True(result.ExitCode == 0);
                Assert.True(result.Output.StartsWith("Registered Sources:"));
                Assert.Contains("encyclopaedia [Enabled]", result.Output);
                Assert.Contains("encyclopædia [Enabled]", result.Output);
                Assert.DoesNotContain("Encyclopaedia", result.Output);
                Assert.DoesNotContain("Encyclopædia", result.Output);
            }
        }

        /// <summary>
        /// Verify non-zero status code and proper messages
        /// </summary>
        /// <remarks>Checks invalid arguments message in stderr, check help message in stdout</remarks>
        /// <param name="commandName">The nuget.exe command name to verify, without "nuget.exe" at the beginning</param>
        public void TestCommandInvalidArguments(string command, int badCommandIndex)
        {
            using (var testDirectory = _fixture.CreateTestDirectory())
            {
                // Act
                var result = _fixture.RunDotnet(testDirectory, command, ignoreExitCode: true);

                var commandSplit = command.Split(' ');

                // Break the test if no proper command is found
                if (commandSplit.Length < 1 || string.IsNullOrEmpty(commandSplit[0]))
                    Assert.True(false, "command not found");

                // 0th - "nuget"
                // 1st - "source"
                // 2nd - action
                // 3rd - nextParam
                string badCommand = commandSplit[badCommandIndex];

                // Assert command
                Assert.Contains("'" + badCommand + "'", result.Output, StringComparison.InvariantCultureIgnoreCase);


                // Assert invalid argument message
                string invalidMessage;
                if (badCommand.StartsWith("-"))
                {
                    invalidMessage = "error: Unrecognized option";
                }
                else
                {
                    invalidMessage = "error: Unrecognized command";
                }

                // Verify Exit code
                VerifyResultFailure(result, invalidMessage);
                // Verify traits of help message in stdout
                Assert.Contains("Specify --help for a list of available options and commands.", result.Output);
            }
        }

        /// <summary>
        /// Utility for asserting faulty executions of dotnet.exe
        /// 
        /// Asserts a non-zero status code and a message on stderr.
        /// </summary>
        /// <param name="result">An instance of <see cref="CommandRunnerResult"/> with command execution results</param>
        /// <param name="expectedErrorMessage">A portion of the error message to be sent</param>
        public static void VerifyResultFailure(CommandRunnerResult result,
                                               string expectedErrorMessage)
        {
            Assert.False(
                result.Success,
                "dotnet.exe nuget DID NOT FAIL: Output is " + result.Output + ". Error is " + result.Errors);

            Assert.True(
                result.Output.Contains(expectedErrorMessage),
                "Expected error is " + expectedErrorMessage + ". Actual error is " + result.Output);
        }
    }
}
