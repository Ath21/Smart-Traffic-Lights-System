using System;
using AutoMapper;
using DetectionData.Collection.Detection;
using DetectionStore.Models.Dtos;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore;

public class DetectionStoreProfile : Profile
{
    public DetectionStoreProfile()
    {
        // Entities ↔ DTO
        CreateMap<EmergencyVehicleDetection, EmergencyVehicleDto>().ReverseMap();
        CreateMap<PublicTransportDetection, PublicTransportDto>().ReverseMap();
        CreateMap<IncidentDetection, IncidentDto>().ReverseMap();

        // DTO ↔ Responses
        CreateMap<EmergencyVehicleDto, EmergencyVehicleResponse>();
        CreateMap<PublicTransportDto, PublicTransportResponse>();
        CreateMap<IncidentDto, IncidentResponse>();
        CreateMap<DetectionSnapshotDto, DetectionSnapshotResponse>();
        CreateMap<DetectionHistoryDto, DetectionHistoryResponse>();

        // Requests ↔ DTOs
        CreateMap<RecordEmergencyVehicleRequest, EmergencyVehicleDto>();
        CreateMap<RecordPublicTransportRequest, PublicTransportDto>();
        CreateMap<RecordIncidentRequest, IncidentDto>();
    }
}