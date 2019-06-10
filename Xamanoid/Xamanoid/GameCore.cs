using System;
using System.Linq;
using Xamarin.Forms;

namespace Xamanoid
{
  public partial class MainPage
  {
    private bool MainLoop()
    {
      if (m_TimerStop) {
        m_TimerStop = false;
        m_Timer = false;
        return false;
      }

      if (suspend > 0) {
        suspend--;
        return true;
      }

      if (m_IsGameOver)
        return GameOverLoop();
      else if (m_lives == 0) {
        GoGameOver();
        return true;
      }

      if (m_disppts < m_pts) {
        m_disppts += 5;
        ShowData();
      }

      double prevX = m_BallObj.X + m_BallObj.Width / 2;
      double prevY = m_BallObj.Y + m_BallObj.Height / 2;
      MoveBall();

      if (m_BallPos.Y >= 1 && DODEAD == true)
        Missed();

      if (RemoveBricks()) {
        ShowData();
        if (view.Children.Count == m_baseviewobjs) {
          NextLevel();
          return true;
        }
      }

      CheckBricks(prevX, prevY);

      CheckPlayer();

      MovePlayer();
      MoveBonus();

      if (m_livesflash > 0) {
        m_livesflash--;
        if (m_livesflash % 10 >= 5)
          lives.BackgroundColor = Color.Yellow;
        else
          lives.BackgroundColor = Color.Transparent;
      }

      return true;
    }

    void MoveBall()
    {
      if (m_IsGrabbed) {
        var q = m_PlayerPos.X - 0.5;
        m_BallPos.X = 0.5 + q * .82;  // factor to mantain alignment
        m_BallPos.Y = m_PlayerPos.Y - 0.02;
        m_dx = 0;
        m_dy = 0;
      } else if (suspend == 0) {
        m_BallPos.X += m_dx;
        m_BallPos.Y += m_dy;
      }
      AbsoluteLayout.SetLayoutBounds(m_BallObj, m_BallPos);
      AbsoluteLayout.SetLayoutBounds(m_BallObj.Shadow, m_BallObj.Bounds);
      if (m_BallPos.X <= 0 || m_BallPos.X >= 1) {
        m_dx = -m_dx;
        if (m_IsGameOver)
        {
          m_dx += rnd.NextDouble() * 0.02 - 0.01;
          m_dx = Math.Max(-.03, Math.Min(.03, m_dx));
        }
        m_BallPos.X += m_dx;
      }
      if (m_BallPos.Y <= 0 || m_BallPos.Y >= 1) {
        m_dy = -m_dy;
      }
    }

    void Missed()
    {
      if (!m_IsGrabbed) {
        RemoveBonus();
        m_IsGrabbed = true;
        m_lives--;
        m_AutoGrab = 0;
        WidePlayer = false;
        BouncingBall = true;
        m_disppts = m_pts;
        over.Children.Clear();
        ShowData();
      }
    }

    void CheckBricks(double prevX, double prevY)
    {
      foreach (var brick in view.Children.OfType<BoxView>()) {
        if (brick == m_BallObj || brick == m_PlayerObj || brick == m_BonusObj || brick.Opacity != 1)
          continue;

        if (brick.Bounds.IntersectsWith(m_BallObj.Bounds)) {
          if (!m_IsGameOver) {
            m_blipStereo = 1 - m_blipStereo;
          }
          if (brick.Color != m_Super) {
            if (brick.Color != m_Double) {
              brick.Opacity = 0.9;
              m_pts += 20 + m_level * 5;
              DeployBonus(brick.Bounds);
            } else {
              brick.Color = m_Half;
              m_pts += 15;
            }
          }

          if (m_BouncingBall || brick.Color == m_Super) {
            if (prevY < brick.Y || prevY > brick.Y + brick.Height)
              m_dy = -m_dy;
            else if (prevX + m_BallPos.Width < brick.X)
              m_dx = -Math.Max(0.01, Math.Abs(m_dx + rnd.NextDouble() / 250));
            else if (prevX > brick.X + brick.Width)
              m_dx = Math.Max(0.01, Math.Abs(m_dx + rnd.NextDouble() / 250));
            else
              m_dy = -m_dy;
          }

          break;
        }

        foreach (var bullet in over.Children.OfType<BoxView>()) {
          if (brick.Bounds.IntersectsWith(bullet.Bounds)) {
            if (brick.Color != m_Super) {
              brick.Opacity = 0.9;
              m_blipStereo = 1 - m_blipStereo;
            }
            bullet.IsVisible = false;
          }
        }
      }
    }

    bool RemoveBricks()
    {
      foreach (var brick in view.Children.OfType<Brick>()) {
        var shadow = brick.Shadow;
        if (brick.Opacity != 1) {
          brick.Opacity *= .9;
          if (shadow != null)
            shadow.Opacity *= .9;
          if (brick.Opacity < .1) {
            view.Children.Remove(brick);
            if (shadow != null)
              shadowview.Children.Remove(brick.Shadow);
            return true;
          }
        }
      }
      return false;
    }

    void CheckPlayer()
    {
      if (m_PlayerObj.Bounds.IntersectsWith(m_BallObj.Bounds) && m_dy > 0) {
        m_dy = -m_dy;

        if (m_AutoGrab <= 0) {
          if (m_acc != 0) {
            m_dx += m_acc * m_pdx;
            m_dx = Math.Max(-m_maxspeed, Math.Min(m_maxspeed, m_dx));
          } else if (Math.Abs(m_dx) > 0.01)
            m_dx *= .9;
        } else {
          m_AutoGrab--;
          m_IsGrabbed = true;
          if (m_AutoGrab == 0)
            DisplayPlayer();
        }
      }
    }

    void MovePlayer()
    {
      if (m_pdx != 0)
        m_acc += 0.002;

      m_PlayerPos.X += m_pdx * m_acc;
      if (m_pdx != 0) {
        m_PlayerPos.X = Math.Max(0, Math.Min(1, m_PlayerPos.X));
        if (m_PlayerPos.X == 0 || m_PlayerPos.X == 1)
          m_acc = 0;
      }

      DisplayPlayer();
    }
  }
}