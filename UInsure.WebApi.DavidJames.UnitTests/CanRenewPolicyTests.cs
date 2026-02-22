using FluentAssertions;
using Moq;
using NUnit.Framework;
using UInsure.WebApi.DavidJames.Entities;
using UInsure.WebApi.DavidJames.Services.Exceptions;

namespace UInsure.WebApi.DavidJames.Tests.Services
{
    public partial class PolicyServiceTests
    {
        [TestFixture]
        public class CanRenewPolicyTests : PolicyServiceTests
        {
            [Test]
            public async Task CanRenewPolicy_NonExistentReference_ThrowsPolicyNotFoundException()
            {
                // Arrange
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("MISSING")).ReturnsAsync((Policy?)null);

                // Act
                var act = async () => await _sut.CanRenewPolicy("MISSING");

                // Assert
                await act.Should().ThrowAsync<PolicyNotFoundException>();
            }

            [Test]
            public async Task CanRenewPolicy_ValidRenewablePolicy_ReturnsCanRenewTrue()
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddDays(-340),
                    endDate: Today.AddDays(25));
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CanRenewPolicy("REF001");

                // Assert
                result.CanRenew.Should().BeTrue();
                result.Reason.Should().BeNullOrEmpty();
            }

            [Test]
            public async Task CanRenewPolicy_ExpiredPolicy_ReturnsCanRenewFalseWithReason()
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddYears(-2),
                    endDate: Today.AddYears(-1));
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CanRenewPolicy("REF001");

                // Assert
                result.CanRenew.Should().BeFalse();
                result.Reason.Should().NotBeNullOrEmpty();
            }

            [Test]
            public async Task CanRenewPolicy_TooEarlyToRenew_ReturnsCanRenewFalseWithReason()
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddDays(-300),
                    endDate: Today.AddDays(31));
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CanRenewPolicy("REF001");

                // Assert
                result.CanRenew.Should().BeFalse();
                result.Reason.Should().Contain("30 days");
            }

            [Test]
            public async Task CanRenewPolicy_ChequePayment_ReturnsCanRenewFalseWithReason()
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddDays(-340),
                    endDate: Today.AddDays(25),
                    paymentType: PaymentType.Cheque);
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.CanRenewPolicy("REF001");

                // Assert
                result.CanRenew.Should().BeFalse();
                result.Reason.Should().Contain("direct debit");
            }
        }
    }
}