using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using DNaNC_Client.ApplicationState;

namespace DNaNC_Client
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnRegisterClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(HostEntry.Text) || string.IsNullOrEmpty(PortEntry.Text))
            {
                StatusLabel.Text = "Status: Please enter a host and port";
                return;
            }
            
            //Prepare to tell the server that we are a node
            var ipv4 = GetExternalIP();
            var random = new Random();
            var port = random.Next(3320, 65533);
            //Http get request to the server to register us as a node
            var client = new HttpClient();
            try
            {
                var response = client.GetAsync($"http://localhost:5216/api/node/register?host={ipv4}&port={port}")
                    .Result;
                StatusLabel.Text = response.IsSuccessStatusCode ? "Status: Registered as a node" : "Status: Failed to register as a node";
            }
            catch
            {
                StatusLabel.Text = "Status: Failed to register as a node";
            }
        }

        private void OnUnregisterClicked(object sender, EventArgs e)
        {
            
        }
        
        private string GetExternalIP()
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString("https://api.ipify.org/");
            }
        }
    }

}
