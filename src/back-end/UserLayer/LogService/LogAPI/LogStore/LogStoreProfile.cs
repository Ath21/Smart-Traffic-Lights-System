using System;
using AutoMapper;
using LogData.Collections;
using LogStore.Models;

namespace LogStore;

public class LogStoreProfile : Profile
{
    public LogStoreProfile()
    {
        // Mapping from LogDto to Log
        CreateMap<LogDto, Log>()
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Mapping from Log to LogDto
        CreateMap<Log, LogDto>();
    }
}
