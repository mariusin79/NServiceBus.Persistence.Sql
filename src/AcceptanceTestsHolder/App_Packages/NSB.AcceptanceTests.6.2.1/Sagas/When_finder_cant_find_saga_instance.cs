﻿namespace NServiceBus.AcceptanceTests.Sagas
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using Extensibility;
    using NServiceBus;
    using NServiceBus.Sagas;
    using NUnit.Framework;
    using Persistence;
    using Persistence.Sql;

    [TestFixture]
    public class When_finder_cant_find_saga_instance : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_start_new_saga()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SagaEndpoint>(b => b.When(session => session.SendLocal(new StartSagaMessage())))
                .Done(c => c.SagaStarted)
                .Run();

            Assert.True(context.FinderUsed);
            Assert.True(context.SagaStarted);
        }

        public class Context : ScenarioContext
        {
            public bool FinderUsed { get; set; }
            public bool SagaStarted { get; set; }
        }

        public class SagaEndpoint : EndpointConfigurationBuilder
        {
            public SagaEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            class CustomFinder : IFindSagas<TestSaga06.SagaData06>.Using<StartSagaMessage>
            {
                public Context Context { get; set; }

                public Task<TestSaga06.SagaData06> FindBy(StartSagaMessage message, SynchronizedStorageSession storageSession, ReadOnlyContextBag context)
                {
                    Context.FinderUsed = true;
                    return Task.FromResult(default(TestSaga06.SagaData06));
                }
            }

            public class TestSaga06 : SqlSaga<TestSaga06.SagaData06>, IAmStartedByMessages<StartSagaMessage>
            {

                protected override string CorrelationPropertyName => null;

                public Context Context { get; set; }

                public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
                {
                    Context.SagaStarted = true;
                    return Task.FromResult(0);
                }

                protected override void ConfigureMapping(IMessagePropertyMapper mapper)
                {
                    // not required because of CustomFinder
                }

                public class SagaData06 : ContainSagaData
                {
                }
            }
        }

        public class StartSagaMessage : IMessage
        {
        }
    }
}