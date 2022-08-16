namespace Ddon.Domain.Specifications.Builder
{
    public interface IIncludableSpecificationBuilder<T, out TProperty> : ISpecificationBuilder<T>
    {
        IIncludeAggregator Aggregator { get; }
    }
}