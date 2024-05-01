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
    private readonly NodeManagerService _nodeManagerService;
    
    public NodeController(NodeManagerService nodeManagerService)
    {
        _nodeManagerService = nodeManagerService;
    }
    
    public IResult RegisterNode(string host, int port)
    {
        var node = new Node
        {
            Host = host,
            Port = port
        };

        return _nodeManagerService.RegisterNode(node) ? Results.Ok() : Results.BadRequest();
    }
    
    public IResult UnregisterNode(string host, int port)
    {
        var node = new Node
        {
            Host = host,
            Port = port
        };

        return _nodeManagerService.UnregisterNode(node) ? Results.Ok() : Results.BadRequest();
    }
}