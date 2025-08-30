using AutoMapper;
using LogData.Collections;
using LogStore.Models.Dtos;
using LogStore.Models.Responses;
using MongoDB.Bson;

namespace LogStore;

public class LogStoreProfile : Profile
{
    public LogStoreProfile()
    {
        // Data → DTO
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.Metadata, 
                opt => opt.MapFrom(src => src.Metadata.ToDictionary()));

        CreateMap<ErrorLog, ErrorLogDto>()
            .ForMember(dest => dest.Metadata, 
                opt => opt.MapFrom(src => src.Metadata.ToDictionary()));

        // DTO → Response
        CreateMap<AuditLogDto, AuditLogResponse>();
        CreateMap<ErrorLogDto, ErrorLogResponse>();
    }
}

// Helper extension for BsonDocument → Dictionary
public static class BsonExtensions
{
    public static Dictionary<string, object> ToDictionary(this BsonDocument doc)
    {
        return doc.Elements.ToDictionary(
            e => e.Name,
            e => (object)e.Value.ToString()
        );
    }
}
