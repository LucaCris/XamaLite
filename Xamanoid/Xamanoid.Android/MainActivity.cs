using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Xamanoid.Droid
{
  [Activity(Label = "Xamanoid", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
  public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
  {
    protected override void OnCreate(Bundle savedInstanceState)
    {
      TabLayoutResource = Resource.Layout.Tabbar;
      ToolbarResource = Resource.Layout.Toolbar;
      base.OnCreate(savedInstanceState);

      this.Window.AddFlags(WindowManagerFlags.Fullscreen);
      this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

      global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
      LoadApplication(new App());
    }
  }
}