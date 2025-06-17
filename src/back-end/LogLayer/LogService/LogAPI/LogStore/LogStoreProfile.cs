/*
 *  LogStore.LogStoreProfile
 *
 *  This class defines the AutoMapper profile for mapping between LogDto and Log models.
 *  It configures the mappings used in the application to convert between these two types.
 *  The mappings are used to facilitate the transfer of data between different layers of the application,
 *  such as from the data access layer to the business logic layer or from the API layer to the data access layer.
 *  The LogDto is a Data Transfer Object that contains properties for log level, service name, message, and timestamp.
 *  The Log model is the actual log entity that is stored in the database.
 *  The profile is registered with AutoMapper to enable automatic mapping between these types.
 *  The mapping configuration ensures that the Timestamp property is set to the current UTC time when a new LogDto is created,
 *  and it maps the properties from LogDto to Log and vice versa.
 *  This allows for seamless conversion between the DTO used in API requests and the Log entity used in the data store.
 */
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
