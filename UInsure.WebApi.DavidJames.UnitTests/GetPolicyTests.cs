using FluentAssertions;
using Moq;
using NUnit.Framework;
using UInsure.WebApi.DavidJames.Entities;
using UInsure.WebApi.DavidJames.Models;
using UInsure.WebApi.DavidJames.Services.Exceptions;

namespace UInsure.WebApi.DavidJames.Tests.Services
{
    public partial class PolicyServiceTests
    {
        [TestFixture]
        public class GetPolicyTests : PolicyServiceTests
        {
            [Test]
            public async Task GetPolicy_ExistingReference_ReturnsMappedModel()
            {
                // Arrange
                var policy = BuildVariableDataModel();
                var expectedModel = new PolicyModel { UniqueReference = policy.UniqueReference };
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("REF001")).ReturnsAsync(policy);
                _mapperMock.Setup(m => m.Map<PolicyModel>(policy)).Returns(expectedModel);
                _mapperMock.Setup(m => m.Map<PaymentTypeModel>(policy.Payment.PaymentType)).Returns(PaymentTypeModel.Card);

                // Act
                var result = await _sut.GetPolicy("REF001");

                // Assert
                result.Should().Be(expectedModel);
                result.PaymentType.Should().Be(PaymentTypeModel.Card);
            }

            [Test]
            public async Task GetPolicy_NonExistentReference_ThrowsPolicyNotFoundException()
            {
                // Arrange
                _repositoryMock.Setup(r => r.GetByReferenceWithDetailsAsync("MISSING")).ReturnsAsync((Policy?)null);

                // Act
                var act = async () => await _sut.GetPolicy("MISSING");

                // Assert
                await act.Should().ThrowAsync<PolicyNotFoundException>();
            }
        }
    }
}