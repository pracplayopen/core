using System;
using System.Collections.Generic;
using System.Drawing;
using TradeLink.API;
using TradeLink.Common;

namespace TradeLink.Common
{
    /// <summary>
    /// for consistent auto-coloring of indicators
    /// </summary>
    public class GTColorHelper
    {
        static Dictionary<string, System.Drawing.Color> _lab2col = new Dictionary<string, System.Drawing.Color>();
        static List<Color> colororder = new List<Color>();

        public static string ColorSchemePath = string.Empty;
        static int[] colints = new int[0];
        static bool isColorScehmePathDefined { get { return (colints.Length != 0) || (!string.IsNullOrWhiteSpace(ColorSchemePath) && Util.FromFile<int[]>(ColorSchemePath, ref colints) && (colints.Length != 0)); } }

        public static string[] Colors2String(Color[] cols)
        {
            List<string> names = new List<string>();
            foreach (var c in cols)
                names.Add(c.ToString());
            return names.ToArray();
        }

        public static string[] Colors2StringNum(Color[] cols)
        {
            List<string> names = new List<string>();
            int ct = 0;
            foreach (var c in cols)
            {
                names.Add(ct.ToString());
                ct++;
            }
            return names.ToArray();
        }

        public static Color[] GetUnselectedColors()
        {
            List<Color> ok = new List<Color>();
            var sel = GetAllSelectedColors();
            var all = GetAllColors();
            foreach (var c in all)
            {
                bool found = false;
                foreach (var sc in sel)
                    if (c == sc)
                    {
                        found = true;
                        break;
                    }
                if (found)
                    continue;
                ok.Add(c);


            }
            return ok.ToArray();
        }

        public static Color[] GetAllColors()
        {
            return colororder.ToArray();

        }

        public static Color[] GetAllSelectedColors()
        {
            var vs = _lab2col.Values;
            Color[] f = new Color[vs.Count];
            vs.CopyTo(f, 0);
            return f;
        }

        public static void SetColor(string label, Color c)
        {
            // set it
            if (_lab2col.ContainsKey(label))
                _lab2col[label] = c;
            else
                _lab2col.Add(label, c);
            // save it
            GetSerializedColorOrder();
        }

        public static Color GetColor() { return GetColor("mylabel"+_lab2col.Count); }
        public static Color GetColor(string label)
        {
            if (colororder.Count == 0)
                ClearColorLabels();
            System.Drawing.Color c;
            if (_lab2col.TryGetValue(label, out c))
                return c;
            c = colororder[_lab2col.Count];
            _lab2col.Add(label, c);
            return c;
        }
        public static void ClearColorLabels()
        {
            _lab2col.Clear();
            colororder = new List<Color>();
            // restore colors order if they exist
            if (!string.IsNullOrWhiteSpace(SerializedColorOrder))
            {
                var sco = TradeLink.Common.Util.Deserialize<int[]>(SerializedColorOrder);
                foreach (var sc in sco)
                    colororder.Add(Color.FromArgb(sc));
            }
            else // otherwise create
                NewColorOrder();




        }

        public static List<Color> ExcludedColors
        {
            get
            {
                return new List<Color>(new Color[] { 
            Color.White, Color.Black, Color.Gray, Color.Pink, Color.CornflowerBlue 
        });
            }
        }

        public static void NewColorOrder() { NewColorOrder(false); }
        public static void NewColorOrder(bool force)
        {
            // clear everything
            _lab2col.Clear();
            colororder = new List<Color>();
            SerializedColorOrder = string.Empty;

            if (force || !isColorScehmePathDefined)
            {

                // keep track of whats picked
                picked.Clear();
                // don't pick special colors
                excludespecial(ExcludedColors.ToArray());

                Random picker = new Random();
                int maxcol = 250;

                while (colororder.Count < maxcol)
                {
                    // get random rgp
                    var r = picker.Next(0, 256);
                    var g = picker.Next(0, 256);
                    var b = picker.Next(0, 256);
                    // ignore if picked
                    var c = Color.FromArgb(r, g, b);
                    int v;
                    if (picked.TryGetValue(c.ToArgb(), out v))
                        continue;
                    picked.Add(c.ToArgb(), 0);
                    colororder.Add(c);



                }
            }
            else if (isColorScehmePathDefined) // grab from file
            {
                foreach (var colint in colints)
                {
                    int tv;
                    if (picked.TryGetValue(colint, out tv))
                        continue;
                    var c = Color.FromArgb(colint);
                    picked.Add(colint, 0);
                    colororder.Add(c);
                }
            }
            GetSerializedColorOrder();
        }

        static Dictionary<int, int> picked = new Dictionary<int, int>();

        static void excludespecial(params Color[] cols) { foreach (Color c in cols) excludespecial(c); }
        static void excludespecial(Color c)
        {
            picked.Add(c.ToArgb(), 0);
        }

        public static string SerializedColorOrder = string.Empty;

        public static string GetSerializedColorOrder()
        {
            if (colororder.Count == 0)
                NewColorOrder();
            List<int> sco = new List<int>(colororder.Count);
            foreach (var co in colororder)
                sco.Add(co.ToArgb());
            SerializedColorOrder = TradeLink.Common.Util.Serialize<int[]>(sco.ToArray());
            if (!string.IsNullOrWhiteSpace(ColorSchemePath))
            {
                Util.ToFile<int[]>(sco.ToArray(), ColorSchemePath);
            }
            return SerializedColorOrder;
        }
    }
}
