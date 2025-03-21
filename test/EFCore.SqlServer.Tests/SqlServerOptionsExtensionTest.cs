// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerOptionsExtensionTest
    {
        [ConditionalFact]
        public void Compiled_model_is_thread_safe()
        {
            var tasks = new Task[Environment.ProcessorCount];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    using (var ctx = new EmptyContext())
                    {
                        Assert.NotNull(ctx.Model.GetRelationalDependencies());
                    }
                });
            }

            Task.WaitAll(tasks);
        }

        private class EmptyContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseSqlServer().UseModel(EmptyContextModel.Instance);
                }
            }
        }

        [DbContext(typeof(EmptyContext))]
        partial class EmptyContextModel : RuntimeModel
        {
            static EmptyContextModel()
            {
                var model = new EmptyContextModel();
                model.Initialize();
                model.Customize();
                _instance = model;
            }

            private static readonly EmptyContextModel _instance;
            public static IModel Instance => _instance;

            partial void Initialize();

            partial void Customize();
        }

        [ConditionalFact]
        public void ApplyServices_adds_SQL_server_services()
        {
            var services = new ServiceCollection();

            new SqlServerOptionsExtension().ApplyServices(services);

            Assert.Contains(services, sd => sd.ServiceType == typeof(ISqlServerConnection));
        }

        private class ChangedRowNumberContext : DbContext
        {
            private static readonly IServiceProvider _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .BuildServiceProvider(validateScopes: true);

            private readonly bool _setInternalServiceProvider;

            public ChangedRowNumberContext(bool setInternalServiceProvider)
            {
                _setInternalServiceProvider = setInternalServiceProvider;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (_setInternalServiceProvider)
                {
                    optionsBuilder.UseInternalServiceProvider(_serviceProvider);
                }

                optionsBuilder.UseSqlServer("Database=Maltesers");
            }
        }
    }
}
