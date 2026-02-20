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
        public class RenewPolicyTests : PolicyServiceTests
        {
            [Test]
            public async Task RenewPolicy_NonExistentReference_ThrowsPolicyNotFoundException()
            {
                // Arrange
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("MISSING")).ReturnsAsync((Policy?)null);

                // Act
                var act = async () => await _sut.RenewPolicy("MISSING");

                // Assert
                await act.Should().ThrowAsync<PolicyNotFoundException>();
            }

            [Test]
            public async Task RenewPolicy_PolicyAlreadyExpired_ThrowsGeneralApiException()
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddYears(-2),
                    endDate: Today.AddYears(-1));
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var act = async () => await _sut.RenewPolicy("REF001");

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*after the end date*");
            }

            [Test]
            public async Task RenewPolicy_MoreThan30DaysBeforeEndDate_ThrowsGeneralApiException()
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddDays(-300),
                    endDate: Today.AddDays(31));
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var act = async () => await _sut.RenewPolicy("REF001");

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*30 days before the end date*");
            }

            [Test]
            public async Task RenewPolicy_ChequePaymentType_ThrowsGeneralApiException()
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddDays(-340),
                    endDate: Today.AddDays(25),
                    paymentType: PaymentType.Cheque);
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var act = async () => await _sut.RenewPolicy("REF001");

                // Assert
                await act.Should().ThrowAsync<GeneralApiException>()
                    .WithMessage("*direct debit and card*");
            }

            [Test]
            public async Task RenewPolicy_ValidPolicy_ExtendsDatesByOneYear()
            {
                // Arrange
                var originalEnd = Today.AddDays(25);
                var policy = BuildVariableDataModel(
                    startDate: Today.AddDays(-340),
                    endDate: originalEnd);
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var result = await _sut.RenewPolicy("REF001");

                // Assert
                result.StartDate.Should().Be(originalEnd);
                result.EndDate.Should().Be(originalEnd.AddYears(1));
            }

            [Test]
            public async Task RenewPolicy_ValidPolicy_SavesChanges()
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddDays(-340),
                    endDate: Today.AddDays(25));
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                await _sut.RenewPolicy("REF001");

                // Assert
                _repositoryMock.Verify(r => r.UpdateAsync(policy), Times.Once);
                _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            }

            [TestCase(PaymentType.Card)]
            [TestCase(PaymentType.DirectDebit)]
            public async Task RenewPolicy_CardOrDirectDebit_DoesNotThrow(PaymentType paymentType)
            {
                // Arrange
                var policy = BuildVariableDataModel(
                    startDate: Today.AddDays(-340),
                    endDate: Today.AddDays(25),
                    paymentType: paymentType);
                _repositoryMock.Setup(r => r.GetByReferenceWithPaymentAsync("REF001")).ReturnsAsync(policy);

                // Act
                var act = async () => await _sut.RenewPolicy("REF001");

                // Assert
                await act.Should().NotThrowAsync();
            }
        }
    }
}