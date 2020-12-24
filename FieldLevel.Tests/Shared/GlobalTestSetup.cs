using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.Configuration;
using FieldLevel.Tests;

namespace FieldLevel.Tests
{
    public class GlobalTestSetup : IDisposable
    {
        public static IConfigurationRoot Configuration;
        public string _connectionString { get; private set; }

        /// <summary>
        /// Do our global setup here; will be called once per test class
        /// </summary>
        public GlobalTestSetup()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _connectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// Do your global teardown here; will be called once per test class
        /// </summary>
        public void Dispose()
        {}

        public string GetConnectionString()
        {
            return _connectionString;
        }
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<GlobalTestSetup>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}