using AutoMapper;
using AccountService.Domain.Entities;
using AccountService.DTOs;
using AccountService.Enums;

namespace AccountService.MappingProfiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            // Entity to DTO
            CreateMap<Account, AccountDto>();

            // Request to Entity (Create new Account)
            CreateMap<CreateAccountRequest, Account>()
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.InitialDeposit))
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore Id, EF will generate
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // if UserId is in request
                .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType));
        }
    }
}