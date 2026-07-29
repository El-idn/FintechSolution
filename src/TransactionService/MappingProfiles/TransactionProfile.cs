using AutoMapper;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.DTOs;

namespace TransactionService.MappingProfiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            // Entity to Response mappings
            CreateMap<Transaction, TransactionResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // Request to Entity mappings (for future use)
            CreateMap<TransactionRequest, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousBalance, opt => opt.Ignore())
                .ForMember(dest => dest.NewBalance, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FailureReason, opt => opt.Ignore())
                .ForMember(dest => dest.Reference, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalReference, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "EUR"));

            // TransferRequest to Transaction mapping
            CreateMap<TransferRequest, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.FromAccountId))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => -src.Amount)) // Negative for outgoing transfer
                .ForMember(dest => dest.PreviousBalance, opt => opt.Ignore())
                .ForMember(dest => dest.NewBalance, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => TransactionType.Transfer))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FailureReason, opt => opt.Ignore())
                .ForMember(dest => dest.Reference, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalReference, opt => opt.MapFrom(src => src.ToAccountId.ToString()))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "EUR"));
        }
    }
}
