﻿// <copyright file="OpenTelemetryBuilderExtensions.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using OpenTelemetry.Instrumentation.AspNetCore;

namespace OpenTelemetry.Trace
{
    /// <summary>
    /// Extension methods to simplify registering of asp.net core request instrumentation.
    /// </summary>
    public static class OpenTelemetryBuilderExtensions
    {
        /// <summary>
        /// Enables the incoming requests automatic data collection for Asp.Net Core.
        /// </summary>
        /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
        /// <param name="configureAspNetCoreInstrumentationOptions">ASP.NET Core Request configuration options.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder AddRequestInstrumentation(
            this TracerProviderBuilder builder,
            Action<AspNetCoreInstrumentationOptions> configureAspNetCoreInstrumentationOptions = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var aspnetCoreOptions = new AspNetCoreInstrumentationOptions();
            configureAspNetCoreInstrumentationOptions?.Invoke(aspnetCoreOptions);
            builder.AddInstrumentation((activitySource) => new AspNetCoreInstrumentation(activitySource, aspnetCoreOptions));

            return builder;
        }
    }
}
