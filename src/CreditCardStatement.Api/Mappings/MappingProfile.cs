using AutoMapper;
using CreditCardStatement.Api.CQRS.Commands;
using CreditCardStatement.Core.DTOs;

namespace CreditCardStatement.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AddPurchaseDto, AddPurchaseCommand>().ReverseMap();
        CreateMap<AddPaymentDto, AddPaymentCommand>().ReverseMap();
    }
}