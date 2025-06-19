using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace MyLoginApp;

[Activity(Theme = "@style/Maui.SplashTheme",
          MainLauncher = true,
          LaunchMode = LaunchMode.SingleTop,
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                                ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{

    private void RequestCameraPermission()
    {
        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Camera }, 0);
        }
    }
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Camera }, 0);
        }
    }
}
