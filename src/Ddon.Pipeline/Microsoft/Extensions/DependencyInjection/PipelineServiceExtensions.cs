using Ddon.Pipeline;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PipelineServiceExtensions
    {
        public static void AddBasePipeline(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IPipelineInstanceProvider<>), typeof(ContainerPipelineInstanceProvider<>));
            services.AddTransient(typeof(IPipelineRegistrar<>), typeof(PipelineRegistrar<>));
            services.AddTransient(typeof(IGeneralCustomPipeline<>), typeof(GeneralCustomPipeline<>));
        }
    }
}
