using System;

namespace TrafficLightCoordinatorStore.Models;

public class LatLngDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public LatLngDto() { }
    public LatLngDto(double lat, double lng) { Lat = lat; Lng = lng; }
}
