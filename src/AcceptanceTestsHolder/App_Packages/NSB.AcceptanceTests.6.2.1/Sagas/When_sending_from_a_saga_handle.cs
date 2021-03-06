﻿namespace NServiceBus.AcceptanceTests.Sagas
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using Features;
    using NUnit.Framework;
    using Persistence.Sql;

    public class When_sending_from_a_saga_handle : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_match_different_saga()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Endpoint>(b => b.When(session => session.SendLocal(new StartSaga1
                {
                    DataId = Guid.NewGuid()
                })))
                .Done(c => c.DidSaga2ReceiveMessage)
                .Run();

            Assert.True(context.DidSaga2ReceiveMessage);
        }

        public class Context : ScenarioContext
        {
            public bool DidSaga2ReceiveMessage { get; set; }
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>(config => config.EnableFeature<TimeoutManager>());
            }

            public class TwoSaga1Saga1 : SqlSaga<TwoSaga1Saga1Data>, IAmStartedByMessages<StartSaga1>, IHandleMessages<MessageSaga1WillHandle>
            {
                protected override string CorrelationPropertyName => nameof(TwoSaga1Saga1Data.DataId);

                public Task Handle(StartSaga1 message, IMessageHandlerContext context)
                {
                    Data.DataId = message.DataId;
                    return context.SendLocal(new MessageSaga1WillHandle
                    {
                        DataId = message.DataId
                    });
                }

                public async Task Handle(MessageSaga1WillHandle message, IMessageHandlerContext context)
                {
                    await context.SendLocal(new StartSaga2
                    {
                        DataId = message.DataId
                    });
                    MarkAsComplete();
                }

                protected override void ConfigureMapping(IMessagePropertyMapper mapper)
                {
                    mapper.ConfigureMapping<MessageSaga1WillHandle>(m => m.DataId);
                    mapper.ConfigureMapping<StartSaga1>(m => m.DataId);
                }
            }

            public class TwoSaga1Saga1Data : ContainSagaData
            {
                public virtual Guid DataId { get; set; }
            }

            public class TwoSaga1Saga2 : SqlSaga<TwoSaga1Saga2.TwoSaga1Saga2Data>, IAmStartedByMessages<StartSaga2>
            {
                protected override string CorrelationPropertyName => nameof(TwoSaga1Saga2Data.DataId);

                public Context Context { get; set; }

                public Task Handle(StartSaga2 message, IMessageHandlerContext context)
                {
                    Data.DataId = message.DataId;
                    Context.DidSaga2ReceiveMessage = true;

                    return Task.FromResult(0);
                }

                protected override void ConfigureMapping(IMessagePropertyMapper mapper)
                {
                    mapper.ConfigureMapping<StartSaga2>(m => m.DataId);
                }

                public class TwoSaga1Saga2Data : ContainSagaData
                {
                    public virtual Guid DataId { get; set; }
                }
            }
        }

        public class StartSaga1 : ICommand
        {
            public Guid DataId { get; set; }
        }

        public class StartSaga2 : ICommand
        {
            public Guid DataId { get; set; }
        }

        public class MessageSaga1WillHandle : IMessage
        {
            public Guid DataId { get; set; }
        }
    }
}