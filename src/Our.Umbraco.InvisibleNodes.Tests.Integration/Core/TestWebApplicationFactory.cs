// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Core;

[Collection("Web")]
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString = "Data Source=InMemory;Mode=Memory;Cache=Shared;Pooling=True";
    private readonly SqliteConnection _sharedConnection;

    public TestWebApplicationFactory()
    {
        _sharedConnection = new SqliteConnection(_connectionString);
        _sharedConnection.Open();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            var inlineConfig = new Dictionary<string, string?>()
            {
                { "ConnectionStrings:umbracoDbDSN", _connectionString },
            };
            
            config.AddInMemoryCollection(inlineConfig);
        });

        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        _sharedConnection.Dispose();
        base.Dispose(disposing);
    }
}