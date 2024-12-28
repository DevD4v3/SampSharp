using SampSharp.Core.Logging;

namespace SampSharp.Entities.SAMP.Commands
{
    /// <summary>
    /// Represents a middleware which lets unhandled OnPlayerCommandText events be processed by the <see cref="IPlayerCommandService"/>.
    /// </summary>
    public class PlayerCommandProcessingMiddleware
    {
        private readonly EventDelegate _next;
        private readonly IEntityManager _entityManager;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCommandProcessingMiddleware" /> class.
        /// </summary>
        /// <param name="next">The next middleware handler.</param>
        public PlayerCommandProcessingMiddleware(EventDelegate next, IEntityManager entityManager)
        {
            _next = next;
            _entityManager = entityManager;
        }
        
        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        public object Invoke(EventContext context, IPlayerCommandService commandService)
        {
            var result = _next(context);

            if (EventHelper.IsSuccessResponse(result))
                return result;

            if (context.Arguments[0] is EntityId player &&
                context.Arguments[1] is string text)
            {
                bool invokeResult = commandService.Invoke(context.EventServices, player, text);
                if (!invokeResult)
                {
                    _entityManager.GetComponent<Player>(player)
                        ?.SendClientMessage(Color.Red, "Command not found. To see all available commands, use /cmds");
                    // Returning true informs the server that the command has been processed.
                    return true;
                }
                return invokeResult;
            }

            CoreLog.Log(CoreLogLevel.Error, "Invalid command middleware input argument types!");
            return null;

        }
    }
}