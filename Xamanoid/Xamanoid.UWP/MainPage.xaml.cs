using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Xamanoid.UWP
{
  public sealed partial class MainPage
  {
    Xamanoid.App m_App;

    public MainPage()
    {
      this.InitializeComponent();

      m_App = new Xamanoid.App();
      LoadApplication(m_App);
    }

    Xamanoid.MainPage Page()
    {
      return ((Xamanoid.MainPage)m_App.MainPage);
    }

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
      base.OnKeyDown(e);

      if (e.Key == Windows.System.VirtualKey.Left)
        Page().Press(true);
      else if (e.Key == Windows.System.VirtualKey.Right)
        Page().Press(false);
      else if (e.Key == Windows.System.VirtualKey.Up)
        Page().Fire();

#if DEBUG
      if (e.Key == Windows.System.VirtualKey.N)
        Page().NextLevel();
      else if (e.Key == Windows.System.VirtualKey.B)
        Page().BouncingBall = !Page().BouncingBall;
      else if (e.Key == Windows.System.VirtualKey.W)
        Page().WidePlayer = !Page().WidePlayer;
      else if (e.Key == Windows.System.VirtualKey.G)
        Page().AutoGrab();
#endif
    }

    protected override void OnKeyUp(KeyRoutedEventArgs e)
    {
      base.OnKeyUp(e);
      Page().Release();
    }
  }
}
