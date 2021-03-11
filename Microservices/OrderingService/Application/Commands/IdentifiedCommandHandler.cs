using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderingService.Infrastructure.Idempotency;

namespace OrderingService.Application.Commands
{
    /// <summary>
    /// Provides a base implementation for handling duplicate request and ensuring idempotent updates, in the cases where
    /// a requestid sent by client is used to detect duplicate requests.
    /// </summary>
    /// <typeparam name="TRequest">Type of the command handler that performs the operation if request is not duplicated</typeparam>
    /// <typeparam name="TResponse">Return value of the inner command handler</typeparam>
    public class IdentifiedCommandHandler<TRequest, TResponse> : IRequestHandler<IdentifiedCommand<TRequest, TResponse>, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IMediator _mediator;
        private readonly IRequestManager _requestManager;
        private readonly ILogger _logger;

        public IdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager, ILoggerFactory loggerFactory)
        {
            _mediator = mediator;
            _requestManager = requestManager;
            _logger = loggerFactory.CreateLogger(nameof(IdentifiedCommandHandler<TRequest,TResponse>));
        }

        /// <summary>
        /// Creates the result value to return if a previous request was found
        /// </summary>
        /// <returns></returns>
        protected virtual TResponse CreateResultForDuplicateRequest()
        {
            return default(TResponse);
        }

        /// <summary>
        /// This method handles the command. It just ensures that no other request exists with the same ID, and if this is the case
        /// just enqueues the original inner command.
        /// </summary>
        /// <param name="message">IdentifiedCommand which contains both original command & request ID</param>
        /// <returns>Return value of inner command or default value if request same ID was found</returns>
        public async Task<TResponse> Handle(IdentifiedCommand<TRequest, TResponse> message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Ensures request with the same ID ({message.Id}) is not already processed.");
            var alreadyExists = await _requestManager.ExistAsync(message.Id);
            if (alreadyExists) 
            {
                return CreateResultForDuplicateRequest();
            }

            await _requestManager.CreateRequestForCommandAsync<TRequest>(message.Id);
            try
            {
                    // Send the embedded business command to mediator so it runs its related CommandHandler
                _logger.LogInformation($"Sending command: ({message.Command.GetType().Name})");
                return await _mediator.Send(message.Command, cancellationToken);
            }
            catch
            {
                return default(TResponse);
            }
        }
    }
}