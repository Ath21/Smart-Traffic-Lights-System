using AutoMapper;
using LogData.Collections;
using LogStore.Models.Dtos;
using MongoDB.Bson;

namespace LogStore;

public class LogStoreProfile : Profile
{
    public LogStoreProfile()
    {
        // ==========================
        // AUDIT LOGS
        // ==========================
        CreateMap<AuditLogDto, AuditLog>()
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null
                    ? new BsonDocument(src.Metadata)
                    : new BsonDocument()));

        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null
                    ? src.Metadata.ToDictionary()
                    : new Dictionary<string, object>()));

        // ==========================
        // ERROR LOGS
        // ==========================
        CreateMap<ErrorLogDto, ErrorLog>()
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null
                    ? new BsonDocument(src.Metadata)
                    : new BsonDocument()));

        CreateMap<ErrorLog, ErrorLogDto>()
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null
                    ? src.Metadata.ToDictionary()
                    : new Dictionary<string, object>()));

        // ==========================
        // FAILOVER LOGS
        // ==========================
        CreateMap<FailoverLogDto, FailoverLog>()
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null
                    ? new BsonDocument(src.Metadata)
                    : new BsonDocument()));

        CreateMap<FailoverLog, FailoverLogDto>()
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null
                    ? src.Metadata.ToDictionary()
                    : new Dictionary<string, object>()));
    }
}

// ==========================
// BSON EXTENSIONS
// ==========================
public static class BsonExtensions
{
    public static Dictionary<string, object> ToDictionary(this BsonDocument doc)
    {
        return doc.Elements.ToDictionary(
            e => e.Name,
            e => BsonTypeMapper.MapToDotNetValue(e.Value) // safer than ToString()
        );
    }
}
