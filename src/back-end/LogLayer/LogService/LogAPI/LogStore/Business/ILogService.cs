using System;
using LogStore.Models;

namespace LogStore.Business;

public interface ILogService
{
    Task StoreLogAsync(LogDto logDto);
}
