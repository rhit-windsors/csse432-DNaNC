using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using DNaNC_Client.Objects;
using DNaNC_Client.Services;

namespace DNaNC_Client
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        
        private void OnHostClicked(object sender, EventArgs e)
        {
            try
            {
                var random = new Random();
                var port = random.Next(3320, 65533);
                NodeManager.InitNetwork(port);
                StatusLabel.Text = "Status: Host created!";
                PortLabel.Text = "Port: " + port;
                //Start listening
                _ = Task.Run(NodeServer.Listen);
                //Hide text boxes and buttons
                HostEntry.IsVisible = false;
                PortEntry.IsVisible = false;
                RegisterBtn.IsVisible = false;
                HostBtn.IsVisible = false;
                
                //Show leave button
                LeaveBtn.IsVisible = true;
                ShareFileBtn.IsVisible = true;
                SharedFilesLabel.IsVisible = true;
                SearchEntry.IsVisible = true;
                SearchBtn.IsVisible = true;
            }
            catch
            {
                StatusLabel.Text = "Status: Could not get host!";
            }
        }

        private void OnRegisterClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(HostEntry.Text) || string.IsNullOrEmpty(PortEntry.Text))
                {
                    StatusLabel.Text = "Status: Please enter a host and port";
                    return;
                }
            
                //Prepare to tell the server that we are a node
                var random = new Random();
                var port = random.Next(3320, 65533);
            
                NodeManager.Join(HostEntry.Text, int.Parse(PortEntry.Text), port);
                //Start listening
                _ = Task.Run(NodeServer.Listen);
            
                StatusLabel.Text = "Status: Node joined the network!";
                PortLabel.Text = "Port: " + port; 
                
                //Hide text boxes and buttons
                HostEntry.IsVisible = false;
                PortEntry.IsVisible = false;
                RegisterBtn.IsVisible = false;
                HostBtn.IsVisible = false;
                
                //Show leave button
                LeaveBtn.IsVisible = true;
                ShareFileBtn.IsVisible = true;
                SharedFilesLabel.IsVisible = true;
                SearchEntry.IsVisible = true;
                SearchBtn.IsVisible = true;
            }
            catch
            {
                StatusLabel.Text = "Status: Node not in network!";
            }
            
        }
        
        private void OnLeaveClicked(object sender, EventArgs e)
        {
            try
            {
                NodeManager.Leave();
                StatusLabel.Text = "Status: Node left the network!";
                PortLabel.Text = "Port: N/A";
                
                //Show text boxes and buttons
                HostEntry.IsVisible = true;
                PortEntry.IsVisible = true;
                RegisterBtn.IsVisible = true;
                HostBtn.IsVisible = true;
                
                //Hide leave button
                LeaveBtn.IsVisible = false;
                ShareFileBtn.IsVisible = false;
                SharedFilesLabel.IsVisible = false;
                SearchEntry.IsVisible = false;
                SearchBtn.IsVisible = false;
                LocatedFilesLabel.IsVisible = false;
                IndexEntry.IsVisible = false;
                GetBtn.IsVisible = false;
            }
            catch
            {
                StatusLabel.Text = "Status: Node not in network!";
                //Show text boxes and buttons
                HostEntry.IsVisible = true;
                PortEntry.IsVisible = true;
                RegisterBtn.IsVisible = true;
                HostBtn.IsVisible = true;
                
                //Hide leave button
                LeaveBtn.IsVisible = false;
                ShareFileBtn.IsVisible = false;
                SharedFilesLabel.IsVisible = false;
                SearchEntry.IsVisible = false;
                SearchBtn.IsVisible = false;
                LocatedFilesLabel.IsVisible = false;
                IndexEntry.IsVisible = false;
                GetBtn.IsVisible = false;
            }
            
        }
        
        private async void OnShareClicked(object sender, EventArgs e)
        {
            PickOptions options = new PickOptions
            {
                PickerTitle = "Select files to share"
            };
            var files = await FilePicker.PickMultipleAsync(options);
            SharedFilesLabel.Text = "Shared Files:";
            foreach (var file in files)
            {
                Services.FileShare.ShareFile(file.FullPath);
                SharedFilesLabel.Text = SharedFilesLabel.Text + "\n" + file.FullPath;
            }
        }

        private void OnSearchClicked(object? sender, EventArgs e)
        {
            NodeManager.Query(SearchEntry.Text, NodeManager.LocalNode);
            LocatedFilesLabel.Text = "Located Files:";
            //Wait for the files to be located
            Thread.Sleep(5000);
            foreach (var file in Services.FileShare.LocatedFiles)
            {
                LocatedFilesLabel.Text = LocatedFilesLabel.Text + "\n" + "[" + Services.FileShare.LocatedFiles.IndexOf(file) + "]: " + file.FileName + " at " + file.FileLocation.Host + ":" + file.FileLocation.Port;
            }
            LocatedFilesLabel.IsVisible = true;
            IndexEntry.IsVisible = true;
            GetBtn.IsVisible = true;
        }

        private void OnGetClicked(object? sender, EventArgs e)
        {
            int index = int.Parse(IndexEntry.Text);
            NodeManager.RequestFile(index);
            StatusLabel.Text = "Status: File available in downloads!";
        }
    }

}
