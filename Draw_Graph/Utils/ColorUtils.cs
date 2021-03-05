using Draw_Graph.Element;
using Microsoft.Msagl.Drawing;

namespace Draw_Graph.Utils
{
    public static class ColorUtils
    {
        public static Color GetColor(DrawColor color)
        {
            switch(color)
            {
                case DrawColor.Blue:
                    return Color.LightBlue;
                case DrawColor.Green:
                    return Color.LightGreen;
                case DrawColor.Red:
                    return Color.OrangeRed;
                case DrawColor.Yellow:
                    return Color.Yellow;
                case DrawColor.Orange:
                    return Color.Orange;
                case DrawColor.Magenta:
                    return Color.Pink;
                case DrawColor.Brown:
                    return Color.RosyBrown;
                case DrawColor.Gold:
                    return Color.Gold;
                case DrawColor.Fuchsia:
                    return Color.Fuchsia;
                default:
                    return Color.Gray;
            }
        }
    }
}
