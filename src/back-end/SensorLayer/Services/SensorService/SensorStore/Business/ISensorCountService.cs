using System;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

namespace SensorStore.Business;

public interface ISensorCountService
{
    Task<SensorResponse> GetSensorDataAsync(int intersectionId);
    Task ReportSensorDataAsync(SensorReportRequest dto);
}
