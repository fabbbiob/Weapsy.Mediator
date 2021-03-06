﻿using System;
using System.Threading.Tasks;
using Weapsy.Mediator.Dependencies;
using Weapsy.Mediator.Domain;
using Weapsy.Mediator.Events;

namespace Weapsy.Mediator.Commands
{
    /// <inheritdoc />
    /// <summary>
    /// CommandSenderAsync
    /// </summary>
    /// <seealso cref="T:Weapsy.Mediator.Commands.ICommandSenderAsync" />
    public class CommandSenderAsync : ICommandSenderAsync
    {
        private readonly IResolver _resolver;
        private readonly IEventPublisherAsync _eventPublisher;
        private readonly IEventStore _eventStore;
        private readonly IEventFactory _eventFactory;

        public CommandSenderAsync(IResolver resolver,
            IEventPublisherAsync eventPublisher, 
            IEventStore eventStore, 
            IEventFactory eventFactory)
        {
            _resolver = resolver;
            _eventPublisher = eventPublisher;
            _eventStore = eventStore;
            _eventFactory = eventFactory;
        }

        public async Task SendAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var commandHandler = _resolver.Resolve<ICommandHandlerAsync<TCommand>>();

            if (commandHandler == null)
                throw new ApplicationException($"No handler of type CommandHandlerAsync<TCommand> found for command '{command.GetType().FullName}'");

            await commandHandler.HandleAsync(command);
        }

        public async Task SendAndPublishAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var commandHandler = _resolver.Resolve<ICommandHandlerWithEventsAsync<TCommand>>();

            if (commandHandler == null)
                throw new ApplicationException($"No handler of type ICommandHandlerWithEventsAsync<TCommand> found for command '{command.GetType().FullName}'");

            var events = await commandHandler.HandleAsync(command);

            foreach (var @event in events)
            {
                var concreteEvent = _eventFactory.CreateConcreteEvent(@event);
                await _eventPublisher.PublishAsync(concreteEvent);
            }
        }

        public async Task SendAndPublishAsync<TCommand, TAggregate>(TCommand command) where TCommand : IDomainCommand where TAggregate : IAggregateRoot
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var commandHandler = _resolver.Resolve<IDomainCommandHandlerAsync<TCommand>>();

            if (commandHandler == null)
                throw new ApplicationException($"No handler of type IDomainCommandHandlerAsync<TCommand> found for command '{command.GetType().FullName}'");

            var events = await commandHandler.HandleAsync(command);

            foreach (var @event in events)
            {
                var concreteEvent = _eventFactory.CreateConcreteEvent(@event);
                await _eventStore.SaveEventAsync<TAggregate>((IDomainEvent)concreteEvent);
                await _eventPublisher.PublishAsync(concreteEvent);
            }
        }
    }
}
