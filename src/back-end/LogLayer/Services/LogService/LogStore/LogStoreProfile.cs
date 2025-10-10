using AutoMapper;
using LogData.Collections;
using LogStore.Models.Responses;
using Messages.Log;
using MongoDB.Bson;

namespace LogStore;

public class LogStoreProfile : Profile
{
    public LogStoreProfile()
    {
        // =========================================================
        // LOG MESSAGE → LOG COLLECTION
        // =========================================================
        CreateMap<LogMessage, AuditLogCollection>()
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.Layer, opt => opt.MapFrom(src => src.Layer))
            .ForMember(dest => dest.Service,
                opt => opt.MapFrom(src =>
                    src.SourceServices != null && src.SourceServices.Any()
                        ? src.SourceServices.First()
                        : "Unknown Service"))
            .ForMember(dest => dest.IntersectionId, opt => opt.MapFrom(src => src.IntersectionId))
            .ForMember(dest => dest.IntersectionName, opt => opt.MapFrom(src => src.IntersectionName))
            .ForMember(dest => dest.LightId, opt => opt.MapFrom(src => src.LightId))
            .ForMember(dest => dest.TrafficLight, opt => opt.MapFrom(src => src.TrafficLight))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Metadata,
                opt => opt.MapFrom(src =>
                    src.Metadata != null ? new BsonDocument(src.Metadata) : null));

        CreateMap<LogMessage, ErrorLogCollection>()
            .IncludeBase<LogMessage, AuditLogCollection>()
            .ForMember(dest => dest.ErrorType,
                opt => opt.MapFrom(src =>
                    src.Metadata != null && src.Metadata.ContainsKey("error_type")
                        ? src.Metadata["error_type"]
                        : "Internal Error"));

        CreateMap<LogMessage, FailoverLogCollection>()
            .IncludeBase<LogMessage, AuditLogCollection>()
            .ForMember(dest => dest.Context,
                opt => opt.MapFrom(src =>
                    src.Metadata != null && src.Metadata.ContainsKey("context")
                        ? src.Metadata["context"]
                        : "N/A"))
            .ForMember(dest => dest.Reason,
                opt => opt.MapFrom(src =>
                    src.Metadata != null && src.Metadata.ContainsKey("reason")
                        ? src.Metadata["reason"]
                        : "Unknown"))
            .ForMember(dest => dest.Mode,
                opt => opt.MapFrom(src =>
                    src.Metadata != null && src.Metadata.ContainsKey("failover_mode")
                        ? src.Metadata["failover_mode"]
                        : "N/A"));

        // =========================================================
        // LOG COLLECTION → SEARCH RESPONSE
        // =========================================================
        CreateMap<AuditLogCollection, SearchLogResponse>()
            .ForMember(dest => dest.LogType, opt => opt.MapFrom(_ => "Audit"))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null ? src.Metadata.ToDictionary() : null));

        CreateMap<ErrorLogCollection, SearchLogResponse>()
            .ForMember(dest => dest.LogType, opt => opt.MapFrom(_ => "Error"))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null ? src.Metadata.ToDictionary() : null));

        CreateMap<FailoverLogCollection, SearchLogResponse>()
            .ForMember(dest => dest.LogType, opt => opt.MapFrom(_ => "Failover"))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null ? src.Metadata.ToDictionary() : null));
    }
}
