using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting;
using NServiceBus.AcceptanceTests;
using NServiceBus.AcceptanceTests.EndpointTemplates;
using NServiceBus.Features;
using NServiceBus.Persistence.Sql;
using NUnit.Framework;

[TestFixture]
public class When_multiple_sagas : NServiceBusAcceptanceTest
{
    [Test]
    public Task Should_use_existing_saga()
    {
        return Scenario.Define<Context>()
            .WithEndpoint<SagaEndpoint>(b => b
                .When(session =>
                {
                    var startSagaMessage = new StartSagaMessage
                    {
                        Property = "Test"
                    };
                    return session.SendLocal(startSagaMessage);
                }))
            .Done(c => c.Saga1Processed && c.Saga2Processed)
            .Run();
    }

    public class Context : ScenarioContext
    {
        public bool Saga1Processed { get; set; }
        public bool Saga2Processed { get; set; }
    }

    public class SagaEndpoint : EndpointConfigurationBuilder
    {
        public SagaEndpoint()
        {
            EndpointSetup<DefaultServer>(b =>
            {
                b.EnableFeature<TimeoutManager>();
            });
        }

        public class TestSaga1 : SqlSaga<TestSaga1.SagaData1>,
            IAmStartedByMessages<StartSagaMessage>,
            IHandleTimeouts<Timeout>
        {
            public Context TestContext { get; set; }

            public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
            {
                RequestTimeout<Timeout>(context, TimeSpan.FromMilliseconds(100));
                return Task.FromResult(0);
            }

            public Task Timeout(Timeout state, IMessageHandlerContext context)
            {
                TestContext.Saga1Processed = true;
                return Task.FromResult(0);
            }

            protected override string CorrelationPropertyName => nameof(SagaData1.Property);

            protected override void ConfigureMapping(IMessagePropertyMapper mapper)
            {
                mapper.ConfigureMapping<StartSagaMessage>(saga => saga.Property);
            }

            public class SagaData1 : ContainSagaData
            {
                public string Property { get; set; }
            }

        }
        public class TestSaga2 : SqlSaga<TestSaga2.SagaData2>,
            IAmStartedByMessages<StartSagaMessage>,
            IHandleTimeouts<Timeout>
        {
            public Context TestContext { get; set; }

            public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
            {
                RequestTimeout<Timeout>(context, TimeSpan.FromMilliseconds(100));
                return Task.FromResult(0);
            }

            public Task Timeout(Timeout state, IMessageHandlerContext context)
            {
                TestContext.Saga2Processed = true;
                return Task.FromResult(0);
            }

            protected override string CorrelationPropertyName => nameof(SagaData2.Property);

            protected override void ConfigureMapping(IMessagePropertyMapper mapper)
            {
                mapper.ConfigureMapping<StartSagaMessage>(saga => saga.Property);
            }

            public class SagaData2 : ContainSagaData
            {
                public string Property { get; set; }
            }

        }
    }

    public class StartSagaMessage : IMessage
    {
        public string Property { get; set; }
    }

    public class Timeout
    {
    }
}