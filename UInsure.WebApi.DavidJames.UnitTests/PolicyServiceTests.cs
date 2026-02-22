using AutoMapper;
using Moq;
using NUnit.Framework;
using UInsure.WebApi.DavidJames.Data;
using UInsure.WebApi.DavidJames.DataModels;
using UInsure.WebApi.DavidJames.Models;
using UInsure.WebApi.DavidJames.Services;

namespace UInsure.WebApi.DavidJames.Tests.Services
{
    [TestFixture]
    public partial class PolicyServiceTests
    {
        private Mock<IPolicyRepository> _repositoryMock;
        private Mock<IMapper> _mapperMock;
        private PolicyService _sut;

        private static readonly DateTime Today = DateTime.UtcNow.Date;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IPolicyRepository>();
            _mapperMock = new Mock<IMapper>();
            _sut = new PolicyService(_repositoryMock.Object, _mapperMock.Object);
        }

        private static PolicyModel BuildValidPolicyModel(
            string reference = "REF001",
            DateTime? startDate = null,
            int holderCount = 1,
            PaymentTypeModel paymentType = PaymentTypeModel.Card)
        {
            var start = startDate ?? Today;
            return new PolicyModel
            {
                UniqueReference = reference,
                StartDate = start,
                EndDate = start.AddYears(1),
                Amount = 500m,
                PaymentType = paymentType,
                Policyholders = Enumerable.Range(0, holderCount)
                    .Select(_ => new PolicyholderModel
                    {
                        DateOfBirth = start.AddYears(-17)
                    })
                    .ToList()
            };
        }

        private static Policy BuildVariableDataModel(
            string reference = "REF001",
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool hasClaims = false,
            bool autoRenew = false,
            PaymentType paymentType = PaymentType.Card)
        {
            var start = startDate ?? Today;
            return new Policy
            {
                UniqueReference = reference,
                StartDate = start,
                EndDate = endDate ?? start.AddYears(1),
                Amount = 500m,
                HasClaims = hasClaims,
                AutoRenew = autoRenew,
                Payment = new Payment { PaymentType = paymentType }
            };
        }
    }
}