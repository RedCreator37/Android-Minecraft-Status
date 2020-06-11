using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using System;
using Xamarin.Essentials;
using static Minecraft_Server_Status.Resource;
using Color = Android.Graphics.Color;
using NVItemListener = Android.Support.Design.Widget
    .NavigationView.IOnNavigationItemSelectedListener;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Minecraft_Server_Status {

    /// <summary>
    /// The main application activity
    /// </summary>
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar",
        MainLauncher = true)]
    // ReSharper disable once UnusedType.Global
    public class MainActivity : AppCompatActivity, NVItemListener {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Layout.activity_main);
            var toolbar = FindViewById<Toolbar>(Id.toolbar);
            SetSupportActionBar(toolbar);

            var drawer = FindViewById<DrawerLayout>(Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, drawer, toolbar,
                Resource.String.navigation_drawer_open,
                Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            // initialize the controls
            ResetControls();
            var v = FindViewById(Android.Resource.Id.Content);

            // add click listeners to the buttons
            FindViewById<Button>(Id.PingButton).Click += delegate {
                OnPingClicked(ref v);
            };

            var navigationView = FindViewById<NavigationView>(Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
        }

        /// <summary>
        /// Handles the Ping button click event
        /// </summary>
        /// <param name="v">The current view</param>
        private void OnPingClicked(ref View v) {
            var addressField = FindViewById<EditText>(Id.InputAddress);
            var portField = FindViewById<EditText>(Id.InputPort);
            if (addressField.Text.Trim().Equals("") || portField.Text.Trim().Equals("")) {
                Toast.MakeText(this, "Fill in both address and port fields",
                    ToastLength.Short).Show();
                return;
            }

            short port;
            try {   // make sure the port is valid
                port = short.Parse(portField.Text);
                if (port < 1) throw new OverflowException();
            } catch (OverflowException) {
                Toast.MakeText(this, "Invalid port number", ToastLength.Short).Show();
                return;
            }

            // in case the server doesn't have a server icon
            v.FindViewById<ImageView>(Id.ServerIcon)
                .SetImageResource(Drawable.ic_texture_black_48dp);

            var toolbar = FindViewById<Toolbar>(Id.toolbar);

            var online = new Pinger(addressField.Text, port, ref v).Ping();
            Window.SetStatusBarColor(online
                ? new Color(ContextCompat.GetColor(this,
                    Resource.Color.colorStatusOkDark))
                : new Color(ContextCompat.GetColor(this,
                    Resource.Color.colorStatusErrorDark)));
            toolbar.SetBackgroundColor(online
                ? new Color(ContextCompat.GetColor(this,
                    Resource.Color.colorStatusOk))
                : new Color(ContextCompat.GetColor(this,
                    Resource.Color.colorStatusError)));
        }

        public override void OnBackPressed() {
            var drawer = FindViewById<DrawerLayout>(Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
                drawer.CloseDrawer(GravityCompat.Start);
            else base.OnBackPressed();
        }

        public bool OnNavigationItemSelected(IMenuItem item) {
            var drawer = FindViewById<DrawerLayout>(Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public override void OnRequestPermissionsResult(int request, string[] permissions,
                [GeneratedEnum] Permission[] results) {
            Platform.OnRequestPermissionsResult(request, permissions, results);
            base.OnRequestPermissionsResult(request, permissions, results);
        }

        /// <summary>
        /// Resets the controls to their initial state
        /// </summary>
        private void ResetControls() {
            FindViewById<EditText>(Id.ServerMaxPlayers).Text = "";
            FindViewById<EditText>(Id.ServerOnlinePlayers).Text = "";
            FindViewById<EditText>(Id.ServerVersion).Text = "";

            // set the motd field initial text
            var motd = FindViewById<WebView>(Id.ServerMotd);
            //motd.HorizontalScrollBarEnabled = true;
            motd.Settings.UseWideViewPort = true;
            const string html = @"<html><body>Enter the server address and"
                                + "<br>press Ping to begin</body></html>";
            motd.LoadData(html, "text/html; charset=utf-8", "UTF-8");
        }

    }

}

