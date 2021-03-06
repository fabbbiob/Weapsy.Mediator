﻿using System.Threading.Tasks;
using Weapsy.Mediator.Domain;

namespace Weapsy.Mediator.Commands
{
    /// <summary>
    /// ICommandSender
    /// </summary>
    public interface ICommandSenderAsync
    {
        /// <summary>
        /// Asynchronously sends the specified command.
        /// The command handler must implement ICommandHandlerAsync&lt;TCommand&gt;.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="command">The command.</param>
        Task SendAsync<TCommand>(TCommand command)
            where TCommand : ICommand;

        /// <summary>
        /// Asynchronously sends the command the and publish the events returned by the command handler.
        /// The command handler must implement ICommandHandlerWithEventsAsync&lt;TCommand&gt;.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="command">The command.</param>
        Task SendAndPublishAsync<TCommand>(TCommand command)
            where TCommand : ICommand;

        /// <summary>
        /// Asynchronously sends the command and the events returned by the handler will be published and saved to the event store.
        /// The command handler must implement ICommandHandlerWithEventsAsync&lt;TCommand, TAggregate&gt;.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        Task SendAndPublishAsync<TCommand, TAggregate>(TCommand command)
            where TCommand : IDomainCommand
            where TAggregate : IAggregateRoot;
    }
}
