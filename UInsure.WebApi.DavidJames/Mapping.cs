using AutoMapper;
using UInsure.WebApi.DavidJames.Entities;
using UInsure.WebApi.DavidJames.Models;

namespace UInsure.WebApi.DavidJames
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            // Model to DB
            CreateMap<PolicyModel, Policy>();
            CreateMap<PolicyholderModel, Policyholder>();
            CreateMap<PropertyModel, Property>();
            CreateMap<PaymentTypeModel, PaymentType>();

            // DB to model
            CreateMap<Policy, PolicyModel>();
            CreateMap<Policyholder, PolicyholderModel>();
            CreateMap<Property, PropertyModel>();
            CreateMap<PaymentType, PaymentTypeModel>();
        }
    }
}
