using SAIMRS.Application.Common.Interfaces;

namespace SAIMRS.Infrastructure.Services;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
