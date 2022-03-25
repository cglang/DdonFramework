namespace Ddon.Identity.Specifications.Builder
{
    public interface IIncludableSpecificationBuilder<T, out TProperty> : ISpecificationBuilder<T>
    {
        IIncludeAggregator Aggregator { get; }
    }
}