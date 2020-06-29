using Android.Graphics;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using MCServerStatus;
using MCServerStatus.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Minecraft_Server_Status {
    internal class Pinger {

        private string Address { get; }

        private short Port { get; }

        private readonly View _view;

        private MinecraftPinger _pinger;

        private Status _serverStatus;

        private readonly Stopwatch _latency = new Stopwatch();

        /// <summary>
        /// Constructs a new Pinger object
        /// </summary>
        /// <param name="address">Server address to ping</param>
        /// <param name="port">Port to use</param>
        /// <param name="view">The view containing the controls</param>
        public Pinger(string address, short port, ref View view) {
            Address = address;
            Port = port;
            _view = view;
        }

        /// <summary>
        /// Runs the server pinger
        /// </summary>
        /// <returns>True if the server is reachable</returns>
        public bool Ping() {
            // todo: don't block the UI thread while it's pinging
            var pingButton = _view.FindViewById<Button>(Resource.Id.PingButton);
            var motdBox = _view.FindViewById<WebView>(Resource.Id.ServerMotd);
            pingButton.Enabled = false;

            _pinger = new MinecraftPinger(Address, Port);
            var status = Task.Factory.StartNew(Loop).Result.Result;

            if (!status.Equals("Success")) {
                var error = @"<html><body><strong>Error: </strong>"
                            + status + "</body></html>";
                motdBox.LoadData(error, "text/html; charset=utf-8", "UTF-8");
                pingButton.Enabled = true;
                return false;
            }

            // convert the favicon from a Base64 PNG to an Image object
            // the first 22 characters are cut off as they're not a part of the image
            try {
                var icon = _view.FindViewById<ImageView>(Resource.Id.ServerIcon);
                var data = Convert.FromBase64String(_serverStatus.Favicon[22..]);
                using var stream = new MemoryStream(data, 0, data.Length);
                icon.SetImageBitmap(BitmapFactory.DecodeStream(stream));
            } catch (Exception) { /* invalid image; leave empty */ }

            string html;
            try {
                html = MotdParser.ParseMotd(_serverStatus.Description.Text);
                _view.FindViewById<EditText>(Resource.Id.ServerMaxPlayers).Text =
                    _serverStatus.Players.Max.ToString();
                _view.FindViewById<EditText>(Resource.Id.ServerOnlinePlayers).Text =
                    _serverStatus.Players.Online.ToString();
                _view.FindViewById<EditText>(Resource.Id.ServerVersion).Text =
                    _serverStatus.Version.Name;
            } catch (Exception) {
                html = @"<html><body><i>No description provided</i></body></html>";
            }
            motdBox.LoadData(html, "text/html; charset=utf-8", "UTF-8");

            _view.FindViewById<Button>(Resource.Id.PingButton).Enabled = true;

            // sets the ping time label (the '\n' is a hack to get it to align properly)
            _view.FindViewById<TextView>(Resource.Id.PingTimeLabel)
                .Text = "\n" + _latency.ElapsedMilliseconds + " ms";
            pingButton.Enabled = true;
            return true;
        }

        /// <summary>
        /// Attempt to connect to the server
        /// </summary>
        /// <returns>A string that will either be Resources.Success or
        /// the error message</returns>
        private async Task<string> Loop() {
            while (true)
                try {
                    _latency.Start();
                    _serverStatus = await _pinger.RequestAsync();
                    _latency.Stop();
                    return "Success";
                } catch (Exception ex) {
                    return ex.Message;
                }
        }

    }
}