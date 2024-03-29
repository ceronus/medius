﻿using Microsoft.Extensions.DependencyInjection;

namespace Medius
{
    public static class MediusDependencyInjectionExtension
    {
        public static IServiceCollection AddMedius(this IServiceCollection services) => Medius.CreateInstance(services);
    }
}