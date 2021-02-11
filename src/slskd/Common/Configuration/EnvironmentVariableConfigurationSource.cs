﻿// <copyright file="EnvironmentVariableConfigurationSource.cs" company="slskd Team">
//     Copyright (c) slskd Team. All rights reserved.
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU Affero General Public License as published
//     by the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU Affero General Public License for more details.
//
//     You should have received a copy of the GNU Affero General Public License
//     along with this program.  If not, see https://www.gnu.org/licenses/.
// </copyright>

namespace slskd.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    ///     Extension methods for adding <see cref="EnvironmentVariableConfigurationProvider"/>.
    /// </summary>
    public static class EnvironmentVariableConfigurationExtensions
    {
        /// <summary>
        ///     Adds an environment variable configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to which to add.</param>
        /// <param name="map">A list of environment variable mappings.</param>
        /// <param name="prefix">A prefix to prepend to all variable names.</param>
        /// <returns>The updated <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddEnvironmentVariables(this IConfigurationBuilder builder, IEnumerable<Option> map, string prefix = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddEnvironmentVariables(s =>
            {
                s.Map = map ?? Enumerable.Empty<Option>();
                s.Prefix = prefix ?? string.Empty;
            });
        }

        /// <summary>
        ///     Adds an enviroment variable configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to which to add.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The updated <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddEnvironmentVariables(this IConfigurationBuilder builder, Action<EnvironmentVariableConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }

    /// <summary>
    ///     An enviroment variable <see cref="ConfigurationProvider"/>.
    /// </summary>
    public class EnvironmentVariableConfigurationProvider : ConfigurationProvider
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnvironmentVariableConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public EnvironmentVariableConfigurationProvider(EnvironmentVariableConfigurationSource source)
        {
            Map = source.Map;
            Prefix = source.Prefix;
        }

        private IEnumerable<Option> Map { get; set; }
        private string Prefix { get; set; }

        /// <summary>
        ///     Loads environment variables and maps them to the specified keys.
        /// </summary>
        public override void Load()
        {
            foreach (var item in Map)
            {
                if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.EnvironmentVariable))
                {
                    continue;
                }

                var value = Environment.GetEnvironmentVariable(Prefix + item.EnvironmentVariable);

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (item.Type == typeof(bool))
                {
                    value = value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ? value : "false";
                }

                Data[item.Key] = value;
            }
        }
    }

    /// <summary>
    ///     Represents environment variables as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class EnvironmentVariableConfigurationSource : IConfigurationSource
    {
        /// <summary>
        ///     Gets or sets a list of enviroment variable mappings.
        /// </summary>
        public IEnumerable<Option> Map { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether configuration keys should be normalized (_, - removed, changed to lowercase).
        /// </summary>
        public bool NormalizeKey { get; set; }

        /// <summary>
        ///     Gets or sets a prefix to prepend to all variable names.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        ///     Builds the <see cref="EnvironmentVariableConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="EnvironmentVariableConfigurationProvider"/>.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EnvironmentVariableConfigurationProvider(this);
        }
    }
}