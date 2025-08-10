using AutoMapper;
using DetectionData.TimeSeriesObjects;
using PedestrianDetectionStore.Models;

namespace PedestrianDetectionStore
{
    public class PedestrianDetectionStoreProfile : Profile
    {
        public PedestrianDetectionStoreProfile()
        {
            // Create → Entity
            CreateMap<PedestrianDetectionCreateDto, PedestrianDetection>();

            // Entity → Read
            CreateMap<PedestrianDetection, PedestrianDetectionReadDto>();

            // Entity → Response
            CreateMap<PedestrianDetection, PedestrianDetectionResponseDto>();
        }
    }
}
