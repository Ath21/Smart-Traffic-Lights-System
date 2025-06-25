/*
 * NotificationStore.NotificationStoreProfile
 *
 * This file is part of the NotificationStore project, which is responsible for mapping
 * between NotificationDto and Notification models.
 * It uses AutoMapper to define the mapping configurations.
 * The NotificationStoreProfile class inherits from AutoMapper's Profile class and
 * defines the mappings in its constructor.
 * The mappings allow for easy conversion between the data transfer object (DTO) and the
 * domain model, facilitating data transfer and manipulation within the Notification Store service.
 */
using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Models;

namespace NotificationStore;

public class NotificationStoreProfile : Profile
{
    public NotificationStoreProfile()
    {
        CreateMap<NotificationDto, Notification>();
        CreateMap<Notification, NotificationDto>();
    }
}
