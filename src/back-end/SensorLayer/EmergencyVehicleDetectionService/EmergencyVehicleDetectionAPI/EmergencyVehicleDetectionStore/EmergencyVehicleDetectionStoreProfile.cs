using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using EmergencyVehicleDetectionStore.Models;
using EmergencyVehicleDetectionStore.Models.Requests;
using EmergencyVehicleDetectionStore.Models.Responses;
using SensorMessages;

namespace EmergencyVehicleDetectionStore;

public class EmergencyVehicleDetectionStoreProfile : Profile
{
    public EmergencyVehicleDetectionStoreProfile()
    {
        CreateMap<EmergencyVehicleDetectionRequest, EmergencyVehicleDetection>()
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow));

        CreateMap<EmergencyVehicleDetection, EmergencyVehicleDetectionResponse>();
        CreateMap<EmergencyVehicleDetection, EmergencyVehicleMessage>();
        CreateMap<EmergencyVehicleMessage, EmergencyVehicleDetection>();
    }
}