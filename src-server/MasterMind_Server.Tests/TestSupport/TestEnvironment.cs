using DotNetEnv;
using log4net;
using log4net.Config;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System;
using System.IO;

[assembly: Parallelizable(ParallelScope.None)]

namespace MasterMind_Server.Tests
{
    [SetUpFixture]
    public class TestEnvironment
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TestEnvironment));

        [OneTimeSetUp]
        public void Initialize()
        {
            SetDirectoryAndEnvironment();
            XmlConfigurator.Configure();
            TestDatabase.CleanupTestData();
            logger.Info("Test environment initialized and cleaned.");
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            TestDatabase.CleanupTestData();
            logger.Info("Test environment cleaned up.");
        }

        private static void SetDirectoryAndEnvironment()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(baseDir);

            string sourceEnvPath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\MasterMind_Server\.env"));
            string localEnvPath = Path.Combine(baseDir, ".env");

            if (File.Exists(sourceEnvPath))
            {
                SanitizeAndCopyEnv(sourceEnvPath, localEnvPath);
            }
            else if (File.Exists(localEnvPath))
            {
                SanitizeAndCopyEnv(localEnvPath, localEnvPath);
            }

            if (File.Exists(localEnvPath))
            {
                Env.Load(localEnvPath);
            }

            Environment.SetEnvironmentVariable("SMTP_SERVER", "127.0.0.1");
            Environment.SetEnvironmentVariable("SMTP_PORT", "1");
        }

        private static void SanitizeAndCopyEnv(string inputPath, string outputPath)
        {
            try
            {
                string[] lines = File.ReadAllLines(inputPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string trimmed = lines[i].Trim();
                    if (trimmed.StartsWith("SMTP_SERVER="))
                    {
                        lines[i] = "SMTP_SERVER=127.0.0.1";
                    }
                    else if (trimmed.StartsWith("SMTP_PORT="))
                    {
                        lines[i] = "SMTP_PORT=1";
                    }
                }

                File.WriteAllLines(outputPath, lines);
            }
            catch (Exception ex)
            {
                logger.Warn("Could not sanitize test .env file.", ex);
            }
        }
    }
}
