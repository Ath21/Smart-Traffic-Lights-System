using System;
using AutoMapper;
using MongoDB.Bson;
using TrafficDataAnalyticsData.Collections;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore;

public class TrafficDataAnalyticsStoreProfile : Profile
{
    public TrafficDataAnalyticsStoreProfile()
    {
        CreateMap<DailySummaryDto, DailySummary>()
            .ForMember(dest => dest.SummaryId, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.PeakHours,
                opt => opt.MapFrom(src => new BsonDocument(src.PeakHours)));
    }
}
