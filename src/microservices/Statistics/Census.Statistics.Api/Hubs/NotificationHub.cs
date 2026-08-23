using Census.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Census.Statistics.Api.Hubs;

[Authorize(Policy = CensusPolicies.CanViewDashboard)]
public class NotificationHub : Hub
{
}
