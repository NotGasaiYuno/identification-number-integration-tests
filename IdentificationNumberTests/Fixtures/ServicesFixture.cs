﻿using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using Microsoft.Extensions.Configuration;
using IntegrationTests.Constants;
using IntegrationTests.Database;
using Microsoft.EntityFrameworkCore;
using IntegrationTests.Variables;
using IntegrationTests.Selenium.Clients;
using IntegrationTests.Selenium.Providers;
using IntegrationTests.Integration.Authorization;
using IntegrationTests.Integration.Otp;
using IntegrationTests.Integration.Token;
using IntegrationTests.Database.Repositories;
using IdentificationNumberTests.Api;

namespace IntegrationTests.Fixtures;

public sealed class ServicesFixture
{
    private readonly IServiceProvider _serviceProvider;

    public ServicesFixture() =>
        _serviceProvider = ConfigureServices();

    private ServiceProvider ConfigureServices() =>
        new ServiceCollection()
            .AddTransient<IConfiguration>(sp =>
            {
                return new ConfigurationBuilder()
                        .AddJsonFile(FileNames.Appsettings)
                        .Build();
            })
            .AddSingleton<SensitiveData>()
            .AddTransient<WebDriver>(sp =>
            {
                return WebDriverProvider.GetWebDriver();
            })
            .AddTransient<WebDriverClient>()
            .AddTransient<OtpService>()
            .AddTransient<AuthService>()
            .AddTransient<TokenService>()
            .AddTransient<IdNumberVerifierService>()
            .AddDbContext<UserInfoDbContext>((sp, options) =>
            {
                SensitiveData sensitiveData = sp.GetService<SensitiveData>()!;

                options.UseCosmos(
                    accountEndpoint: sensitiveData.CosmosDbInfo.AccountEndpoint,
                    accountKey: sensitiveData.CosmosDbInfo.AccountKey,
                    databaseName: sensitiveData.CosmosDbInfo.DbName);
            })
            .AddTransient<UserInfoDbRepository>()
            .BuildServiceProvider(new ServiceProviderOptions
            { 
                ValidateOnBuild = true
            });

    public TService? GetService<TService>() where TService : class =>
        _serviceProvider.GetService<TService>();
}
