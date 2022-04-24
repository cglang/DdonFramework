namespace Ddon.Job
{
    public interface IDdonJob
    {
        Task Add(Plan plan);

        Task Remove(Guid id);

        Task Update(Plan plan);

        Task<IEnumerable<Plan>> All();
    }
}
