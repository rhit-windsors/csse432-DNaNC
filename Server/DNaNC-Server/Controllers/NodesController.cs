using DNaNC_Server.Models;
using DNaNC_Server.Objects;
using DNaNC_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace DNaNC_Server.Controllers;

public class NodesController : Controller
{
    public IActionResult Index()
    {
        //Get all of the nodes
        var nodes = GetNodes(NodeManager.LocalNode);
        
        //Pass the nodes to the view

        return View(nodes);
    }
    
    private Nodes GetNodes(Node node)
    {
        var nodes = new Nodes();
        nodes.NodeList.Add(NodeManager.LocalNode);
        var successor = NodeManager.GetSuccessor(node);
        if (successor.Id != NodeManager.LocalNode.Id)
        {
            nodes.NodeList.Add(successor);
            nodes.NodeList.AddRange(GetNodes(successor).NodeList);
        }
        
        //Make sure each node is distinct
        nodes.NodeList = nodes.NodeList.Distinct().ToList();
        
        return nodes;
    }
}