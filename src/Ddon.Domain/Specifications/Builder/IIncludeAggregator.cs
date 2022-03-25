namespace Ddon.Identity.Specifications.Builder
{
    public interface IIncludeAggregator
    {
        void AddNavigationPropertyName(string? navigationPropertyName);

        string IncludeString { get; }
    }
}