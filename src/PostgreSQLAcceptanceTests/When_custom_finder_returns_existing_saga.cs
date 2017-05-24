using System;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using NServiceBus;
using NServiceBus.AcceptanceTesting;
using NServiceBus.AcceptanceTests;
using NServiceBus.AcceptanceTests.EndpointTemplates;
using NServiceBus.Extensibility;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Sql;
using NServiceBus.Sagas;
using NUnit.Framework;

[TestFixture]
public class When_custom_finder_returns_existing_saga : NServiceBusAcceptanceTest
{
    // this test triggers DTC which means that the max_prepared_statements flag on the postgresql database config must be bigger than 0 (disabled by default)

    [Test]
    public async Task Should_use_existing_saga()
    {
        var context = await Scenario.Define<Context>()
            .WithEndpoint<SagaEndpoint>(b => b
                .When(session =>
                {
                    var startSagaMessage = new StartSagaMessage
                    {
                        Property = "Test"
                    };
                    return session.SendLocal(startSagaMessage);
                }))
            .Done(c => c.HandledOtherMessage)
            .Run()
            .ConfigureAwait(false);

        Assert.True(context.FinderUsed);
    }

    public class Context : ScenarioContext
    {
        public bool FinderUsed { get; set; }
        public bool HandledOtherMessage { get; set; }
    }

    public class SagaEndpoint : EndpointConfigurationBuilder
    {
        public SagaEndpoint()
        {
            EndpointSetup<DefaultServer>();
        }

        public class CustomFinder : IFindSagas<TestSaga.SagaData>.Using<SomeOtherMessage>
        {
            // ReSharper disable once MemberCanBePrivate.Global
            public Context Context { get; set; }

            public Task<TestSaga.SagaData> FindBy(SomeOtherMessage message, SynchronizedStorageSession session, ReadOnlyContextBag context)
            {
                Context.FinderUsed = true;

                return session.GetSagaData<TestSaga.SagaData>(
                    context: context,
                    whereClause: "Data @> @propertyValue",
                    appendParameters: (builder, append) =>
                    {
                        var parameter = builder() as NpgsqlParameter;
                        parameter.ParameterName = "propertyValue";
                        parameter.Value = "{ \"Property\" : \"Test\" }";
                        parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
                        append(parameter);
                    });
            }
        }

        public class TestSaga : SqlSaga<TestSaga.SagaData>,
            IAmStartedByMessages<StartSagaMessage>,
            IHandleMessages<SomeOtherMessage>
        {
            public Context TestContext { get; set; }

            public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
            {
                var otherMessage = new SomeOtherMessage
                {
                    SagaId = Data.Id
                };
                return context.SendLocal(otherMessage);
            }

            public Task Handle(SomeOtherMessage message, IMessageHandlerContext context)
            {
                TestContext.HandledOtherMessage = true;
                return Task.FromResult(0);
            }

            protected override string CorrelationPropertyName => nameof(SagaData.Property);

            protected override void ConfigureMapping(IMessagePropertyMapper mapper)
            {
                mapper.ConfigureMapping<StartSagaMessage>(saga => saga.Property);
            }

            public class SagaData : ContainSagaData
            {
                public string Property { get; set; }
            }
        }
    }

    public class StartSagaMessage : IMessage
    {
        public string Property { get; set; }
    }

    public class SomeOtherMessage : IMessage
    {
        public Guid SagaId { get; set; }
    }
}