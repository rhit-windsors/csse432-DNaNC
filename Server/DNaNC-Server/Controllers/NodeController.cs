using System.Net;
using DNaNC_Server.Models;
using DNaNC_Server.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DNaNC_Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NodeController
{
    
    public NodeController()
    {
    }
    
    [HttpGet]
    [Route("register")]
    public IResult RegisterNode(string host, int port)
    {
        var node = new Node
        {
            Host = host,
            Port = port
        };

        return null;
    }
    
    [HttpGet]
    [Route("unregister")]
    public IResult UnregisterNode(string host, int port)
    {
        var node = new Node
        {
            Host = host,
            Port = port
        };

        return null;
    }
}