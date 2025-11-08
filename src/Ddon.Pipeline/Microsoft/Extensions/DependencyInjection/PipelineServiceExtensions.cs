using Ddon.Pipeline;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PipelineServiceExtensions
    {
        public static void AddBasePipeline(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IPipelineInstanceProvider<>), typeof(ContainerPipelineInstanceProvider<>));
            services.AddSingleton(typeof(IPipelineRegistrar<>), typeof(PipelineRegistrar<>));
            services.AddSingleton(typeof(IGeneralCustomPipeline<>), typeof(GeneralCustomPipeline<>));
        }
    }
}
