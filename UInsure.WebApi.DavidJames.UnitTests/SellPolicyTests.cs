using FluentAssertions;
using Moq;
using NUnit.Framework;
using UInsure.WebApi.DavidJames.DataModels;
using UInsure.WebApi.DavidJames.Services.Exceptions;

namespace UInsure.WebApi.DavidJames.Tests.Services
{
    public partial class PolicyServiceTests
    {
        [TestFixture]
        public class SellPolicyTests : PolicyServiceTests
        {
            [Test]
            public async Task SellPolicy_ValidPolicy_SavesAndReturnsPolicy()
            {
                // Arrange
                var model = BuildValidPolicyModel();
                var mappedPolicy = BuildVariableDataModel();
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);
                _mapperMock.Setup(m => m.Map<Policy>(model)).Returns(mappedPolicy);
                _mapperMock.Setup(m => m.Map<PaymentType>(model.PaymentType)).Returns(PaymentType.Card);

                // Act
                var result = await _sut.SellPolicy(model);

                // Assert
                result.Should().Be(mappedPolicy);
                _repositoryMock.Verify(r => r.AddAsync(mappedPolicy), Times.Once);
                _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            }

            [Test]
            public async Task SellPolicy_ValidPolicy_SetsPaymentReference()
            {
                // Arrange
                var model = BuildValidPolicyModel();
                var mappedPolicy = BuildVariableDataModel();
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);
                _mapperMock.Setup(m => m.Map<Policy>(model)).Returns(mappedPolicy);
                _mapperMock.Setup(m => m.Map<PaymentType>(model.PaymentType)).Returns(PaymentType.Card);

                // Act
                await _sut.SellPolicy(model);

                // Assert
                mappedPolicy.Payment.Should().NotBeNull();
                mappedPolicy.Payment.PaymentReference.Should().NotBeNullOrWhiteSpace();
            }

            [Test]
            public async Task SellPolicy_DuplicateReference_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel();
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(true);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*UniqueReference already exists*");
            }

            [Test]
            public async Task SellPolicy_StartDateInPast_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel(startDate: Today.AddDays(-1));
                model.EndDate = model.StartDate.AddYears(1);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*StartDate must be within the next 60 days*");
            }

            [Test]
            public async Task SellPolicy_StartDateMoreThan60DaysAhead_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel(startDate: Today.AddDays(61));
                model.EndDate = model.StartDate.AddYears(1);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*StartDate must be within the next 60 days*");
            }

            [Test]
            public async Task SellPolicy_StartDateExactly60DaysAhead_DoesNotThrow()
            {
                // Arrange
                var model = BuildValidPolicyModel(startDate: Today.AddDays(60));
                model.EndDate = model.StartDate.AddYears(1);
                var mappedPolicy = BuildVariableDataModel(startDate: model.StartDate, endDate: model.EndDate);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);
                _mapperMock.Setup(m => m.Map<Policy>(model)).Returns(mappedPolicy);
                _mapperMock.Setup(m => m.Map<PaymentType>(model.PaymentType)).Returns(PaymentType.Card);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().NotThrowAsync();
            }

            [Test]
            public async Task SellPolicy_EndDateNotOneYearFromStart_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel();
                model.EndDate = model.StartDate.AddMonths(6);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*one annual year*");
            }

            [Test]
            public async Task SellPolicy_NoPolicyholders_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel(holderCount: 0);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*between 1 and 3 policy holders*");
            }

            [Test]
            public async Task SellPolicy_FourPolicyholders_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel(holderCount: 4);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*between 1 and 3 policy holders*");
            }

            [TestCase(1)]
            [TestCase(2)]
            [TestCase(3)]
            public async Task SellPolicy_ValidPolicyholderCount_DoesNotThrow(int count)
            {
                // Arrange
                var model = BuildValidPolicyModel(holderCount: count);
                var mappedPolicy = BuildVariableDataModel();
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);
                _mapperMock.Setup(m => m.Map<Policy>(model)).Returns(mappedPolicy);
                _mapperMock.Setup(m => m.Map<PaymentType>(model.PaymentType)).Returns(PaymentType.Card);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().NotThrowAsync();
            }

            [Test]
            public async Task SellPolicy_PolicyholderExactly16OnStartDate_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel();
                model.Policyholders[0].DateOfBirth = model.StartDate.AddYears(-16);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);
                        
                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*over the age of 16*");
            }

            [Test]
            public async Task SellPolicy_PolicyholderUnder16OnStartDate_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel();
                model.Policyholders[0].DateOfBirth = model.StartDate.AddYears(-15);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*over the age of 16*");
            }

            [Test]
            public async Task SellPolicy_OnePolicyholderUnder16WithOthersValid_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel(holderCount: 3);
                model.Policyholders[2].DateOfBirth = model.StartDate.AddYears(-15);
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*over the age of 16*");
            }

            [Test]
            public async Task SellPolicy_InvalidPostcode_ThrowsGeneralApiException()
            {
                // Arrange
                var model = BuildValidPolicyModel(holderCount: 3);
                model.Property.Postcode = "12345";
                _repositoryMock.Setup(r => r.ExistsAsync(model.UniqueReference)).ReturnsAsync(false);

                // Act
                var act = async () => await _sut.SellPolicy(model);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*postcode is not valid*");
            }
        }
    }
}