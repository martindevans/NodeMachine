using System;
using System.Windows.Media;

namespace NodeMachine
{
    public class HSLColor
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double _hue = 1.0;
        private double _saturation = 1.0;
        private double _luminosity = 1.0;

        public double Hue
        {
            get { return _hue; }
            set { _hue = CheckRange(value); }
        }
        public double Saturation
        {
            get { return _saturation; }
            set { _saturation = CheckRange(value); }
        }
        public double Luminosity
        {
            get { return _luminosity; }
            set { _luminosity = CheckRange(value); }
        }

        private static double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        #region Casts to/from System.Drawing.Color
        public static implicit operator Color(HSLColor hslColor)
        {
            double temp2 = GetTemp2(hslColor);
            double temp1 = 2.0 * hslColor._luminosity - temp2;

            double r = GetColorComponent(temp1, temp2, hslColor._hue + 1.0 / 3.0);
            double g = GetColorComponent(temp1, temp2, hslColor._hue);
            double b = GetColorComponent(temp1, temp2, hslColor._hue - 1.0 / 3.0);

            return Color.FromRgb((byte)(byte.MaxValue * r), (byte)(byte.MaxValue * g), (byte)(byte.MaxValue * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }
        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;
            if (hslColor._luminosity < 0.5)  //<=??
                temp2 = hslColor._luminosity * (1.0 + hslColor._saturation);
            else
                temp2 = hslColor._luminosity + hslColor._saturation - (hslColor._luminosity * hslColor._saturation);
            return temp2;
        }
        #endregion

        public HSLColor(double hue, double saturation, double luminosity)
        {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }
    }
}
