using System;
using AutoMapper;
using TrafficAnalyticsData.Entities;
using TrafficAnalyticsStore.Models;
using TrafficAnalyticsStore.Models.Dtos;
using TrafficAnalyticsStore.Models.Responses;

namespace TrafficAnalyticsStore;

public class TrafficAnalyticsStoreProfile : Profile
{
    public TrafficAnalyticsStoreProfile()
    {
        // Entities → DTOs
        CreateMap<DailySummary, SummaryDto>().ReverseMap();
        CreateMap<Alert, IncidentDto>().ReverseMap();

        // DTOs → Responses
        CreateMap<SummaryDto, SummaryResponse>();
        CreateMap<IncidentDto, IncidentResponse>();
        CreateMap<CongestionDto, CongestionResponse>();
    }
}