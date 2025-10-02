using System;

namespace DetectionData.Repositories;

public interface IRepository<T>
{
    Task InsertAsync(T entity);
    Task<List<T>> GetAllAsync();
    Task<T> GetLatestAsync(int intersectionId);
}