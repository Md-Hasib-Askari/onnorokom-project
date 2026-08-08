namespace AssignmentSystem.Tests;

public class MappingTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        TestMappers.CreateMapper().ConfigurationProvider.AssertConfigurationIsValid();
    }
}
