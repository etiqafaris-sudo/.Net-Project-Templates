using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Auth.Commands.Logout
{
    public sealed record LogoutCommand(int UserId) : IRequest<Result<bool>>;

    internal sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
    {
        private readonly ILogger<LogoutCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly ITemplateDbContext _context;

        public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IMediator mediator, ITemplateDbContext context)
        {
            _logger = logger;
            _mediator = mediator;
            _context = context;
        }

        public async ValueTask<Result<bool>> Handle(LogoutCommand request,
                                                    CancellationToken cancellationToken)
        {
            //do something on logout on server side 
            _logger.LogInformation("Handling logout command.");
            var user = await _context.GlUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            await _mediator.Publish(new UserLogoutEvent(request.UserId), cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
