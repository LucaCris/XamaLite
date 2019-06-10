using System;
using System.Linq;
using Xamarin.Forms;

namespace Xamanoid
{
  public partial class MainPage
  {
    int m_BonusKind; // 0=off, 1=superball, 2=wideplayer, 3=autograb, 4=lives, 5=points
    int m_livesflash;

    private void DeployBonus(Rectangle bounds)
    {
      if (m_BonusKind != 0 || m_IsGameOver)
        return;

      if (view.Children.Count % 4 != 3)
        return;

      if (rnd.NextDouble() < .2)
        return;

      m_BonusKind = rnd.Next(1, 6);
      if (m_BonusKind == 4 && m_lives >= 3)
        m_BonusKind++;
      m_BonusObj.Color = m_BonusColor[m_BonusKind - 1];
      m_BonusObj.IsVisible = true;
      bounds = bounds.Inflate(-2, -2);

      AbsoluteLayout.SetLayoutBounds(m_BonusObj, bounds);
    }

    void MoveBonus()
    {
      if (m_BonusKind == 0)
        return;

      var pos = m_BonusObj.Bounds;
      pos.Y += m_BallObj.Height / 2;
      AbsoluteLayout.SetLayoutBounds(m_BonusObj, pos);

      if (m_PlayerObj.Bounds.IntersectsWith(m_BonusObj.Bounds)) {
        m_pts += 100;
        switch (m_BonusKind) {
          case 1:
            BouncingBall = false;

            WidePlayer = false;
            m_AutoGrab = 0;
            break;
          case 2:
            WidePlayer = true;

            m_AutoGrab = 0;
            BouncingBall = true;
            break;
          case 3:
            AutoGrab();

            WidePlayer = false;
            break;
          case 4:
            m_lives++;
            m_livesflash = 120;
            ShowData();
            break;
          case 5:
            m_pts += 1000;
            break;
        }
        DisplayPlayer();
        RemoveBonus();
      } else if (pos.Y > m_PlayerObj.Y + m_PlayerObj.Height) {
        RemoveBonus();
      }
    }

    void RemoveBonus()
    {
      m_BonusObj.IsVisible = false;
      m_BonusKind = 0;
    }
  }
}