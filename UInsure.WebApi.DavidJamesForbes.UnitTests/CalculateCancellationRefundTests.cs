using FluentAssertions;
using Moq;
using NUnit.Framework;
using UInsure.WebApi.DavidJamesForbes.DataModels;
using UInsure.WebApi.DavidJamesForbes.Services.Exceptions;

namespace UInsure.WebApi.DavidJamesForbes.Tests.Services
{
    public partial class PolicyServiceTests
    {
        [TestFixture]
        public class CalculateCancellationRefundTests : PolicyServiceTests
        {
            [Test]
            public async Task CalculateCancellationRefund_CancellationDateInPast_ThrowsGeneralApiException()
            {
                // Arrange / Act
                var act = async () => await _sut.CalculateCancellationRefund("REF001", Today.AddDays(-1));

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*in the past*");
            }

            [Test]
            public async Task CalculateCancellationRefund_NonExistentReference_ThrowsPolicyNotFoundException()
            {
                // Arrange
                _repositoryMock.Setup(r => r.GetByReferenceAsync("MISSING")).ReturnsAsync((Policy?)null);

                // Act
                var act = async () => await _sut.CalculateCancellationRefund("MISSING", Today);

                // Assert
                await act.Should().ThrowAsync<PolicyNotFoundException>();
            }

            [Test]
            public async Task CalculateCancellationRefund_PolicyWithClaim_ThrowsGeneralApiException()
            {
                // Arrange
                var policy = BuildVariableDataModel(hasClaims: true);
                _repositoryMock.Setup(r => r.GetByReferenceAsync("REF001")).ReturnsAsync(policy);

                // Act
                var act = async () => await _sut.CalculateCancellationRefund("REF001", Today);

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*active claim*");
            }

            [Test]
            public async Task CalculateCancellationRefund_ValidRequest_ReturnsExpectedAmount()
            {
                // Arrange
                var start = Today.Date;
                var policy = new Policy
                {
                    UniqueReference = "123",
                    StartDate = start,
                    EndDate = start.AddYears(1),
                    Amount = 365m,
                    HasClaims = false,
                    Payment = new Payment { PaymentType = PaymentType.Card }
                };
                _repositoryMock.Setup(r => r.GetByReferenceAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CalculateCancellationRefund("REF001", Today.AddMonths(5));

                // Assert
                result.Should().Be(214.00m);
            }

            [Test]
            public async Task CalculateCancellationRefund_RefundRoundedToTwoDecimalPlaces()
            {
                // Arrange
                var start = Today.Date;
                var end = start.AddYears(1);
                var cancellationDate = start.AddMonths(6);
                var policy = new Policy
                {
                    UniqueReference = "123",
                    StartDate = start,
                    EndDate = end,
                    Amount = 100m,
                    HasClaims = false,
                    Payment = new Payment { PaymentType = PaymentType.Card }
                };
                _repositoryMock.Setup(r => r.GetByReferenceAsync("REF001")).ReturnsAsync(policy);
                var totalDays = (end - start).Days;
                var usedDays = (cancellationDate - start).Days + 1;
                var unusedDays = totalDays - usedDays;
                var expectedRefund = Math.Round(100m * unusedDays / totalDays, 2, MidpointRounding.AwayFromZero);

                // Act
                var result = await _sut.CalculateCancellationRefund("REF001", cancellationDate);

                // Assert
                result.Should().Be(expectedRefund);
            }
        }
    }
}