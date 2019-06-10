using System;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Xamanoid
{
  public partial class App : Application
  {
    public App()
    {
      InitializeComponent();

      MainPage = new MainPage();
    }

    protected override void OnStart()
    {
      // Handle when your app starts
    }

    protected override void OnSleep()
    {
      // Handle when your app sleeps
    }

    protected override void OnResume()
    {
      // Handle when your app resumes
    }
  }

  [ContentProperty(nameof(Source))]
  public class ImageResourceExtension : IMarkupExtension
  {
    public string Source { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
      if (Source == null) {
        return null;
      }

      // Do your translation lookup here, using whatever method you require
      var imageSource = ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);

      return imageSource;
    }
  }
}
