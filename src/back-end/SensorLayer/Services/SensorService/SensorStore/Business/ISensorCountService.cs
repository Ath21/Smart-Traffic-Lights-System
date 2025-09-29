using System;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

namespace SensorStore.Business;

public interface ISensorCountService
{
    Task<SensorResponse> GetSensorDataAsync();
    Task ReportSensorDataAsync(SensorReportRequest dto);
}
