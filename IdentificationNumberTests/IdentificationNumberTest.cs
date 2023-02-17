using FluentAssertions;
using IdentificationNumberTests.Database;
using PINTests.Database.Models;
using PINTests.Database.Repositories;
using PINTests.Fixtures;
using PINTests.Integration.IdNumberVerifier;
using PINTests.Variables;
using Xunit;

namespace PINTests;

public class IdentificationNumberTest : IClassFixture<ServicesFixture>
{
    private readonly IdNumberVerifierService _idNumberVerifierService;
    private readonly UserInfoDbRepository _userInfoDbRepository;
    private readonly SensitiveData _sensitiveData;

    public IdentificationNumberTest(ServicesFixture serviceFixture) 
    {
        _idNumberVerifierService = serviceFixture.GetService<IdNumberVerifierService>()!;
        _userInfoDbRepository = serviceFixture.GetService<UserInfoDbRepository>()!;
        _sensitiveData = serviceFixture.GetService<SensitiveData>()!;
    }

    [Fact]
    public async void CheckIdentificationNumber_WithValidData()
    {
        // Act
        int id = await _idNumberVerifierService
            .CheckIdAsync(_sensitiveData.PersonalInfo.TaxpayerIdentificationNumber);

        UserInfoContext? userInfo = await Try.ExecuteAsync(ct =>
        {
            return _userInfoDbRepository.GetUserInfoAsync($"{id}");
        }, userInfo => userInfo != null);

        // Assert
        id.Should().BePositive();

        userInfo.Should().NotBeNull();
        userInfo!.Id.Should().BeEquivalentTo($"{id}");
        userInfo!.BirthDate.Should().BeEquivalentTo(_sensitiveData.PersonalInfo.BirthDate);
        userInfo.AccountNumber.Should().BeEquivalentTo(_sensitiveData.PersonalInfo.TaxpayerIdentificationNumber);
    }
}