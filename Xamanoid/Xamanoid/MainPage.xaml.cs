using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Numerics;
using Android.Util;
using Android.Views;
using Android.Content;
using System.IO;
using System.Reflection;

namespace Xamanoid
{
  class Brick : BoxView
  {
    public BoxView Shadow;
  }

  class Ball : BoxView
  {
    public BoxView Shadow;
  }

  class Player : BoxView
  {
    public BoxView Shadow;
  }

  public partial class MainPage : ContentPage
  {
    bool DODEAD = true;
    int FIRSTLEVEL = 1;
    const double PLAYERW = 0.2;

    Rectangle m_BallPos = new Rectangle() { X = 0.5, Y = 0.8 };
    Rectangle m_PlayerPos = new Rectangle() { X = 0.5, Y = 0.98 };
    Ball m_BallObj;
    Player m_PlayerObj;
    BoxView m_BonusObj;

    List<BoxView> m_Bullet = new List<BoxView>();

    double m_dx, m_dy;
    double m_pdx = 0;
    double m_acc = 0;
    bool m_IsGrabbed = true;
    bool m_IsGameOver = true;
    bool m_BouncingBall = true;
    bool m_IsWide;
    int m_AutoGrab;
    int m_pts = 0, m_disppts;
    int suspend = 0;
    int m_baseviewobjs;
    int m_lives;
    bool m_AutoPlay = false;
    int m_level;
    public string m_levels;
    Color m_Super = Color.Silver;
    Color m_Double = Color.DodgerBlue;
    Color m_Half = Color.LightSkyBlue;
    bool m_Timer = false;
    bool m_TimerStop = false;
    double m_maxspeed = .025;

    string MessageBase = "|||GAME OVER|PRESS ⬤ TO START|LAST SCORE: *S LEVEL: *L|HI-SCORE: *H";
    string Message;
    int MessagePos, MessageDemux;

    Random rnd = new Random();

    int m_blipStereo = 0;
    int curr_music = 0;

    Color[] m_BonusColor = { Color.Yellow, Color.Beige, Color.OrangeRed, Color.Red, Color.Gold, Color.LightGray };

    public MainPage()
    {
      InitializeComponent();
      view.SizeChanged += OnSizeChanged;

      if (DesignMode.IsDesignModeEnabled)
        return;

      var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
      Stream stream = assembly.GetManifestResourceStream("Xamanoid.levels.txt");
      using (var reader = new System.IO.StreamReader(stream)) {
        m_levels = reader.ReadToEnd();
      }
      m_levels = m_levels.Replace("\r\n", "\n");
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
      m_BallPos.Width = view.Bounds.Width / 55.0;
      m_BallPos.Height = m_BallPos.Width;
      m_PlayerPos.Width = view.Bounds.Width * PLAYERW;
      m_PlayerPos.Height = m_BallPos.Height;
      if (!m_IsGameOver) {
        GoGameOver();
      }
      ShowData();
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
      System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    void CreateLevel()
    {
      curr_music = (m_level - 1) % 3;

      RemoveBonus();
      view.Children.Clear();
      view.Children.Add(m_BallObj);
      view.Children.Add(m_PlayerObj);
      view.Children.Add(m_BonusObj);
      m_baseviewobjs = view.Children.Count;

      shadowview.Children.Clear();
      shadowview.Children.Add(m_BallObj.Shadow);
      shadowview.Children.Add(m_PlayerObj.Shadow);

      over.Children.Clear();

      m_PlayerPos.X = 0.5;

      m_livesflash = 0;
      lives.BackgroundColor = Color.Transparent;

      FW.Text = m_level.ToString("D2");

      double tw = view.Bounds.Width / 10;
      double w = tw * .9;
      double ow = (tw - w) / 2;

      double th = tw / 2;
      double h = th * .85;

      int st;
      int findlevel = m_level;
      do {
        st = m_levels.IndexOf(">" + findlevel.ToString("D2"));
        findlevel--;
      } while (st < 0);
      int en = m_levels.IndexOf("<", st);
      string lev = m_levels.Substring(st + 4, en - st - 4);
      string[] lines = lev.Split('\n');
      for (int y = 0; y < lines.Length; y++) {
        for (int x = 0; x < lines[y].Length; x++) {
          char c = lines[y][x];
          if ((c >= '1' && c <= '3') || (c == '=') || (c == '-')) {
            var brick = new Brick();

            switch (c) {
              case '=':
                brick.Color = m_Super;
                m_baseviewobjs++;
                break;
              case '-':
                brick.Color = m_Double;
                break;
              case '1':
                brick.Color = Color.Yellow;
                break;
              case '2':
                brick.Color = Color.Red;
                break;
              case '3':
                brick.Color = Color.Green;
                break;
            }
            AbsoluteLayout.SetLayoutFlags(brick, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(brick, new Rectangle(ow + x * tw, y * th, w, h));
            view.Children.Add(brick);

            var shbr = new BoxView() { Color = Color.Black };
            AbsoluteLayout.SetLayoutFlags(shbr, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(shbr, new Rectangle(ow + x * tw, y * th, w, h));
            shadowview.Children.Add(shbr);

            brick.Shadow = shbr;
          }
        }
      }

      BouncingBall = true;
      WidePlayer = false;
      m_BonusKind = 0;

      ShowData();
    }

    void ShowData()
    {
      if (m_IsGameOver) {
        lives.Text = null;
        brickN.Text = null;
        pts.Text = null;
        FW.Text = "⬤";
        return;
      }
      if (m_lives > 0)
        lives.Text = "L:" + string.Concat(Enumerable.Repeat("❤️", m_lives));
      else
        lives.Text = "💀";
      brickN.Text = $"B:{(view.Children.Count - m_baseviewobjs).ToString("D3")}";
      pts.Text = $"P:{m_disppts.ToString("D6")}" + (m_AutoPlay ? "A" : null);
    }

    public void Press(bool left)
    {
      if (m_pdx != 0)
        return;
      m_acc = 0.01;
      if (left)
        m_pdx = -1;
      else
        m_pdx = 1;
    }

    public void Release()
    {
      m_pdx = 0;
      m_acc = 0;
    }

    public void Fire()
    {
      FW_Clicked(null, null);
    }

    private void Button_Pressed(object sender, EventArgs e)
    {
      Press(sender == LT);
    }

    private void Button_Released(object sender, EventArgs e)
    {
      Release();
    }

    protected override void OnAppearing()
    {
      base.OnAppearing();

      m_BallObj = new Ball() { Color = Color.White };
      m_BallObj.Shadow = new BoxView() { Color = Color.Black };
      AbsoluteLayout.SetLayoutFlags(m_BallObj, AbsoluteLayoutFlags.PositionProportional);
      AbsoluteLayout.SetLayoutFlags(m_BallObj.Shadow, AbsoluteLayoutFlags.None);

      m_PlayerObj = new Player() { Color = Color.LightGray, CornerRadius = new CornerRadius(4) };
      m_PlayerObj.Shadow = new BoxView() { Color = Color.Black, CornerRadius = new CornerRadius(4) };
      AbsoluteLayout.SetLayoutFlags(m_PlayerObj, AbsoluteLayoutFlags.PositionProportional);
      AbsoluteLayout.SetLayoutFlags(m_PlayerObj.Shadow, AbsoluteLayoutFlags.None);

      m_BonusObj = new BoxView() { IsVisible = false, CornerRadius = new CornerRadius(8) };
      AbsoluteLayout.SetLayoutFlags(m_BonusObj, AbsoluteLayoutFlags.None);

      if (!DesignMode.IsDesignModeEnabled && !m_Timer) {
        m_Timer = true;
        Device.StartTimer(TimeSpan.FromMilliseconds(1), MainLoop);
        //OrientationSensor.Start(SensorSpeed.UI);
      }
    }

    private void FW_Clicked(object sender, EventArgs e)
    {
      if (!m_Timer)
        return;

      if (m_IsGameOver) {
        GameTitle.IsVisible = false;
        head.Text = null;
        m_IsGameOver = false;
        m_IsGrabbed = true;
        m_lives = 3;
        m_pts = 0;
        m_disppts = 0;
        m_AutoPlay = false;
        m_pdx = 0;
        m_level = FIRSTLEVEL;
        CreateLevel();
        return;
      }

      if (m_IsGrabbed) {
        m_dx = rnd.NextDouble() * 0.02 - 0.01;
        if (m_AutoGrab > 0)
          m_dx /= 3.0;
        m_dy = -.01;
        m_IsGrabbed = false;
      }
    }

    int GetHiScore()
    {
      if (App.Current.Properties.ContainsKey("hiscore"))
        return (int)App.Current.Properties["hiscore"];
      else
        return 0;
    }

    void SetHiScore()
    {
      App.Current.Properties["hiscore"] = m_pts;
      App.Current.SavePropertiesAsync();
    }

    bool GameOverLoop()
    {
      if (view.Bounds.Width > view.Bounds.Height) {
        var th = MainGrid.Margin;
        th.Left += 10;
        th.Right += 10;
        MainGrid.Margin = th;
      } else if (MainGrid.Width < MainGrid.Height / 2)
        MainGrid.Margin = new Thickness(0);

      if (view.Children.Count == 0) {
        view.Children.Add(m_BallObj);
        m_BallPos.X = 0.5;
        m_BallPos.Y = 0.5;
        m_dx = rnd.NextDouble() * 0.02 - 0.01;
        m_dy = -.01;
        m_IsGrabbed = false;
        Message = MessageBase.Replace("*S", m_pts.ToString("D6")).Replace("*L", m_level.ToString()).Replace("*H", GetHiScore().ToString("D6"));
        if (m_pts > 0 && m_pts == GetHiScore())
          Message = "|✌ NEW HI-SCORE ✌" + Message;
        Message = Message.Replace("|", "   ★   ");
        MessagePos = 0;
        MessageDemux = 0;
      }

      if (MessageDemux++ == 0) {
        head.Text = Message.Substring(MessagePos++) + Message;
        if (MessagePos == Message.Length)
          MessagePos = 0;
      }
      if (MessageDemux == 8)
        MessageDemux = 0;

      if (view.Children.Count < 50 && rnd.Next(5 + view.Children.Count * 2) == 0) {
        var brick = new Brick() { Color = Color.FromUint((uint)rnd.Next(0xffffff) + 0xff000000) };

        AbsoluteLayout.SetLayoutFlags(brick, AbsoluteLayoutFlags.All);
        AbsoluteLayout.SetLayoutBounds(brick, new Rectangle(rnd.Next(1, 10) / 10.0, rnd.Next(1, 10) / 10.0, .08, .025));

        view.Children.Add(brick);
      }

      if (BouncingBall == true && view.Children.Count > 30)
        BouncingBall = false;

      if (BouncingBall == false && view.Children.Count < 20)
        BouncingBall = true;

      double prevX = m_BallObj.X + m_BallObj.Width / 2;
      double prevY = m_BallObj.Y + m_BallObj.Height / 2;
      MoveBall();
      CheckBricks(prevX, prevY);
      RemoveBricks();

      return true;
    }

    public void NextLevel()
    {
      m_level++;
      CreateLevel();
      m_IsGrabbed = true;
    }

    void GoGameOver()
    {
      shadowview.Children.Clear();
      m_IsGameOver = true;
      m_AutoPlay = false;
      ShowData();
      view.Children.Clear();
      m_BouncingBall = true;
      GameTitle.IsVisible = true;
      if (m_pts > GetHiScore()) {
        SetHiScore();
      }
    }

    void DisplayPlayer()
    {
      if (m_AutoGrab > 0)
        m_PlayerObj.Color = Color.OrangeRed;
      else
        m_PlayerObj.Color = Color.LightGray;

      AbsoluteLayout.SetLayoutBounds(m_PlayerObj, m_PlayerPos);
      AbsoluteLayout.SetLayoutBounds(m_PlayerObj.Shadow, m_PlayerObj.Bounds);
    }

    public bool BouncingBall
    {
      get { return m_BouncingBall; }
      set
      {
        m_BouncingBall = value;
        if (!value)
          m_BallObj.Color = Color.Yellow;
        else
          m_BallObj.Color = Color.White;
      }
    }

    public bool WidePlayer
    {
      get { return m_IsWide; }
      set
      {
        m_IsWide = value;
        if (value)
          m_PlayerPos.Width = view.Bounds.Width * PLAYERW * 2;
        else
          m_PlayerPos.Width = view.Bounds.Width * PLAYERW;

        DisplayPlayer();
      }
    }

    public void AutoGrab()
    {
      m_AutoGrab += 3;
      DisplayPlayer();
    }
  }
}
