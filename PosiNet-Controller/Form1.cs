using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.DirectInput;
using System.IO;
using System.Xml.Serialization;

namespace PosiNet_Controller
{
    public partial class Form1 : Form
    {
        //declarations
        Point Brain = new Point(50, 50);
        Timer syncTimer = new Timer();
        int xInversTrig = 1;
        int yInversTrig = 1;
        int zInversTrig = 1;
        int xOffInversTrig = 1;
        int yOffInversTrig = 1;
        int movementX = 0;
        int movementY = 0;
        int offsetmovementX = 0;
        int offsetmovementY = 0;
        int dimmer = 0;
        int dimmerChange = 0;
        byte[] _dmxData = new byte[511];
        ArtNet.Sockets.ArtNetSocket artnet = new ArtNet.Sockets.ArtNetSocket();
        Joystick joystick = startJoystick();
        Point lowerBound = new Point(6, 6);
        Point upperBound = new Point(955, 537);
        int timerInterval = 20;
        string[] lineArray;

        static Joystick startJoystick()
        {
            //Start Joystick
            // Initialize DirectInput
            DirectInput directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad,
                        DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick,
                        DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                MessageBox.Show("Posi-Controle could not detect a Joystick or Gamepad.", "No joystick/Gamepad found");
                //Console.ReadKey();
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
                Console.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            return joystick;

        }

        private void outputConsole(string output)
        {
            lineArray = consoleOutput.Lines;
            for (int i = 0; i < 16; i ++){
                lineArray[i] = lineArray[i + 1];
            }
            lineArray[16] = output;
            consoleOutput.Lines = lineArray;
        }

        //Timer Event
        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {

            //Pull Joystick
            joystick.Poll();
            var datas = joystick.GetBufferedData();
            foreach (var state in datas)
            {
                updateMovement(state);
                Console.WriteLine(state);
                outputConsole(Convert.ToString(state));
            }

            updatePosition();


            ArtNet.Packets.ArtNetDmxPacket posipacket = new ArtNet.Packets.ArtNetDmxPacket();
            TransformData();
            posipacket.DmxData = _dmxData;
            posipacket.Universe = (short)Convert.ToInt32(universeBox.Text);
            artnet.Send(posipacket);

        }

        private void updateMovement(JoystickUpdate state)
        {
            if (Convert.ToString(state.Offset) == Convert.ToString(xAxisKey.Text))
            {
                if (state.Value > 32768)
                {
                    movementX = (int)((double)state.Value * 0.0001);
                }
                else if (state.Value < 32766)
                {
                    movementX = -(int)((double)(65535 - state.Value) * 0.0001);
                }
                else
                {
                    movementX = 0;
                }
            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(yAxisKey.Text))
            {
                if (state.Value > 32768)
                {
                    movementY = (int)((double)state.Value * 0.0001);
                }
                else if (state.Value < 32766)
                {
                    movementY = -(int)((double)(65535 - state.Value) * 0.0001);
                }
                else
                {
                    movementY = 0;
                }
            } 
            else if (Convert.ToString(state.Offset) == Convert.ToString(zAxisKey.Text))
            {
                if (state.Value > 32768)
                {
                    dimmerChange = (int)((double)state.Value * 0.0001);
                }
                else if (state.Value < 32766)
                {
                    dimmerChange = -(int)((double)(65535 - state.Value) * 0.0001);
                }
                else
                {
                    dimmerChange = 0;
                }
            } 
            else if (Convert.ToString(state.Offset) == Convert.ToString(xAxisOffsetKey.Text))
            {
                if (state.Value > 32768)
                {
                    offsetmovementX = (int)((double)state.Value * 0.0001);
                }
                else if (state.Value < 32766)
                {
                    offsetmovementX = -(int)((double)(65535 - state.Value) * 0.0001);
                }
                else
                {
                    offsetmovementX = 0;
                }
            } 
            else if (Convert.ToString(state.Offset) == Convert.ToString(yAxisOffsetKey.Text))
            {
                if (state.Value > 32768)
                {
                    offsetmovementY = (int)((double)state.Value * 0.0001);
                }
                else if (state.Value < 32766)
                {
                    offsetmovementY = -(int)((double)(65535 - state.Value) * 0.0001);
                }
                else
                {
                    offsetmovementY = 0;
                }
            } 
            else if (Convert.ToString(state.Offset) == Convert.ToString(resetOffsetKey.Text))
            {
                offsetX.Text = "0";
                offsetY.Text = "0";
            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(swCh5Key))
            {
                
            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(swCh6Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(swCh7Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(swCh8Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(swCh9Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(swCh10Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(swCh11Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(btCh12Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(btCh13Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(btCh14Key))
            {

            }
            else if (Convert.ToString(state.Offset) == Convert.ToString(btCh15Key))
            {

            }
        }

        private void updatePosition()
        {
            Brain = new Point(pictureGreenCircle.Location.X, pictureGreenCircle.Location.Y);
            offsetX.Text = Convert.ToString(Convert.ToInt32(offsetX.Text) + offsetmovementX);
            offsetY.Text = Convert.ToString(Convert.ToInt32(offsetY.Text) + offsetmovementY);

            Brain.X = Brain.X + movementX;
            Brain.Y = Brain.Y + movementY;

            if (Brain.X > upperBound.X) Brain.X = upperBound.X;
            if (Brain.Y > upperBound.Y) Brain.Y = upperBound.Y;
            if (Brain.X < lowerBound.X) Brain.X = lowerBound.X;
            if (Brain.Y < lowerBound.Y) Brain.Y = lowerBound.Y;
            pictureGreenCircle.Location = Brain;
            if (Brain.X + Convert.ToInt32(offsetX.Text) > upperBound.X) Brain.X = upperBound.X - Convert.ToInt32(offsetX.Text);
            if (Brain.Y + Convert.ToInt32(offsetY.Text) > upperBound.Y) Brain.Y = upperBound.Y - Convert.ToInt32(offsetY.Text);
            if (Brain.X + Convert.ToInt32(offsetX.Text) < lowerBound.X) Brain.X = lowerBound.X - Convert.ToInt32(offsetX.Text);
            if (Brain.Y + Convert.ToInt32(offsetY.Text) < lowerBound.Y) Brain.Y = lowerBound.Y - Convert.ToInt32(offsetY.Text);
            pictureRedCircle.Location = new Point(Brain.X + Convert.ToInt32(offsetX.Text), Brain.Y + Convert.ToInt32(offsetY.Text));
        }

        private void TransformData()
        {
            double curving(double xPro, double yPro, double dmx, int color)
            {
                double correction = 0;
                double x;
                double y;
                if (xPro < 50) { x = (50 - xPro) * -1; } else { x = xPro - 50; }
                y = -0.04 * x * x + 100;
                if (color == 1) //Green
                {
                    if (dmx > Convert.ToDouble(zTPG.Text))
                    {
                        correction = (Convert.ToDouble(bUMGY.Text) - Convert.ToDouble(zTPG.Text)) / 100 * yPro;
                        if (xPro > 50)
                        {
                            correction = correction + (Convert.ToDouble(bLMGY.Text) - Convert.ToDouble(bLGY.Text)) / 100 * ((50 - xPro) * 2);
                        }
                        else
                        {
                            correction = correction + (Convert.ToDouble(bLMGY.Text) - Convert.ToDouble(bLGY.Text)) / 100 * ((xPro - 50) * 2);
                        }
                    }
                    else
                    {
                        correction = (Convert.ToDouble(bLMGY.Text) - Convert.ToDouble(zTPG.Text)) / 100 * yPro;
                        if (xPro > 50)
                        {
                            correction = correction + (Convert.ToDouble(bLMRY.Text) - Convert.ToDouble(bLRY.Text)) / 100 * ((50 - xPro) * 2);
                        }
                        else
                        {
                            correction = correction + (Convert.ToDouble(bLMRY.Text) - Convert.ToDouble(bLRY.Text)) / 100 * ((xPro - 50) * 2);
                        }
                    }
                } 
                else if (color == 2)//Red
                {
                    if (dmx > Convert.ToDouble(zTPR.Text))
                    {
                        correction = (Convert.ToDouble(bUMRY.Text) - Convert.ToDouble(zTPR.Text)) / 100 * yPro;
                    }
                    else
                    {
                        correction = (Convert.ToDouble(bLMRY.Text) - Convert.ToDouble(zTPR.Text)) / 100 * yPro; ;
                    }
                }
                correction = 0;
                return correction;
            }
            //Green X
            double posGreenX = pictureGreenCircle.Location.X - lowerBound.X; // 6 is the lower Position Boundry
            posGreenX = (posGreenX / upperBound.X * 100); // Max X Position in Window
            double dmxGreenX = Convert.ToInt32(bUGX.Text) - Convert.ToInt32(bLGX.Text);
            dmxGreenX = (dmxGreenX / 100 * posGreenX) + Convert.ToDouble(bLGX.Text);
            //Green Xx
            double dmxGreenXx = (dmxGreenX - System.Math.Truncate(dmxGreenX)) * 255;
            //Green Y
            double posGreenY = pictureGreenCircle.Location.Y - lowerBound.Y; // 6 is the lower Position Boundry
            posGreenY = 100 - (posGreenY / upperBound.Y * 100); // Max Y Position in Window
            double dmxGreenY = Convert.ToInt32(bUGY.Text) - Convert.ToInt32(bLGY.Text);
            dmxGreenY = (dmxGreenY / 100 * posGreenY) + Convert.ToInt32(bLGY.Text);
            dmxGreenY = dmxGreenY + curving(posGreenX, posGreenY, dmxGreenY, 1);
            //Green Yy
            double dmxGreenYy = (dmxGreenY - System.Math.Truncate(dmxGreenY)) * 255;
            //Red X
            double posRedX = pictureRedCircle.Location.X - lowerBound.X; // 6 is the lower Position Boundry
            posRedX = posRedX / upperBound.X * 100; // Max X Position in Window
            double dmxRedX = Convert.ToInt32(bURX.Text) - Convert.ToInt32(bLRX.Text);
            dmxRedX = (dmxRedX / 100 * posRedX) + Convert.ToInt32(bLRX.Text);
            //Red Xx
            double dmxRedXx = (dmxRedX - System.Math.Truncate(dmxRedX)) * 255;
            //Red Y
            double posRedY = pictureRedCircle.Location.Y - lowerBound.Y; // 6 is the lower Position Boundry
            posRedY = 100 - (posRedY / upperBound.Y * 100); // Max Y Position in Window
            double dmxRedY = Convert.ToInt32(bURY.Text) - Convert.ToInt32(bLRY.Text);
            dmxRedY = (dmxRedY / 100 * posRedY) + Convert.ToInt32(bLRY.Text);
            dmxRedY = dmxRedY + curving(posRedX, posRedY, dmxRedY, 2);
            //Red Yy
            double dmxRedYy = (dmxRedY - System.Math.Truncate(dmxRedY)) * 255;

            // Truncate Decimal places
            dmxGreenX = System.Math.Truncate(dmxGreenX);
            dmxGreenXx = System.Math.Truncate(dmxGreenXx);
            dmxGreenY = System.Math.Truncate(dmxGreenY);
            dmxGreenYy = System.Math.Truncate(dmxGreenYy);
            dmxRedX = System.Math.Truncate(dmxRedX);
            dmxRedXx = System.Math.Truncate(dmxRedXx);
            dmxRedY = System.Math.Truncate(dmxRedY);
            dmxRedYy = System.Math.Truncate(dmxRedYy);

            //Set Dimmer
            dimmer = dimmer + (dimmerChange * zInversTrig);
            //Set Switches
            //Set Buttons

            // Prevent false Value to be sent
            if (dmxGreenX > 255) { dmxGreenX = 255; }
            if (dmxGreenX < 0) { dmxGreenX = 0; }
            if (dmxGreenXx > 255) { dmxGreenXx = 255; }
            if (dmxGreenXx < 0) { dmxGreenXx = 0; }
            if (dmxGreenY > 255) { dmxGreenY = 255; }
            if (dmxGreenY < 0) { dmxGreenY = 0; }
            if (dmxGreenYy > 255) { dmxGreenYy = 255; }
            if (dmxGreenYy < 0) { dmxGreenYy = 0; }
            if (dmxRedX > 255) { dmxRedX = 255; }
            if (dmxRedX < 0) { dmxRedX = 0; }
            if (dmxRedXx > 255) { dmxRedXx = 255; }
            if (dmxRedXx < 0) { dmxRedXx = 0; }
            if (dmxRedY > 255) { dmxRedY = 255; }
            if (dmxRedY < 0) { dmxRedY = 0; }
            if (dmxRedYy > 255) { dmxRedYy = 255; }
            if (dmxRedYy < 0) { dmxRedYy = 0; }
            if (dimmer > 255) { dimmer = 255; }
            if (dimmer < 0) { dimmer = 0; }




            //Fill DMX Array
            _dmxData[0] = Convert.ToByte(dmxGreenX);
            _dmxData[1] = Convert.ToByte(dmxGreenXx);
            _dmxData[2] = Convert.ToByte(dmxGreenY);
            _dmxData[3] = Convert.ToByte(dmxGreenYy);
            _dmxData[4] = Convert.ToByte(dmxRedX);
            _dmxData[5] = Convert.ToByte(dmxRedXx);
            _dmxData[6] = Convert.ToByte(dmxRedY);
            _dmxData[7] = Convert.ToByte(dmxRedYy);
            _dmxData[15] = Convert.ToByte(dimmer);

            return;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
            //Set up the timer
            syncTimer.Tick += new EventHandler(TimerEventProcessor);
            syncTimer.Interval = timerInterval;
            syncTimer.Start();

            //ArtNet Stuff
            artnet.EnableBroadcast = true;
            Console.WriteLine(artnet.BroadcastAddress.ToString());
            artnet.Open(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("255.255.255.0"));

            //Set Inverse Vars
            if (xInverse.Checked) xInversTrig = xInversTrig * -1;
            if (yInverse.Checked) yInversTrig = yInversTrig * -1;
            if (zInverse.Checked) zInversTrig = zInversTrig * -1;
            if (xOffInverse.Checked) xOffInversTrig = xOffInversTrig * -1;
            if (yOffInverse.Checked) yOffInversTrig = yOffInversTrig * -1;

        }

        private void xInverse_CheckedChanged(object sender, EventArgs e)
        {
            xInversTrig = xInversTrig * -1;
        }

        private void yInverse_CheckedChanged(object sender, EventArgs e)
        {
            yInversTrig = yInversTrig * -1;
        }

        private void zInverse_CheckedChanged(object sender, EventArgs e)
        {
            zInversTrig = zInversTrig * -1;
        }

        private void xOffInverse_CheckedChanged(object sender, EventArgs e)
        {
            xOffInversTrig = xOffInversTrig * -1;
        }

        private void yOffInverse_CheckedChanged(object sender, EventArgs e)
        {
            yOffInversTrig = yOffInversTrig * -1;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.InitialDirectory = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\";
            saveFileDialog.Filter = "xml Files (*.xml)|*.xml";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Settings settings = new Settings();
                settings.XAxis = xAxisKey.Text;
                settings.YAxis = yAxisKey.Text;
                settings.ZAxis = zAxisKey.Text;
                settings.XOffset = xAxisOffsetKey.Text;
                settings.YOffset = yAxisOffsetKey.Text;
                settings.ResetOffset = resetOffsetKey.Text;
                settings.XInvers = xInverse.Checked;
                settings.YInvers = yInverse.Checked;
                settings.ZInvers = zInverse.Checked;
                settings.XOffInvers = xInverse.Checked;
                settings.YOffInvers = yInverse.Checked;
                settings.SwCh5 = swCh5Key.Text;
                settings.SwCh6 = swCh6Key.Text;
                settings.SwCh7 = swCh7Key.Text;
                settings.SwCh8 = swCh8Key.Text;
                settings.SwCh9 = swCh9Key.Text;
                settings.SwCh10 = swCh10Key.Text;
                settings.SwCh11 = swCh11Key.Text;
                settings.BtCh12 = btCh12Key.Text;
                settings.BtCh13 = btCh13Key.Text;
                settings.BtCh14 = btCh14Key.Text;
                settings.BtCh15 = btCh15Key.Text;

                settings.BUGX = bUGX.Text;
                settings.BLGX = bLGX.Text;
                settings.BUGY = bUGY.Text;
                settings.BLGY = bLGY.Text;
                settings.BURX = bURX.Text;
                settings.BLRX = bLRX.Text;
                settings.BURY = bURY.Text;
                settings.BLRY = bLRY.Text;
                settings.BUMGY = bUMGY.Text;
                settings.BLMGY = bLMGY.Text;
                settings.BUMRY = bUMRY.Text;
                settings.BLMRY = bLMRY.Text;
                settings.OffsetX = offsetX.Text;
                settings.OffsetY = offsetY.Text;
                settings.UniverseBox = universeBox.Text;
                settings.MoveSpeed = moveSpeed.Text;
                settings.ZTPG = zTPG.Text;
                settings.ZTPR = zTPR.Text;


                xmlSave.SaveData(settings, saveFileDialog.FileName);
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            xmlLoad<Settings> loadSettings = new xmlLoad<Settings>();

            openFileDialog.InitialDirectory = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\";
            openFileDialog.Filter = "xml Files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                settings = loadSettings.LoadData(openFileDialog.FileName);
                xAxisKey.Text = settings.XAxis;
                yAxisKey.Text = settings.YAxis;
                zAxisKey.Text = settings.ZAxis;
                xAxisOffsetKey.Text = settings.XOffset;
                yAxisOffsetKey.Text = settings.YOffset;
                resetOffsetKey.Text = settings.ResetOffset;
                xInverse.Checked = settings.XInvers;
                yInverse.Checked = settings.YInvers;
                zInverse.Checked = settings.ZInvers;
                xInverse.Checked = settings.XOffInvers;
                yInverse.Checked = settings.YOffInvers;
                swCh5Key.Text = settings.SwCh5;
                swCh6Key.Text = settings.SwCh6;
                swCh7Key.Text = settings.SwCh7;
                swCh8Key.Text = settings.SwCh8;
                swCh9Key.Text = settings.SwCh9;
                swCh10Key.Text = settings.SwCh10;
                swCh11Key.Text = settings.SwCh11;
                btCh12Key.Text = settings.BtCh12;
                btCh13Key.Text = settings.BtCh13;
                btCh14Key.Text = settings.BtCh14;
                btCh15Key.Text = settings.BtCh15;

                bUGX.Text = settings.BUGX;
                bLGX.Text = settings.BLGX;
                bUGY.Text = settings.BUGY;
                bLGY.Text = settings.BLGY;
                bURX.Text = settings.BURX;
                bLRX.Text = settings.BLRX;
                bURY.Text = settings.BURY;
                bLRY.Text = settings.BLRY;
                bUMGY.Text = settings.BUMGY;
                bLMGY.Text = settings.BLMGY;
                bUMRY.Text = settings.BUMRY;
                bLMRY.Text = settings.BLMRY;
                offsetX.Text = settings.OffsetX;
                offsetY.Text = settings.OffsetY;
                universeBox.Text = settings.UniverseBox;
                moveSpeed.Text = settings.MoveSpeed;
                zTPG.Text = settings.ZTPG;
                zTPR.Text = settings.ZTPR;
            }
        }

    }
    public class xmlSave
    {
        public static void SaveData(object IClass, string filename)
        {
            StreamWriter writer = null;
            try
            {
                XmlSerializer xml = new XmlSerializer((IClass.GetType()));
                writer = new StreamWriter(filename);
                xml.Serialize(writer, IClass);
            }
            finally
            {
                if (writer != null) writer.Close();
                writer = null;
            }
        }
    }
    public class xmlLoad<T>
    {
        public static Type type;

        public xmlLoad()
        {
            type = typeof(T);
        }

        public T LoadData(string filename)
        {
            T result;
            XmlSerializer xml = new XmlSerializer(type);
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            result = (T)xml.Deserialize(fs);
            fs.Close();
            return result;
        }
    }
    public class Settings
    {
        private string xAxis;
        private string yAxis;
        private string zAxis;
        private string xOffset;
        private string yOffset;
        private string resetOffset;
        private bool xInvers;
        private bool yInvers;
        private bool zInvers;
        private bool xOffInvers;
        private bool yOffInvers;
        private string swCh5;
        private string swCh6;
        private string swCh7;
        private string swCh8;
        private string swCh9;
        private string swCh10;
        private string swCh11;
        private string btCh12;
        private string btCh13;
        private string btCh14;
        private string btCh15;
        private string bUGX;
        private string bLGX;
        private string bUGY;
        private string bLGY;
        private string bURX;
        private string bLRX;
        private string bURY;
        private string bLRY;
        private string bUMGY;
        private string bLMGY;
        private string bUMRY;
        private string bLMRY;
        private string offsetX;
        private string offsetY;
        private string universeBox;
        private string moveSpeed;
        private string zTPG;
        private string zTPR;

        public string XAxis { get; set; }
        public string YAxis { get; set; }
        public string ZAxis { get; set; }
        public string XOffset { get; set; }
        public string YOffset { get; set; }
        public string ResetOffset { get; set; }
        public bool XInvers { get; set; }
        public bool YInvers { get; set; }
        public bool ZInvers { get; set; }
        public bool XOffInvers { get; set; }
        public bool YOffInvers { get; set; }
        public string SwCh5 { get; set; }
        public string SwCh6 { get; set; }
        public string SwCh7 { get; set; }
        public string SwCh8 { get; set; }
        public string SwCh9 { get; set; }
        public string SwCh10 { get; set; }
        public string SwCh11 { get; set; }
        public string BtCh12 { get; set; }
        public string BtCh13 { get; set; }
        public string BtCh14 { get; set; }
        public string BtCh15 { get; set; }
        public string BUGX { get; set; }
        public string BLGX { get; set; }
        public string BUGY { get; set; }
        public string BLGY { get; set; }
        public string BURX { get; set; }
        public string BLRX { get; set; }
        public string BURY { get; set; }
        public string BLRY { get; set; }
        public string BUMGY { get; set; }
        public string BLMGY { get; set; }
        public string BUMRY { get; set; }
        public string BLMRY { get; set; }
        public string OffsetX { get; set; }
        public string OffsetY { get; set; }
        public string UniverseBox { get; set; }
        public string MoveSpeed { get; set; }
        public string ZTPG { get; set; }
        public string ZTPR { get; set; }
    }
}
