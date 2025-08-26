using System;
using AutoMapper;
using TrafficDataAnalyticsData.Entities;
using TrafficDataAnalyticsStore.Models;
using TrafficDataAnalyticsStore.Models.Dtos;
using TrafficDataAnalyticsStore.Models.Responses;

namespace TrafficDataAnalyticsStore;

public class TrafficDataAnalyticsStoreProfile : Profile
{
    public TrafficDataAnalyticsStoreProfile()
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