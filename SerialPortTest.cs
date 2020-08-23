using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Fx;

namespace Serial_Port_Testing
{
    class Program
    {
        static string lastCommand = "";
        public static List<string> ports = new List<string>();
        public static string selectedPort;
        public static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            // ######################################################################
            // T. Nathan Mundhenk
            // mundhenk@usc.edu
            // C/C++ Macro HSV to RGB

            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        public static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }


        static void Main(string[] args)
        {
            

            ports = SerialPort.GetPortNames().ToList<string>();
            foreach (var item in ports)
            {
                Console.Write(item + ", ");
            }
            Console.WriteLine();
            Console.WriteLine("Select arduino port: ");
            selectedPort = Console.ReadLine();
            bool nowRed = true;

            SerialPort arduinoPort = new SerialPort(selectedPort, 115200, Parity.None, 8);
            arduinoPort.Open();
            Console.WriteLine("Connection Status: " + arduinoPort.IsOpen);
            arduinoPort.Write("#");
            while (lastCommand != "exit")
            {

                Console.WriteLine("Type Command: ");
                lastCommand = Console.ReadLine();

                if(lastCommand.Length == 1)
                {
                    arduinoPort.Write(lastCommand);
                }


                while(lastCommand == "" || lastCommand == null)
                {
                    if (nowRed)
                    {
                        arduinoPort.Write("x9");
                        Console.WriteLine("RED!");
                        nowRed = false;
                    }
                    else
                    {
                        arduinoPort.Write("c9");
                        Console.WriteLine("BLUE!");
                        nowRed = true;
                    }
                    lastCommand = Console.ReadLine();
                }

                if(lastCommand == "Visualizer")
                {

                    lastCommand = Console.ReadLine();
                }

                if(lastCommand == "SetColor")
                {
                    int r, g, b;
                    double h = 255;
                    double s = 0.5f;
                    double v = 0.3f;
                    HsvToRgb(h, 1, 1, out r, out g, out b);
                    Console.WriteLine("R: " + r);
                    Console.WriteLine("G: " + g);
                    Console.WriteLine("B: " + b);
                    Console.WriteLine("");

                    decimal p_r; //Primary Red
                    decimal p_g; //Primary Green
                    decimal p_b; //Primary Blue
                    byte[] primary = new byte[3];

                    Console.WriteLine("Set primary color (0-255): ");
                    Console.Write("Red: ");
                    p_r = Convert.ToDecimal(Console.ReadLine());
                    primary[0] = Convert.ToByte(p_r);
                    Console.Write("Green: ");
                    p_g = Convert.ToDecimal(Console.ReadLine());
                    primary[1] = Convert.ToByte(p_g);
                    Console.Write("Blue: ");
                    p_b = Convert.ToDecimal(Console.ReadLine());
                    primary[2] = Convert.ToByte(p_b);

                    decimal s_r; //Secondary Red
                    decimal s_g; //Secondary Green
                    decimal s_b; //Secondary Blue
                    byte[] secondary = new byte[3];

                    Console.WriteLine("Set secondary color (0-255): ");
                    Console.Write("Red: ");
                    s_r = Convert.ToDecimal(Console.ReadLine());
                    secondary[0] = Convert.ToByte(s_r);
                    Console.Write("Green: ");
                    s_g = Convert.ToDecimal(Console.ReadLine());
                    secondary[1] = Convert.ToByte(s_g);
                    Console.Write("Blue: ");
                    s_b = Convert.ToDecimal(Console.ReadLine());
                    secondary[2] = Convert.ToByte(s_b);

                    Console.WriteLine("Press Enter to send color data.");
                    lastCommand = Console.ReadLine();

                    arduinoPort.Write("$"); //Open primary color input
                    arduinoPort.Write(primary, 0 ,3);

                    arduinoPort.Write("%");
                    arduinoPort.Write(secondary, 0, 3);
                }

            }
            arduinoPort.Close();
        }
    }
}
