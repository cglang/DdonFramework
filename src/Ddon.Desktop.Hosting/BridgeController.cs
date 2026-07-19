using Ddon.Desktop.Bridge;
using Ddon.Desktop.Protocol;
using Microsoft.AspNetCore.Mvc;

namespace Ddon.Desktop.Hosting;

[ApiController]
[Route("api/bridge")]
public class BridgeController : ControllerBase
{
    private readonly IBridgeDispatcher _dispatcher;

    public BridgeController(IBridgeDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("invoke")]
    public async Task<IActionResult> Invoke([FromBody] BridgeRequest request)
    {
        try
        {
            var result = await _dispatcher.DispatchAsync(request.Method, request.Payload);
            return Ok(new BridgeResponse
            {
                Id = request.Id,
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            return Ok(new BridgeResponse
            {
                Id = request.Id,
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpPost("event")]
    public IActionResult Publish([FromBody] BridgeEvent bridgeEvent)
    {
        return Ok();
    }
}
