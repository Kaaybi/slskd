﻿// <copyright file="CommandLineConfigurationSource.cs" company="slskd Team">
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
    using Utility.CommandLine;

    /// <summary>
    ///     Extension methods for adding <see cref="CommandLineConfigurationProvider"/>.
    /// </summary>
    public static class CommandLineConfigurationExtensions
    {
        /// <summary>
        ///     Adds a command line argument configuration soruce to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to which to add.</param>
        /// <param name="map">A list of command line argument mappings.</param>
        /// <param name="commandLine">The command line string from which to parse arguments.</param>
        /// <returns>The updated <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, IEnumerable<Option> map, string commandLine = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddCommandLine(s =>
            {
                s.Map = map;
                s.CommandLine = commandLine ?? Environment.CommandLine;
            });
        }

        /// <summary>
        ///     Adds a command line argument configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to which to add.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The updated <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, Action<CommandLineConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }

    /// <summary>
    ///     A command line argument <see cref="ConfigurationProvider"/>.
    /// </summary>
    public class CommandLineConfigurationProvider : ConfigurationProvider
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandLineConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public CommandLineConfigurationProvider(CommandLineConfigurationSource source)
        {
            Map = source.Map;
            CommandLine = source.CommandLine;
        }

        private string CommandLine { get; set; }
        private IEnumerable<Option> Map { get; set; }

        /// <summary>
        ///     Parses command line arguments from the specified string and maps them to the specified keys.
        /// </summary>
        public override void Load()
        {
            var dictionary = Arguments.Parse(CommandLine).ArgumentDictionary;

            foreach (Option item in Map)
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }

                var arguments = new[] { item.ShortName.ToString(), item.LongName }.Where(i => !string.IsNullOrEmpty(i));

                foreach (var argument in arguments)
                {
                    if (dictionary.ContainsKey(argument))
                    {
                        var value = dictionary[argument].ToString();

                        if (item.Type == typeof(bool) && string.IsNullOrEmpty(value))
                        {
                            value = "true";
                        }

                        Data[item.Key] = value;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Represents command line arguments as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class CommandLineConfigurationSource : IConfigurationSource
    {
        /// <summary>
        ///     Gets or sets the command line string from which to parse arguments.
        /// </summary>
        public string CommandLine { get; set; }

        /// <summary>
        ///     Gets or sets a list of command line argument mappings.
        /// </summary>
        public IEnumerable<Option> Map { get; set; }

        /// <summary>
        ///     Builds the <see cref="CommandLineConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="CommandLineConfigurationProvider"/>.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new CommandLineConfigurationProvider(this);
    }
}