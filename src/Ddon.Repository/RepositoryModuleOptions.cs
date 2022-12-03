using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repository
{
    public class RepositoryModuleOptions
    {
        public static void DbContextOptionsBuilderInit(Action<DbContextOptionsBuilder> optionsBuilder)
        {
            Option.Instance.Value.OptionsBuilder = optionsBuilder;
        }

        public static Action<DbContextOptionsBuilder>? OptionsBuilder => Option.Instance.Value.OptionsBuilder;

        private class Option
        {
            public static readonly Lazy<Option> Instance = new(() => new Option());

            public Action<DbContextOptionsBuilder>? OptionsBuilder { get; set; }
        }
    }
}
