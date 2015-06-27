// Copyright 2014 Serilog Contributors
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

using System;
using Microsoft.ApplicationInsights.Extensibility;

namespace Serilog.Sinks.ApplicationInsights.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="TelemetryConfiguration"/> instances.
    /// </summary>
    public static class TelemetryConfigurationExtensions
    {
        /// <summary>
        /// Adds the context initializers to the provided <paramref name="configuration" />.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="contextInitializers">The context initializers.</param>
        public static void AddContextInitializers(this TelemetryConfiguration configuration, IContextInitializer[] contextInitializers)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            if (contextInitializers == null)
                return;

            // else
            foreach (var contextInitializer in contextInitializers)
            {
                configuration.ContextInitializers.Add(contextInitializer);
            }
        }
    }
}
