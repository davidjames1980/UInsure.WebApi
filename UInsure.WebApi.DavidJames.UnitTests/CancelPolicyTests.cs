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
        public class CancelPolicyTests : PolicyServiceTests
        {
            [Test]
            public async Task CancelPolicy_CancellationDateInPast_ThrowsGeneralApiException()
            {
                // Arrange / Act
                var act = async () => await _sut.CancelPolicy("REF001", Today.AddDays(-1));

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*in the past*");
            }

            [Test]
            public async Task CancelPolicy_NonExistentReference_ThrowsPolicyNotFoundException()
            {
                // Arrange
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("MISSING")).ReturnsAsync((Policy?)null);

                // Act
                var act = async () => await _sut.CancelPolicy("MISSING", Today);

                // Assert
                await act.Should().ThrowAsync<PolicyNotFoundException>();
            }

            [Test]
            public async Task CancelPolicy_PolicyWithActiveClaim_ThrowsGeneralApiException()
            {
                // Arrange
                var policy = BuildVariableDataModel(hasClaims: true);
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("REF001")).ReturnsAsync(policy);

                // Act
                var act = async () => await _sut.CancelPolicy("REF001", Today);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*active claim*");
            }

            [Test]
            public async Task CancelPolicy_CancelledBeforePolicyStart_ReturnsFullRefund()
            {
                // Arrange
                var policy = BuildVariableDataModel(startDate: Today.AddDays(10));
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CancelPolicy("REF001", Today);

                // Assert
                result.Should().Be(policy.Amount);
            }

            [Test]
            public async Task CancelPolicy_CancelledOnStartDate_ReturnsFullRefund()
            {
                // Arrange
                var policy = BuildVariableDataModel(startDate: Today);
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CancelPolicy("REF001", Today);

                // Assert
                result.Should().Be(policy.Amount);
            }

            [Test]
            public async Task CancelPolicy_CancelledOnDay14OfCoolOff_ReturnsFullRefund()
            {
                // Arrange
                var start = Today.AddDays(-14);
                var policy = BuildVariableDataModel(startDate: start);
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CancelPolicy("REF001", Today);

                // Assert
                result.Should().Be(policy.Amount);
            }

            [Test]
            public async Task CancelPolicy_CancelledAfterCoolOff_ReturnsProratedRefund()
            {
                // Arrange
                var start = Today.AddDays(-30);
                var policy = BuildVariableDataModel(startDate: start);
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CancelPolicy("REF001", Today);

                // Assert
                result.Should().BeGreaterThan(0).And.BeLessThan(policy.Amount);
            }

            [Test]
            public async Task CancelPolicy_ProratedRefund_IsCorrectlyCalculated()
            {
                // Arrange
                var cancellationDate = Today.Date.AddMonths(7);
                var policy = new Policy
                {
                    UniqueReference = "123",
                    StartDate = Today.Date,
                    EndDate = Today.AddMonths(12),
                    Amount = 365m,
                    HasClaims = false,
                    Payment = new Payment { PaymentType = PaymentType.Card }
                };
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CancelPolicy("REF001", cancellationDate);

                // Assert
                result.Should().Be(152.00m);
            }
        }
    }
}