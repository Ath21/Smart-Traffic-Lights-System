using AutoMapper;
using DetectionData.TimeSeriesObjects; // or wherever your CyclistDetection entity is
using CyclistDetectionStore.Models;

namespace CyclistDetectionStore
{
    public class CyclistDetectionStoreProfile : Profile
    {
        public CyclistDetectionStoreProfile()
        {
            // Map CreateDto to entity
            CreateMap<CyclistDetectionCreateDto, CyclistDetection>();

            // Map entity to ReadDto
            CreateMap<CyclistDetection, CyclistDetectionReadDto>();

            // Map entity to ResponseDto
            CreateMap<CyclistDetection, CyclistDetectionResponseDto>();
        }
    }
}
