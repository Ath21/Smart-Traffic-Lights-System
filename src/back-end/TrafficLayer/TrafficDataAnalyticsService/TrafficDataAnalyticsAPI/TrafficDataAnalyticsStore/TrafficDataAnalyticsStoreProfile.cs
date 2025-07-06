using System;
using AutoMapper;
using TrafficDataAnalyticsData.Entities;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore;

public class TrafficDataAnalyticsStoreProfile : Profile
{
    public TrafficDataAnalyticsStoreProfile()
    {
        CreateMap<DailySummary, DailySummaryDto>().ReverseMap();
        CreateMap<CongestionAlert, CongestionAlertDto>().ReverseMap();
    }
}
