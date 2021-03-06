namespace NServiceBus.AcceptanceTests.Routing.AutomaticSubscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NServiceBus.Pipeline;
    using NUnit.Framework;
    using Persistence.Sql;

    [TestFixture]
    [Explicit]
    public class When_starting_an_endpoint_with_a_saga_autosubscribe_disabled : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_not_autoSubscribe_messages_handled_by_sagas_if_asked_to()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Subscriber>(g => g.CustomConfig(c => c.AutoSubscribe().DoNotAutoSubscribeSagas()))
                .Done(c => c.EndpointsStarted)
                .Run();

            Assert.False(context.EventsSubscribedTo.Any(), "Events only handled by sagas should not be auto subscribed");
        }

        class Context : ScenarioContext
        {
            public Context()
            {
                EventsSubscribedTo = new List<Type>();
            }

            public List<Type> EventsSubscribedTo { get; }
        }

        class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                EndpointSetup<DefaultServer>(c => c.Pipeline.Register("SubscriptionSpy", new SubscriptionSpy((Context) ScenarioContext), "Spies on subscriptions made"),
                    metadata =>
                    {
                        metadata.RegisterPublisherFor<MyEventWithParent>(typeof(Subscriber));
                        metadata.RegisterPublisherFor<MyEvent>(typeof(Subscriber));
                    });
            }

            class SubscriptionSpy : IBehavior<ISubscribeContext, ISubscribeContext>
            {
                public SubscriptionSpy(Context testContext)
                {
                    this.testContext = testContext;
                }

                public async Task Invoke(ISubscribeContext context, Func<ISubscribeContext, Task> next)
                {
                    await next(context).ConfigureAwait(false);

                    testContext.EventsSubscribedTo.Add(context.EventType);
                }

                Context testContext;
            }

            public class NotAutoSubscribedSaga : SqlSaga<NotAutoSubscribedSaga.NotAutoSubscribedSagaSagaData>, IAmStartedByMessages<MyEvent>
            {
                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }

                public class NotAutoSubscribedSagaSagaData : ContainSagaData
                {
                    public virtual string SomeId { get; set; }
                }

                protected override string CorrelationPropertyName => nameof(NotAutoSubscribedSagaSagaData.SomeId);
                protected override void ConfigureMapping(IMessagePropertyMapper mapper)
                {
                    mapper.ConfigureMapping<MyEvent>(msg => msg.SomeId);
                }
            }

            public class NotAutoSubsubscribedSagaThatReactsOnASuperClassEvent : SqlSaga<NotAutoSubsubscribedSagaThatReactsOnASuperClassEvent.NotAutosubscribeSuperClassEventSagaData>,
                IAmStartedByMessages<MyEventBase>
            {
                public Task Handle(MyEventBase message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }

                public class NotAutosubscribeSuperClassEventSagaData : ContainSagaData
                {
                    public virtual string SomeId { get; set; }
                }

                protected override string CorrelationPropertyName => nameof(NotAutosubscribeSuperClassEventSagaData.SomeId);
                protected override void ConfigureMapping(IMessagePropertyMapper mapper)
                {
                    mapper.ConfigureMapping<MyEventBase>(saga => saga.SomeId);
                }
            }
        }

        public class MyEventBase : IEvent
        {
            public string SomeId { get; set; }
        }

        public class MyEventWithParent : MyEventBase
        {
        }

        public class MyMessage : IMessage
        {
        }

        public class MyEvent : IEvent
        {
            public string SomeId { get; set; }
        }
    }
}