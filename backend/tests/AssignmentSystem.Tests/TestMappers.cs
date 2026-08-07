using AssignmentSystem.Application.Common.Mappings;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace AssignmentSystem.Tests;

public static class TestMappers
{
    public static IMapper CreateMapper()
        => new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
}
