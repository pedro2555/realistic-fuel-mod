/*
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

Further developments done by Sandakelum <sandakelum2009@gmail.com>
with the permission of original author, Pedro Rodrigues.
*/

// References
using GTA;
using SlimDX.XInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

// Namespace
namespace FuelScript
{
    // Main Class
    public class FuelScript : Script
    {
        /// <summary>
        /// Primary function and the constructor.
        /// RELEASE WARNING, SlimDX.dll should be placed on GTA root folder, NOT the scripts folder.
        /// </summary>
        public FuelScript()
        {
            // Get the file version from the assembled DLL.
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Game.InstallFolder + "\\scripts\\FuelScript.net.dll");
            string version = fvi.FileVersion;

            // Script command functions...
            GUID = new Guid("3583e09d-6c44-4820-85e9-93926307d4f8");

            // A different GUID specified by player in config file?
            string extGuid = Settings.GetValueString("GUID", "MISC", "").Trim();
            if (extGuid != "")
            {
                ///
                /// Usage example:
                /// SendScriptCommand(new Guid("3583e09d-6c44-4820-85e9-93926307d4f8"), "SetCurrentFuel", 100.0f);
                /// Still missing documentation on how to use this for what, initially was intended to implement the car saving feature externally.
                ///
                BindScriptCommand("GetCurrentFuel", new ScriptCommandDelegate(SendCurrentFuel));
                BindScriptCommand("GetCurrentFuelPercentage", new ScriptCommandDelegate(SendCurrentFuelPercentage));
                BindScriptCommand("GetCurrentDrain", new ScriptCommandDelegate(SendCurrentDrain));
                BindScriptCommand("GetCurrentFuelBottles", new ScriptCommandDelegate(SendCurrentFuelBottles));
                BindScriptCommand("SetCurrentFuel", new ScriptCommandDelegate(SetCurrentFuel));
                BindScriptCommand("SetCurrentFuelPercentage", new ScriptCommandDelegate(SetCurrentFuelPercentage));
                BindScriptCommand("SetVehicleFuel", new ScriptCommandDelegate(SetVehicleFuel));
                BindScriptCommand("SetVehicleFuelPercentage", new ScriptCommandDelegate(SetVehicleFuelPercentage));
                ExtScriptGUID = new Guid(extGuid);
            }
            // No GUID?
            else
            {
                ExtScriptGUID = Guid.Empty;
            }

            // Hook the ticking function.
            this.Interval = 1000;
            this.Tick += new EventHandler(FuelScript_Tick);

            // Start a new log session by putting this dashed line so we can easily identify it
            Log("FuelScript", "NEW GAME SESSION STARTED ON " + System.DateTime.Now + " - GTA IV");

            // Then log the rest of bla blas...
            Log("FuelScript", "Realistic Fuel Mod " + version + " for GTA IV loaded under GTA IV " + Game.Version.ToString() + " successfully.");
            Log("FuelScript", "Modified and further development done by Sandakelum (sandakelum2009@gmail.com)");
            Log("FuelScript", "Based on Ultimate Fuel Script v2.1 (https://code.google.com/p/ultimate-fuel-script)");
            Log("FuelScript", "Realistic Fuel Mod " + version + " running under: " + getOSInfo());
            Log("FuelScript", "Realistic Fuel Mod " + version + " found dsound.dll " + ((File.Exists(Game.InstallFolder + "\\dsound.dll")) ? "present" : "not present") + ", xlive.dll " + ((File.Exists(Game.InstallFolder + "\\xlive.dll")) ? "present" : "not present") + " and SlimDX.dll " + ((File.Exists(Game.InstallFolder + "\\SlimDX.dll")) ? "present." : "not present."));

            Log("FuelScript", "Loading settings file: FuelScripts.ini...");

            // Load the settings file.
            SettingsFile.Open("FuelScript.ini");
            Settings.Load();

            // If it made this long, the settings file must be loaded without an error.
            Log("FuelScript", "Settings file: FuelScripts.ini is loaded.");

            // Set as not refueling when the script is starting.
            Refueling = false;

            // Log as reading the settings file.
            Log("FuelScript", "Reading settings file: FuelScripts.ini...");

            // Show the script status.
            if (Settings.GetValueBool("STARTUPTEXT", "TEXTS", true))
            {
                Game.DisplayText("Realistic Fuel Mod " + version + " for GTA IV has loaded\nYou got " + (MaxFuelBottles - UsedFuelBottles) + " free emergency fuel bottles.", 10000);
            }

            // Log("FuelScript", "Selecting the fuel display panel mode from settings file...");

            // Defualt display type is: CLASSIC.
            switch (Settings.GetValueString("MODE", "DASHBOARD", "CLASSIC").ToUpper().Trim())
            {
                // Developer mode display panel.
                case "DEV":
                    this.PerFrameDrawing += new GraphicsEventHandler(FuelScript_PerFrameDrawing_devMode);
                    // Bind Q to reload settings file.
                    this.BindKey(Keys.Q, Settings.Load);
                    break;

                // Digital mode display panel (with digits).
                case "DIGITAL":
                    this.PerFrameDrawing += new GraphicsEventHandler(FuelScript_PerFrameDrawing_digitalMode);
                    break;

                // Classical mode display panel (with meters and several digits).
                // Recommended mode for the public releases.
                case "CLASSIC":
                    this.PerFrameDrawing += new GraphicsEventHandler(FuelScript_PerFrameDrawing_classicMode);
                    break;
            }

            // Log which display panel mode was chosen by the player.
            Log("FuelScript", "Fuel display panel has been selected as: " + Settings.GetValueString("MODE", "DASHBOARD", "CLASSIC").ToUpper().Trim() + " display mode.");

            // Bind the keydown function.
            this.KeyDown += new GTA.KeyEventHandler(FuelScript_KeyDown);

            // Bind the keyup function.
            this.KeyUp += new GTA.KeyEventHandler(FuelScript_KeyUp);

            // Fuel meter dashboard location, X, Y and W (width).
            Dashboard = new PointF(Settings.GetValueFloat("X", "DASHBOARD", 0.0f), Settings.GetValueFloat("Y", "DASHBOARD", 0.0f));
            GaugeWidth = Settings.GetValueFloat("W", "DASHBOARD", 0.11f);

            // Speed multipier if needed.
            SpeedMultiplier = (Settings.GetValueString("SPEED", "MISC", "KPH").ToUpper().Trim() == "KPH") ? 3.6f : 2.23693629f;

            // Log key mappings for diagnostics (not really necessary, whatever).
            Log("FuelScript", "Settings: Refuel Key - " + Settings.GetValueKey("REFUELKEY", "KEYS", Keys.E) + ", Bottle Use Key - " + Settings.GetValueKey("BOTTLEUSEKEY", "KEYS", Keys.U) + ", Bottle Buy Key - " + Settings.GetValueKey("BOTTLEBUYKEY", "KEYS", Keys.B) + ".");

            // Select the input method.
            // Is he chosen to use a GamePad? If so SlimDX.dll exists?
            if (Settings.GetValueBool("GAMEPAD", "MISC", false) && File.Exists(Game.InstallFolder + "\\SlimDX.dll"))
            {
                // OK, here you go with the GamePad.
                GamePad = new Controller(UserIndex.One);
                Log("FuelScript", "Selected active controller type as: GAMEPAD.");
            }
            // Oops, either player chosen to use Keyboard or he don't have SlimDX.dll in GTA IV directory, which is required.
            else
            {
                // Go with the keyboard, it always better, right?
                GamePad = null;
                Log("FuelScript", "Selected active controller type as: KEYBOARD.");
            }

            // No... no... no it's not in reserve.
            this.OnReserve = false;

            // Classic mode meter digits styles...
            // FuelMeterFont.Color = ColorIndex.SecuricorLightGray;
            FuelMeterFont.Effect = FontEffect.Edge;
            FuelMeterFont.EffectSize = 1;
            FuelMeterFont.EffectColor = ColorIndex.Black;

            // Set the beta watermark PNG file.
            if (Settings.GetValueBool("BETA", "RELEASE", false))
            {
                BetaMark = Resources.GetTexture("beta_mark.png");
            }

            /// <summary>
            /// Loads all the stations.
            /// There are 3 types of stations: STATION, HELISTATION, BOATSTATION. 
            /// Each type can have up to 254 stations.
            /// The first identifier is 1, the last is 255.
            /// The identifiers must be consecutive (1, 2, 3, 4. NOT 1, 3, 5)
            /// </summary>
            #region Load Dueling Stations
            try
            {
                // Log as placing blips...
                Log("FuelScript", "Placing fueling station blips on the map...");

                // First time basic values for stations counters.
                int stationsCount = 0;
                int carStations = 0;
                int heliStations = 0;
                int boatStations = 0;

                // Fuel stations for cars and bikes are enabled?
                if (Settings.GetValueBool("CARS", "MISC", true))
                {
                    // Load stations...
                    for (byte i = 1; i <= Byte.MaxValue; i++)
                    {
                        // Blips should be placed inside the map. Validate them.
                        Vector3 loc = Settings.GetValueVector3("LOCATION", "STATION" + i, new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                        if (loc.X == -123456789.0987654321f && loc.Y == -123456789.0987654321f && loc.Z == -123456789.0987654321f)
                            break;
                        // OK to proceed...
                        else
                        {
                            // Add a blip...
                            Blip b = GTA.Blip.AddBlip(loc);
                            // Choose the icon...
                            b.Icon = (BlipIcon)79;
                            // Set a name...
                            b.Name = (Settings.GetValueString("NAME", "STATION" + i, "Fuel Station").ToUpper().Trim().Length > 30) ? Settings.GetValueString("NAME", "STATION" + i, "Fuel Station").ToUpper().Trim().Substring(0, 29) : Settings.GetValueString("NAME", "STATION" + i, "Fuel Station").ToUpper().Trim();
                            // Display only in map...
                            b.Display = BlipDisplay.MapOnly;
                            // It's ours...
                            b.Friendly = true;
                            // Auto set route?
                            b.RouteActive = false;
                            // Minimap only.
                            b.ShowOnlyWhenNear = true;

                            // Log("FuelScript", "Blip placed for: Cars, Station: " + Settings.GetValueString("NAME", "STATION" + i, "Unknown") + " Fueling Station " + i + ".");
                        }

                        // Stations increment.
                        stationsCount = stationsCount + 1;

                        // Car stations increment.
                        carStations = carStations + 1;
                    }
                }

                // Fueling stations for Helicopters are enabled?
                if (Settings.GetValueBool("HELIS", "MISC", true))
                {
                    // Load stations...
                    for (byte i = 1; i <= Byte.MaxValue; i++)
                    {
                        // Blips should be placed inside the map. Validate them.
                        Vector3 loc = Settings.GetValueVector3("LOCATION", "HELISTATION" + i, new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                        if (loc.X == -123456789.0987654321f && loc.Y == -123456789.0987654321f && loc.Z == -123456789.0987654321f)
                            break;
                        // OK to proceed...
                        else
                        {
                            // Add a blip...
                            Blip b = GTA.Blip.AddBlip(loc);
                            // Choose the icon...
                            b.Icon = (BlipIcon)56;
                            // Set a name...
                            b.Name = (Settings.GetValueString("NAME", "HELISTATION" + i, "Fuel Station").ToUpper().Trim().Length > 30) ? Settings.GetValueString("NAME", "HELISTATION" + i, "Fuel Station").ToUpper().Trim().Substring(0, 29) : Settings.GetValueString("NAME", "HELISTATION" + i, "Fuel Station").ToUpper().Trim();
                            // Display only in map...
                            b.Display = BlipDisplay.MapOnly;
                            // It's ours...
                            b.Friendly = true;
                            // Auto set route?
                            b.RouteActive = false;
                            // Minimap only...
                            b.ShowOnlyWhenNear = true;

                            // Log("FuelScript", "Blip placed for: Helicopters, Station: " + Settings.GetValueString("NAME", "STATION" + i, "Unknown") + " Fueling Station " + i + ".");
                        }

                        // Stations increment.
                        stationsCount = stationsCount + 1;

                        // Helicopter stations increment.
                        heliStations = heliStations + 1;
                    }
                }

                // Fueling stations for boats are enabled?
                if (Settings.GetValueBool("BOATS", "MISC", true))
                {
                    // Load stations...
                    for (byte i = 1; i <= Byte.MaxValue; i++)
                    {
                        // Blips should be placed inside the map. Validate them.
                        Vector3 loc = Settings.GetValueVector3("LOCATION", "BOATSTATION" + i, new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                        if (loc.X == -123456789.0987654321f && loc.Y == -123456789.0987654321f && loc.Z == -123456789.0987654321f)
                            break;
                        // OK to proceed...
                        else
                        {
                            // Add a blip...
                            Blip b = GTA.Blip.AddBlip(loc);
                            // Choose an icon...
                            b.Icon = (BlipIcon)48;
                            // Set a name...
                            b.Name = (Settings.GetValueString("NAME", "BOATSTATION" + i, "Fuel Station").ToUpper().Trim().Length > 30) ? Settings.GetValueString("NAME", "BOATSTATION" + i, "Fuel Station").ToUpper().Trim().Substring(0, 29) : Settings.GetValueString("NAME", "BOATSTATION" + i, "Fuel Station").ToUpper().Trim();
                            // Display only in map...
                            b.Display = BlipDisplay.MapOnly;
                            // It's ours...
                            b.Friendly = true;
                            // Auto set route?
                            b.RouteActive = false;
                            // Minimap only...
                            b.ShowOnlyWhenNear = true;

                            // Log("FuelScript", "Blip placed for: Boats, Station: " + Settings.GetValueString("NAME", "STATION" + i, "Unknown") + " Fueling Station " + i + ".");
                        }

                        // Stations increment.
                        stationsCount = stationsCount + 1;

                        // Boat stations increment.
                        boatStations = boatStations + 1;
                    }
                }

                // Log how much fuel stations has been found...
                Log("FuelScript", "Finished placing blips: " + stationsCount + " blips placed. " + carStations + " car, " + heliStations + " helicopter and " + boatStations + " boat stations.");

                // The outro? Perhaps?
                // Game.DisplayText("Based on the source of Ultimate Fuel Script v2.1", 3000);
            }
            catch (Exception crap) { Log("ERROR: FuelScript", crap.Message); }
            #endregion
        }

        #region Variables and Properties
        /// <summary>
        /// Where to send data
        /// </summary>
        Guid ExtScriptGUID;
        /// <summary>
        /// Fuel meter styled font
        /// </summary>
        private GTA.Font FuelMeterFont = new GTA.Font(0.030f, FontScaling.ScreenUnits);
        /// <summary>
        /// Determines if the CurrentVehicle as already entered the reserve level.
        /// </summary>
        private bool OnReserve;
        /// <summary>
        /// Used for classic mode only.
        /// </summary>
        private float GaugeWidth;
        /// <summary>
        /// To keep track of the Flashinging sequence in reserve levels, this can probably be changed to a lower allocation later
        /// </summary>
        private int Flashing = 0;
        /// <summary>
        /// mps to knots
        /// </summary>
        private const float Knots = 1.94384449f;
        /// <summary>
        /// Determines if speed is shown in KPH or MPH
        /// </summary>
        private float SpeedMultiplier;
        /// <summary>
        /// The location of the dash board
        /// </summary>
        private PointF Dashboard;
        /// <summary>
        /// Holds the last vehicle the player has driven
        /// </summary>
        private Vehicle LastVehicle;
        /// <summary>
        /// Returns true if the player is refueling.
        /// </summary>
        private bool Refueling;
        /// <summary>
        /// Used to debt to the total money from the player's money value.
        /// </summary>
        private float RefuelAmount;
        /// <summary>
        /// Only used in devMode
        /// </summary>
        private float DrainSpeed;
        /// <summary>
        /// Current game, if aplicable.
        /// </summary>
        private Controller GamePad;
        /// <summary>
        /// Station names at the config file
        /// </summary>
        private string StationName;
        /// <summary>
        /// Keeps track of fuel bottles used
        /// </summary>
        private int UsedFuelBottles = 2;
        /// <summary>
        /// How much times player can use emergency reserved fuel bottles
        /// </summary>
        private int MaxFuelBottles = 5;
        /// <summary>
        /// How much one fuel bottle cost to refil it
        /// </summary>
        private float FuelBottleCost = 129.99f;
        /// <summary>
        /// Beta Watermark Texture (embedded)
        /// </summary>
        GTA.Texture BetaMark;
        /// <summary>
        /// Alias for Player.Character.CurrentVehicle
        /// </summary>
        private Vehicle CurrentVehicle
        { get { return (Player.Character.isInVehicle()) ? Player.Character.CurrentVehicle : null; } }
        #endregion

        #region Methods and Functions
        /// <summary>
        /// Send current vehicle's fuel value, use command 'GetCurrentFuel'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void SendCurrentFuel(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                if (ExtScriptGUID != Guid.Empty && Player.Character.isInVehicle())
                    SendScriptCommand(ExtScriptGUID, "CurrentFuel", CurrentVehicle.Metadata.Fuel);
            }
            catch (Exception crap) { Log("ERROR: SendCurrentFuel", crap.Message); }
        }
        /// <summary>
        /// Send current vehicle's fuel value as percentage of maximum capacity, use command 'GetCurrentFuelPercentage'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void SendCurrentFuelPercentage(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                float FuelPercentage = (Convert.ToInt32(CurrentVehicle.Metadata.Fuel) * 100) / Convert.ToInt32(CurrentVehicle.Metadata.MaxTank);
                if (ExtScriptGUID != Guid.Empty && Player.Character.isInVehicle())

                    SendScriptCommand(ExtScriptGUID, "CurrentFuelPercentage", Convert.ToInt32((float)FuelPercentage).ToString("00"));
            }
            catch (Exception crap) { Log("ERROR: SendCurrentFuelPercentage", crap.Message); }
        }
        /// <summary>
        /// Send current vehicle's fuel drain, use command 'SendCurrentDrain'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void SendCurrentDrain(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                if (ExtScriptGUID != Guid.Empty && Player.Character.isInVehicle())
                    SendScriptCommand(ExtScriptGUID, "CurrentDrain", this.DrainSpeed);
            }
            catch (Exception crap) { Log("ERROR: SendCurrentDrain", crap.Message); }
        }
        /// <summary>
        /// Send current vehicle's fuel drain, use command 'GetCurrentFuelBottles'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void SendCurrentFuelBottles(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                if (ExtScriptGUID != Guid.Empty && Player.Character.isInVehicle())
                    SendScriptCommand(ExtScriptGUID, "CurrentFuelBottles", (this.MaxFuelBottles - this.UsedFuelBottles));
            }
            catch (Exception crap) { Log("ERROR: SendCurrentFuelBottles", crap.Message); }
        }
        /// <summary>
        /// Sets the current fuel level of the current car with the value specified by Parameter[0], this value must be parseable to float.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void SetCurrentFuel(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                if (Player.Character.isInVehicle())
                {
                    float newFuelValue = Parameter.Convert<float>(0);
                    CurrentVehicle.Metadata.Fuel = newFuelValue;
                }
            }
            catch (Exception crap) { Log("ERROR: SendCurrentFuel", crap.Message); }
        }
        /// <summary>
        /// Sets the current fuel level of the current car with the percentage value specified by Parameter[0], this value must be parseable to float.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void SetCurrentFuelPercentage(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                if (Player.Character.isInVehicle())
                {
                    float FuelPercentage = Parameter.Convert<float>(0);
                    float newFuelValue = (FuelPercentage * CurrentVehicle.Metadata.MaxTank) / 100;
                    CurrentVehicle.Metadata.Fuel = newFuelValue;
                }
            }
            catch (Exception crap) { Log("ERROR: SetCurrentFuelPercentage", crap.Message); }
        }
        /// <summary>
        /// Sets the current fuel level of the car specified by Parameter[1] with the value specified by Parameter[0], this value must be parseable to float.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void SetVehicleFuel(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                Vehicle v = Parameter.Convert<Vehicle>(1);
                float newFuelValue = Parameter.Convert<float>(0);
                v.Metadata.Fuel = newFuelValue;
            }
            catch (Exception crap) { Log("ERROR: SetVehicleFuel", crap.Message); }
        }
        /// <summary>
        /// Sets the current fuel level of the car specified by Parameter[1] with the percentage value specified by Parameter[0], this value must be parseable to float.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void SetVehicleFuelPercentage(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                Vehicle v = Parameter.Convert<Vehicle>(1);
                float FuelPercentage = Parameter.Convert<float>(0);
                float newFuelValue = (FuelPercentage * CurrentVehicle.Metadata.MaxTank) / 100;
                v.Metadata.Fuel = newFuelValue;
            }
            catch (Exception crap) { Log("ERROR: SetVehicleFuelPercentage", crap.Message); }
        }
        /// <summary>
        /// Get os name and SP
        /// </summary>
        /// <returns></returns>
        string getOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else
                            operatingSystem = "7";
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }
        /// <summary>
        /// Get OS architecture in use.
        /// </summary>
        /// <returns></returns>
        int getOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }
        /// <summary>
        /// Saves an exception's message with the current date and time, and the method that originated it.
        /// </summary>
        /// <param name="methodName">The method that originated it</param>
        /// <param name="message">The exception's message</param>
        private void Log(string methodName, string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message) && Settings.GetValueBool("LOG", "MISC", true))
                {
                    using (StreamWriter streamWriter = File.AppendText(Application.StartupPath + "\\scripts\\FuelScript.log"))
                    {
                        streamWriter.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff tt") + ": " + methodName + "() - " + message);
                        streamWriter.Close();
                    }
                }
            }
            catch { }
            finally { if (Settings.GetValueString("MODE", "DASHBOARD", "CLASSIC").ToUpper().Trim() == "DEV") Game.DisplayText("Check log - " + message, 3000); }

        }
        /// <summary>
        /// Show message with modern formatted text.
        /// </summary>
        /// <param name="message">Message you want to show</param>
        /// <param name="time">How much it should be on screen</param>
        internal void ShowMessage(string message, int time)
        {
            // Call out for the native function.
            GTA.Native.Function.Call("PRINT_STRING_WITH_LITERAL_STRING_NOW", "STRING", message, time, true);
        }
        /// <summary>
        /// Use ONLY when player is in vehicle!
        /// Retrieves the values of DRAIN, MAXTANK and RESERVE, from the loaded ini file. First looks for the car hash, then for the car name, then defaults.
        /// Generates a random amount of fuel, if an amount can't be find under CurrentVehicle.Metadata.Fuel.
        /// Calculates the amount of fuel to be drain and drains it, and prevents the car engine from running when out of fuel.
        /// </summary>
        private void DrainFuel()
        {
            try
            {
                // Set station name right now!
                if (Player.Character.isInVehicle() && Player == CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver))
                {
                    StationName = (CurrentVehicle.Model.isBoat) ? "BOATSTATION" : (CurrentVehicle.Model.isHelicopter) ? "HELISTATION" : "STATION";
                }

                #region Vehicle Fuel Level Checker
                try { float f = CurrentVehicle.Metadata.Fuel; }
                catch
                {
                    // If it does not exists...
                    // First the ini file is checked for the vehicle's hash code, then for the vehicle's name.
                    if (CurrentVehicle.Model.isCar || CurrentVehicle.Model.isBike)
                    {
                        // Get max tank capacity
                        CurrentVehicle.Metadata.MaxTank = Settings.GetValueInteger("TANK", CurrentVehicle.GetHashCode().ToString(),
                        Settings.GetValueInteger("TANK", CurrentVehicle.Name, 100));

                        // Get draining speed
                        CurrentVehicle.Metadata.Drain = Settings.GetValueInteger("DRAIN", CurrentVehicle.GetHashCode().ToString(),
                        Settings.GetValueInteger("DRAIN", CurrentVehicle.Name, 10));

                        // Get reserved fuel capacity
                        CurrentVehicle.Metadata.Reserve = Settings.GetValueInteger("RESERVE", CurrentVehicle.GetHashCode().ToString(),
                        Settings.GetValueInteger("RESERVE", CurrentVehicle.Name, 10));

                        // Random it vehicle to vehicle
                        CurrentVehicle.Metadata.Fuel = (int)new Random().Next(CurrentVehicle.Metadata.Reserve + 1, CurrentVehicle.Metadata.MaxTank);
                    }
                    else if (CurrentVehicle.Model.isHelicopter || CurrentVehicle.Model.isBoat)
                    {
                        // Get max tank capacity
                        CurrentVehicle.Metadata.MaxTank = Settings.GetValueInteger("TANK", CurrentVehicle.GetHashCode().ToString(),
                        Settings.GetValueInteger("TANK", CurrentVehicle.Name, 200));

                        // Get draining speed
                        CurrentVehicle.Metadata.Drain = Settings.GetValueInteger("DRAIN", CurrentVehicle.GetHashCode().ToString(),
                        Settings.GetValueInteger("DRAIN", CurrentVehicle.Name, 20));

                        // Get reserved fuel capacity
                        CurrentVehicle.Metadata.Reserve = Settings.GetValueInteger("RESERVE", CurrentVehicle.GetHashCode().ToString(),
                        Settings.GetValueInteger("RESERVE", CurrentVehicle.Name, 20));

                        // Random it vehicle to vehicle
                        CurrentVehicle.Metadata.Fuel = (int)new Random().Next(CurrentVehicle.Metadata.Reserve + 1, CurrentVehicle.Metadata.MaxTank);
                    }

                }
                #endregion

                #region Fuel Draining and Vehicle Fuel Status
                // If Niko have fuel in the vehicle.
                if (CurrentVehicle.Metadata.Fuel > 0.0f)
                {
                    // Keep hazard lights turned off.
                    CurrentVehicle.HazardLightsOn = false;
                    // Draining enabled for cars and bikes?
                    if ((CurrentVehicle.Model.isCar || CurrentVehicle.Model.isBike) && CurrentVehicle.EngineRunning && Settings.GetValueBool("CARS", "MISC", true))
                    {
                        // CurrentVehicle.Metadata.Drain is a user defined constant, defaults to 20
                        DrainSpeed = CurrentVehicle.Metadata.Drain * CurrentVehicle.CurrentRPM / 100;
                        // increase consumption based on engine damage
                        DrainSpeed = DrainSpeed * ((1000 - CurrentVehicle.EngineHealth) / 1000) + DrainSpeed;
                        // actually remove the calculated value
                        CurrentVehicle.Metadata.Fuel -= DrainSpeed;
                        // avoid negative values
                        CurrentVehicle.Metadata.Fuel = (CurrentVehicle.Metadata.Fuel < 0.0f) ? 0.0f : CurrentVehicle.Metadata.Fuel;
                    }
                    // Draining enabled for helicopters?
                    else if (CurrentVehicle.Model.isHelicopter && CurrentVehicle.EngineRunning && Settings.GetValueBool("HELIS", "MISC", true))
                    {
                        // Note: 254.921568627451f
                        // Note: 0.2 + ((speed * 0.2) / 5)
                        // Only take in account speed when accelerate xor reverse key is pressed.

                        // GamePad disabled or unavailable? Use keyboard!
                        if (GamePad == null)
                            if (Game.isGameKeyPressed(GameKey.MoveForward))
                                DrainSpeed = (CurrentVehicle.Metadata.Drain * (.2f + ((CurrentVehicle.Speed * .2f) / 5.0f))) / 100.0f;
                            else
                                DrainSpeed = (CurrentVehicle.Metadata.Drain * .208f) / 100.0f;
                        // Use the GamePad if available.
                        else if (GamePad.GetState().Gamepad.RightTrigger > 0.0f)
                            DrainSpeed = CurrentVehicle.Metadata.Drain * (((GamePad.GetState().Gamepad.RightTrigger * 100.0f) / 255.0f) / 10000.0f);
                        // Just go with it already.
                        else
                            DrainSpeed = (CurrentVehicle.Metadata.Drain * .208f) / 100.0f;

                        // Calculate the draining speed also taking engine damage to an account.
                        DrainSpeed = DrainSpeed * ((1000 - CurrentVehicle.EngineHealth) / 1000.0f) + DrainSpeed;
                        CurrentVehicle.Metadata.Fuel -= DrainSpeed;
                        CurrentVehicle.Metadata.Fuel = (CurrentVehicle.Metadata.Fuel < .0f) ? .0f : CurrentVehicle.Metadata.Fuel;
                    }
                    // Draining enabled for boats?
                    else if (CurrentVehicle.Model.isBoat && CurrentVehicle.EngineRunning && Settings.GetValueBool("BOATS", "MISC", true))
                    {
                        // Note: 0.2 + ((speed * 0.2) / 5)
                        // Only take in account speed when accelerate xor reverse key is pressed.

                        // GamePad disabled or unavailable? Use keyboard!
                        if (GamePad == null)
                            if (Game.isGameKeyPressed(GameKey.MoveForward) ^ Game.isGameKeyPressed(GameKey.MoveBackward))
                                DrainSpeed = (CurrentVehicle.Metadata.Drain * (.2f + ((CurrentVehicle.Speed * .2f) / 5.0f))) / 100;
                            else
                                DrainSpeed = (CurrentVehicle.Metadata.Drain * .208f) / 100;
                        // Use the GamePad if available.
                        else
                            if (GamePad.GetState().Gamepad.RightTrigger > 0 ^ GamePad.GetState().Gamepad.LeftTrigger > 0)
                                DrainSpeed = (CurrentVehicle.Metadata.Drain * (.2f + ((CurrentVehicle.Speed * .2f) / 5.0f))) / 100;
                            else
                                DrainSpeed = (CurrentVehicle.Metadata.Drain * .208f) / 100;

                        // Calculate the draining speed also taking engine damage to an account.
                        DrainSpeed = DrainSpeed * ((1000 - CurrentVehicle.EngineHealth) / 1000) + DrainSpeed;
                        CurrentVehicle.Metadata.Fuel -= DrainSpeed;
                        CurrentVehicle.Metadata.Fuel = (CurrentVehicle.Metadata.Fuel < .0f) ? .0f : CurrentVehicle.Metadata.Fuel;
                    }

                    // Enter to reserved fuel
                    if (!OnReserve && CurrentVehicle.Metadata.Fuel <= CurrentVehicle.Metadata.Reserve && CurrentVehicle.EngineRunning && CurrentVehicle.Speed > 2.5f)
                    {
                        // Set as in reserve.
                        OnReserve = true;

                        // Let the player know.
                        if (Settings.GetValueBool("RESERVEDFUELTEXT", "TEXTS", true))
                        {
                            Game.DisplayText("Your vehicle is now running on reserved fuel.\n" + (((MaxFuelBottles - UsedFuelBottles) >= 1) ? "You have " + (MaxFuelBottles - UsedFuelBottles) + " emergency fuel bottle" + (((MaxFuelBottles - UsedFuelBottles) == 1) ? "" : "s") + " left." : "Drive to a refueling station quickly!"), 10000);
                        }

                        // Log the situation.
                        Log("DrainFuel", "Player entered to reserved fuel on vehicle: " + CurrentVehicle.Name.ToString() + " with " + CurrentVehicle.Metadata.Fuel + " fuel units and " + (MaxFuelBottles - UsedFuelBottles) + " emergency fuel bottle" + (((MaxFuelBottles - UsedFuelBottles) == 1) ? "" : "s") + " left.");
                    }
                    // If vehicle has fuel than reserved amount.
                    else if (CurrentVehicle.Metadata.Fuel > CurrentVehicle.Metadata.Reserve)
                    {
                        // Then it's not on reserve... that's obvious!
                        OnReserve = false;
                    }
                }
                // Oh... bad luck! No fuel?!
                else
                {
                    // Stop the engine immediately!
                    CurrentVehicle.EngineRunning = false;

                    // Turn hazard lights on to assist the traffic!
                    CurrentVehicle.HazardLightsOn = true;

                    // Set fuel level as zero for double sure.
                    CurrentVehicle.Metadata.Fuel = 0;

                    // Smoking a little maybe? (only if he isn't damaged too much)
                    CurrentVehicle.EngineHealth = (CurrentVehicle.EngineHealth > 100.0f) ? 100.0f : CurrentVehicle.EngineHealth;
                    CurrentVehicle.Metadata.NoFuelDamage = (bool)true;

                    // I don't know how to explain this line, hahhha... Let's say it's a big dynamic text?
                    // This is shown when the vehicle ran out of fuel.
                    if (Settings.GetValueBool("OUTOFFUELTEXT", "TEXTS", true))
                    {
                        // changing this with chained if statments will be way more understandable, I haven't been able to, very confusing.
                        Game.DisplayText(((MaxFuelBottles - UsedFuelBottles) >= 1) ? ((CurrentVehicle.Speed == 0.0f) ? "Press " + Settings.GetValueKey("BOTTLEUSEKEY", "KEYS", Keys.U) + " button to inject an emergency fuel bottle. " + ((((MaxFuelBottles - UsedFuelBottles) - 1) >= 1) ? "You have " + (MaxFuelBottles - UsedFuelBottles) + " fuel bottle" + (((MaxFuelBottles - UsedFuelBottles) == 1) ? "" : "s") + " left.\n" + ((UsedFuelBottles > 0) ? "Refilling your " + UsedFuelBottles + " empty fuel bottle" + ((UsedFuelBottles == 1) ? "" : "s") + " costs $" + (UsedFuelBottles * FuelBottleCost) + " for you ($" + FuelBottleCost + " each)" : "A used fuel bottle can be refilled again for $" + FuelBottleCost + " at fueling stations") + "." : "\nLast fuel bottle, find a refueling station quickly!") : "You ran out of fuel. " + (((MaxFuelBottles - UsedFuelBottles) >= 1) ? "You have " + (MaxFuelBottles - UsedFuelBottles) + " emergency fuel bottle" + (((MaxFuelBottles - UsedFuelBottles) == 1) ? "" : "s") + " left." : "No emerygency fuel bottles left.") + "\nWait until the vehicle stops and engine is idle.") : "You ran out of fuel. Sorry, but this vehicle cannot be started.");
                    }

                    // Log("DrainFuel", "Player ran out of fuel on vehicle: " + CurrentVehicle.Name.ToString() + " as " + CurrentVehicle.Metadata.Fuel + " fuel units and " + CurrentVehicle.Metadata.Reserve + " reserve units.");

                    // Mass angry when Niko lost a vehicle...
                    // NOTE: Something weired happening, exact thing runs. But last only around a second!
                    /*
                    if (CurrentVehicle.Metadata.Fuel == 0 && UsedFuelBottles == MaxFuelBottles)
                    {
                        Player.Character.SayAmbientSpeech("HIGH_FALL");
                        GTA.Native.Function.Call("SET_CAM_SHAKE", Game.DefaultCamera, true, 10);
                    }
                    */
                }

                // Is the player near a fueling station, then give him some help!
                if (isAtFuelStation() > -1)
                {
                    // Show currently owned cash so the player can decide whether to purchase fuel or not, or unit by unit.
                    GTA.Native.Function.Call("DISPLAY_CASH", true);

                    // Another big dynamic text which shows when player inside of a fueling station radius.
                    if (Settings.GetValueBool("FUELINGSTATIONTEXT", "TEXTS", true))
                    {
                        // changing this with chained if statments will be way more understandable, I haven't been able to, very confusing.
                        Game.DisplayText("Welcome to " + Settings.GetValueString("NAME", StationName + isAtFuelStation(), "Unknown") + " Fueling Station " + isAtFuelStation() + ". We offer fuel just for $" + Settings.GetValueFloat("PRICE", "STATION" + isAtFuelStation(), 6.99f) + " per litre.\nHold " + Settings.GetValueKey("REFUELKEY", "KEYS", Keys.E) + " button to purchase full tank fuel which costs $" + Convert.ToInt32(((CurrentVehicle.Metadata.MaxTank - CurrentVehicle.Metadata.Fuel) * Settings.GetValueFloat("PRICE", StationName + isAtFuelStation(), 6.99f))) + " at this moment." + (((MaxFuelBottles - UsedFuelBottles) < MaxFuelBottles) ? "\nPress " + Settings.GetValueKey("BOTTLEBUYKEY", "KEYS", Keys.B) + " to buy a fuel bottle for $" + FuelBottleCost + ". You can buy " + UsedFuelBottles + " more bottle" + ((UsedFuelBottles == 1) ? "" : "s") + "." : ""));
                    }

                    // Writing too much lines at the log is really annoying everytime you cross a square foot of a station!
                    // Log("DrainFuel", "Player entered to: " + Settings.GetValueString("NAME", station + isAtFuelStation(), "Unknown") + " Fueling Station " + isAtFuelStation() + " zone with vehicle: " + CurrentVehicle.Name.ToString() + ".");
                }

                #endregion
            }
            catch (Exception crap) { Log("ERROR: DrainFuel", crap.Message); }
        }
        /// <summary>
        /// Use if not sure if player is in vehicle.
        /// </summary>
        /// <returns></returns>
        private bool isAtFuelStationGeneric()
        {
            try
            {
                // Car and bike fueling stations
                for (int i = 1; i < 1000; i++)
                {
                    // Validate position.
                    Vector3 loc = Settings.GetValueVector3("LOCATION", "STATION" + i, new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                    if (loc.X == -123456789.0987654321f && loc.Y == -123456789.0987654321f && loc.Z == -123456789.0987654321f)
                        break;
                    // OK to proceed, calculate distance.
                    else if (loc.DistanceTo(Player.Character.Position) < Settings.GetValueFloat("RADIUS", "STATION" + i, 10))
                        return true;
                }

                // Boat fueling stations
                for (int i = 1; i < 1000; i++)
                {
                    // Validate position.
                    Vector3 loc = Settings.GetValueVector3("LOCATION", "BOATSTATION" + i, new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                    if (loc.X == -123456789.0987654321f && loc.Y == -123456789.0987654321f && loc.Z == -123456789.0987654321f)
                        break;
                    // OK to proceed, calculate distance.
                    else if (loc.DistanceTo(Player.Character.Position) < Settings.GetValueFloat("RADIUS", "BOATSTATION" + i, 10))
                        return true;
                }

                // Helicopter fueling stations
                for (int i = 1; i < 1000; i++)
                {
                    // Validate position.
                    Vector3 loc = Settings.GetValueVector3("LOCATION", "HELISTATION" + i, new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                    if (loc.X == -123456789.0987654321f && loc.Y == -123456789.0987654321f && loc.Z == -123456789.0987654321f)
                        break;
                    // OK to proceed, calculate distance.
                    else if (loc.DistanceTo(Player.Character.Position) < Settings.GetValueFloat("RADIUS", "HELISTATION" + i, 10))
                        return true;
                }

                // If everything gone wrong
                return false;
            }
            catch (Exception crap) { Log("ERROR: isAtFuelStationGeneric", crap.Message); return false; }
        }
        /// <summary>
        /// Use ONLY when player is in vehicle!
        ///
        /// Returns the station id, if the player is at any station valid for the vehicle type.
        /// </summary>
        /// <returns></returns>
        private int isAtFuelStation()
        {
            try
            {
                // Get the type of the fueling station according to the type of vehicle player is in.
                string toLookFor = (CurrentVehicle.Model.isHelicopter) ? "HELISTATION" : (CurrentVehicle.Model.isBoat) ? "BOATSTATION" : "STATION";
                for (int i = 1; i < 1000; i++)
                {
                    // Validate position.
                    Vector3 loc = Settings.GetValueVector3("LOCATION", toLookFor + i, new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                    if (loc.X == -123456789.0987654321f && loc.Y == -123456789.0987654321f && loc.Z == -123456789.0987654321f)
                        break;
                    // OK to proceed, calculate distance.
                    else if (loc.DistanceTo(Player.Character.Position) < Settings.GetValueFloat("RADIUS", "STATION" + i, 10))
                        return i;
                }
                return -1;
            }
            catch (Exception crap) { Log("ERROR: isAtFuelStation", crap.Message); return -1; }
        }
        /// <summary>
        /// Finishes the Refueling process.
        ///
        /// Debts the money from the player's money value, and allows the car to be started.
        /// </summary>
        private void FinishRefuel()
        {
            try
            {
                // Should player be refueling?
                if (Refueling)
                {
                    // If player is refueling from a public fueling station, deduct the cost.
                    if (Settings.GetValueInteger("STARS", StationName + isAtFuelStation(), 0) == 0)
                    {
                        Player.Money -= Convert.ToInt32((RefuelAmount * Settings.GetValueFloat("PRICE", StationName + isAtFuelStation(), 6.99f)));
                    }

                    // If player should get a wanted level by refueling vehicle with a goverment property.
                    Player.WantedLevel = (Settings.GetValueInteger("STARS", StationName + isAtFuelStation(), 0) > 0 && Player.WantedLevel < Settings.GetValueInteger("STARS", StationName + isAtFuelStation(), 0)) ? Settings.GetValueInteger("STARS", StationName + isAtFuelStation(), 0) : Player.WantedLevel;

                    // Set as not refeuling... again...
                    Refueling = false;

                    // Startup engine and don't hotwire.
                    CurrentVehicle.EngineRunning = true;
                    CurrentVehicle.NeedsToBeHotwired = false;

                    // CurrentVehicle.HazardLightsOn = false;

                    // Turn on lights if required.
                    GTA.Native.Function.Call("SET_VEH_LIGHTS", CurrentVehicle, 2);

                    // Display the balance.
                    GTA.Native.Function.Call("DISPLAY_CASH", true);

                    // In case if the player made it to the fueling station even when no fuel!
                    if (CurrentVehicle.EngineHealth <= 500.0f)
                    {
                        CurrentVehicle.EngineHealth = 1000.0f;
                    }

                    // Game.DisplayText("You've refueled vehicle with " + Convert.ToInt32(RefuelAmount) + " fuel units for $" + Convert.ToInt32((RefuelAmount * Settings.GetValueFloat("PRICE", station + isAtFuelStation(), 6.99f))) + " at " + Settings.GetValueString("NAME", station + isAtFuelStation(), "Unknown") + " Fueling Station " + isAtFuelStation() + ".", 10000);
                    Log("FinishRefuel", "Player refueled vehicle: " + CurrentVehicle.Name.ToString() + " with " + RefuelAmount + " fuel units for $" + Convert.ToInt32((RefuelAmount * Settings.GetValueFloat("PRICE", StationName + isAtFuelStation(), 6.99f))) + " at " + Settings.GetValueString("NAME", StationName + isAtFuelStation(), "Unknown") + " Fueling Station " + isAtFuelStation() + ".");

                    // Let the player know if the wanted level has been increased.
                    if (Settings.GetValueInteger("STARS", StationName + isAtFuelStation(), 0) > 0 && Player.WantedLevel < Settings.GetValueInteger("STARS", StationName + isAtFuelStation(), 0))
                    {
                        Log("FinishRefuel", "Wanted level for vehicle: " + CurrentVehicle.Name.ToString() + " by " + Settings.GetValueInteger("STARS", StationName + isAtFuelStation(), 0) + " stars by using " + Settings.GetValueString("NAME", StationName + isAtFuelStation(), "Unknown") + " Fueling Station " + isAtFuelStation() + ".");
                    }

                    // We currently show cash, and this also not showing for much time as the catalogue takes place.
                    // Game.DisplayText("You've refueled vehicle with " + Convert.ToInt32(RefuelAmount) + " fuel units for $" + Convert.ToInt32((RefuelAmount * Settings.GetValueFloat("PRICE", station + isAtFuelStation(), 6.99f))) + ((damageLowFuel) ? " and vehicle repaired." : "."), 10000);

                    RefuelAmount = 0.0f;
                }
            }
            catch (Exception crap) { Log("ERROR: FinishRefuel", crap.Message); }
        }
        /// <summary>
        /// Fills the fuel tank at 5 units per second in case of cars and bikes, fills at 10 units per second in case of boats and helis.
        /// </summary>
        private void ReFuel()
        {
            try
            {
                // Fill the tank of cars and bikes 5 unit per second, and for helicopters and boats fill tank 10 units per second.
                float unitsPerSecond = (CurrentVehicle.Model.isCar || CurrentVehicle.Model.isBike) ? 5 : 10;

                // Don't overrun maximum fuel tank capacity.
                if (CurrentVehicle.Metadata.Fuel >= CurrentVehicle.Metadata.MaxTank)
                {
                    // Take care of it.
                    CurrentVehicle.Metadata.Fuel = CurrentVehicle.Metadata.MaxTank;
                    FinishRefuel();
                }

                // If player tank is not full, he can refuel it.
                else
                {
                    // Calculate amount how much player costs for the fuel.
                    float amount = (CurrentVehicle.Metadata.Fuel + unitsPerSecond > CurrentVehicle.Metadata.MaxTank) ? CurrentVehicle.Metadata.MaxTank - CurrentVehicle.Metadata.Fuel : unitsPerSecond;

                    // If player doesn't have enough money to buy it.
                    if (Player.Money < (amount * Settings.GetValueFloat("PRICE", "STATION" + isAtFuelStation(), 6.99f)) + (RefuelAmount * Settings.GetValueFloat("PRICE", (CurrentVehicle.Model.isBoat) ? "BOATSTATION" : (CurrentVehicle.Model.isHelicopter) ? "HELISTATION" : "STATION" + isAtFuelStation(), 6.99f)))
                    {
                        // Player does not have any money
                        FinishRefuel();

                        // Inform the player about what happened.
                        if (Settings.GetValueBool("OUTOFFUNDSFUELTEXT", "TEXTS", true))
                        {
                            Game.DisplayText("Sorry, but you don't have enough money to refuel your vehicle.\nFuel unit costs $" + Settings.GetValueFloat("PRICE", "STATION" + isAtFuelStation(), 6.99f) + " here", 5000);
                        }

                        // Log it.
                        Log("Refuel", "Player couldn't refuel vehicle: " + CurrentVehicle.Name.ToString() + " due to insufficient money at " + Settings.GetValueString("NAME", StationName + isAtFuelStation(), "Unknown") + " Fueling Station " + isAtFuelStation() + ".");
                    }
                    // If player have enough money to buy it.
                    else
                    {
                        // Add the requested fuel to vehicle's tank and deduct the cost.
                        CurrentVehicle.Metadata.Fuel += amount;
                        RefuelAmount += amount;

                        // Currently it's not much useful and it's kind of annoying find repeated similar lines at log file
                        // Log("Refuel", "Player refueling vehicle: " + CurrentVehicle.Name.ToString() + " with " + amount + " more units at " + Settings.GetValueString("NAME", station + isAtFuelStation(), "Unknown") + " Fueling Station " + isAtFuelStation() + " and now have " + CurrentVehicle.Metadata.Fuel + " units.");
                    }
                }
            }
            catch (Exception crap) { Log("ERROR: ReFuel", crap.Message); }
        }

        #endregion

        #region Key Bindings
        /// <summary>
        /// Handles the REFUELKEY's up event behaviour.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FuelScript_KeyUp(object sender, GTA.KeyEventArgs e)
        {
            FinishRefuel();
        }
        /// <summary>
        /// Handles the REFUELKEY's down event behaviour.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FuelScript_KeyDown(object sender, GTA.KeyEventArgs e)
        {
            // If player presses REFUELKEY, default E
            if (e.Key == Settings.GetValueKey("REFUELKEY", "KEYS", Keys.E))
            {
                // If player is in vehicle at a fueling station and is not already refueling
                if (!Refueling && Player.Character.isInVehicle() && isAtFuelStation() > -1)
                {
                    // Set to refuel.
                    RefuelAmount = 0.0f;
                    Refueling = true;

                    // Let the player know.
                    if (Settings.GetValueBool("REFUELINGTEXT", "TEXTS", true))
                    {
                        Game.DisplayText("You're vehicle is now being refueled by the fueling station.\nHold the button until it reaches to the amount you would like to purchase.", 7500);
                    }

                    // Log as player using a fueling station.
                    Log("KeyDown", "Player is now using: " + Settings.GetValueString("NAME", StationName + isAtFuelStation(), "Unknown") + " Fueling Station " + isAtFuelStation() + ", which offers fuel for $" + Settings.GetValueFloat("PRICE", "STATION" + isAtFuelStation(), 6.99f) + " per unit.");
                }
            }
            // If player presses BOTTLEUSEKEY, default U
            else if (e.Key == Settings.GetValueKey("BOTTLEUSEKEY", "KEYS", Keys.U))
            {
                // If player ran out of fuel, and vehicle is stopped.
                if (CurrentVehicle.Metadata.Fuel == 0 && CurrentVehicle.Speed == 0.0f)
                {
                    // If player has at least one fuel bottle.
                    if ((MaxFuelBottles - UsedFuelBottles) >= 1)
                    {
                        // Say something as clue?
                        Player.Character.SayAmbientSpeech("START_CAR_PANIC");
                        Wait(2000);

                        // Start the repair by using the fuel bottle!
                        // Clear tasks...
                        Player.Character.Task.ClearAll();

                        // Focus on current tasks...
                        Player.Character.Task.AlwaysKeepTask = true;

                        // Get out of vehicle.
                        // If Niko is driving a Helicopter or a Boat we don't want to get him out to inject a fuel bottle, do we?
                        // That would kill Niko... lol, it could be fun though :D
                        // Added a fix for the crash when injecting fuel bottles to a bus by letting Niko do it inside!
                        if ((CurrentVehicle.Model.isCar || CurrentVehicle.Model.isBike) && CurrentVehicle.Name != "BUS")
                        {
                            Player.Character.Task.LeaveVehicle(CurrentVehicle, true);

                            // Let him know that Niko doing a magic!
                            if (Settings.GetValueBool("BOTTLEUSINGTEXT", "TEXTS", true))
                            {
                                Game.DisplayText("You're now using one of your emergency fuel bottles on this vehicle.", 10000);
                            }

                            // Wait until Niko got to the position.
                            while (Player.Character.isInVehicle())
                            {
                                Wait(500);
                            }

                            // Turn to the vehicle side, door side!
                            Player.Character.Task.TurnTo(LastVehicle.Position);
                            Wait(500);

                            // Do his magic!
                            Game.LocalPlayer.Character.Task.PlayAnimation(new AnimationSet("misstaxidepot"), "workunderbonnet", 4.0f);
                            Wait(6800);

                            // Repair the vehicle.
                            // Is the damage caused by low fuel running?
                            if (LastVehicle.Metadata.NoFuelDamage)
                            {
                                // If so, repair the engine, not visual damage!
                                LastVehicle.EngineHealth = 1000.0f;
                            }
                            // Is the damage caused by player's act?
                            else
                            {
                                // If so, repair few of the damage in engine, not visual damage!
                                LastVehicle.EngineHealth = (1000.0f - LastVehicle.EngineHealth) / 3;
                            }

                            // Give a little fuel capacity...
                            LastVehicle.Metadata.Fuel = LastVehicle.Metadata.Reserve + (LastVehicle.Metadata.MaxTank / 10);
                            // Not on reserve now...
                            OnReserve = false;

                            // Startup the engine.
                            LastVehicle.EngineRunning = true;
                            LastVehicle.HazardLightsOn = false;
                            Player.Character.Task.EnterVehicle(LastVehicle, VehicleSeat.Driver);
                        }
                        // If it a helicopter, boat or a bus...
                        // Inject the fuel bottle without getting off the vehicle
                        // For safety issues... Hahhha...
                        else
                        {
                            // Let him know that Niko doing a magic!
                            if (Settings.GetValueBool("BOTTLEUSINGTEXT", "TEXTS", true))
                            {
                                Game.DisplayText("You used one of your fuel bottles on this vehicle.", 5000);
                            }

                            // Repair the vehicle.
                            // Is the damage caused by low fuel running?
                            if (CurrentVehicle.Metadata.NoFuelDamage)
                            {
                                // If so, repair the engine, not visual damage!
                                CurrentVehicle.EngineHealth = 1000.0f;
                            }
                            // Is the damage caused by player's act?
                            else
                            {
                                // If so, repair few of the damage in engine, not visual damage!
                                CurrentVehicle.EngineHealth = (1000.0f - CurrentVehicle.EngineHealth) / 3;
                            }

                            // Give a little fuel capacity...
                            CurrentVehicle.Metadata.Fuel = CurrentVehicle.Metadata.Reserve + (CurrentVehicle.Metadata.MaxTank / 10);
                            // Not on reserve now...
                            OnReserve = false;

                            // Startup the engine.
                            CurrentVehicle.EngineRunning = true;
                            CurrentVehicle.HazardLightsOn = false;
                        }

                        // Relax a while...
                        Wait(2000);

                        // Let the player know...
                        // Game.DisplayText("You injected " + Convert.ToInt32(CurrentVehicle.Metadata.Fuel) + " litre(s) of fuel to your vehicle.", 6000);
                        Log("VehicleRepair", "Player injected " + Convert.ToInt32(CurrentVehicle.Metadata.Fuel) + " litre(s) of fuel for vehicle: " + CurrentVehicle.Name.ToString() + " with bottle " + (UsedFuelBottles + 1) + ".");

                        CurrentVehicle.Metadata.NoFuelDamage = (bool)false;

                        // Cost one fuel bottle...
                        UsedFuelBottles += 1;

                        // Hurry up, we wasted some time!
                        Wait(600);
                        Player.Character.SayAmbientSpeech("HURRY_UP");
                    }
                }
            }
            // If player presses BOTTLEBUYKEY, default B
            else if (e.Key == Settings.GetValueKey("BOTTLEBUYKEY", "KEYS", Keys.B))
            {
                // If player haven't exceeded max fuel bottles limit and player is in vehicle at a fueling station.
                if ((MaxFuelBottles - UsedFuelBottles) < MaxFuelBottles && Player.Character.isInVehicle() && isAtFuelStation() > -1)
                {
                    // Does the player have enough money to buy a fuel bottle?
                    if (Player.Money >= Convert.ToInt32(FuelBottleCost))
                    {
                        // Deduct from player's money.
                        Player.Money -= Convert.ToInt32(FuelBottleCost);
                        // Display the deduction.
                        GTA.Native.Function.Call("DISPLAY_CASH", true);

                        // Add one more bottle to player's inventory.
                        UsedFuelBottles -= 1;

                        // Let the player know.
                        if (Settings.GetValueBool("BOTTLEPURCHASETEXT", "TEXTS", true))
                        {
                            Game.DisplayText("You purchased one more fuel bottle for $" + FuelBottleCost + ".\nNow you have " + (MaxFuelBottles - UsedFuelBottles) + " fuel bottles.", 5000);
                        }

                        Log("KeyDown", "Player purchased one more emergency fuel bottle on vehicle: " + CurrentVehicle.Name.ToString() + " and now have " + (MaxFuelBottles - UsedFuelBottles) + " out of " + MaxFuelBottles + " bottles.");
                    }
                    // If player doesn't have enough money
                    else
                    {
                        // Let the player know.
                        if (Settings.GetValueBool("OUTOFFUNDSBOTTLETEXT", "TEXTS", true))
                        {
                            Game.DisplayText("Sorry, you don't have enough money to buy a fuel bottle.", 5000);
                        }
                    }
                }
            }
        }
        #endregion

        #region Common Events
        /// <summary>
        /// run every second
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FuelScript_Tick(object sender, EventArgs e)
        {
            try
            {
                // Make sure Niko is in a vehicle.
                if (Player.Character.isInVehicle())
                {
                    // Make sure player is seated on the driving seat.
                    if (Player == CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver))
                    {
                        if (Refueling)
                        {
                            // Refuel
                            ReFuel();
                        }
                        else
                        {
                            // Take care of fuel draining
                            // Vehicle required for mission?
                            if (CurrentVehicle.isRequiredForMission)
                            {
                                // User chosen to drain even so?
                                if (Settings.GetValueBool("MVDRAIN", "MISC", false))
                                {
                                    DrainFuel();
                                }
                            }
                            // Free roam vehicles?
                            else
                            {
                                DrainFuel();
                            }
                        }

                        // Don't know anything about a last vehicle?
                        if (LastVehicle == null || CurrentVehicle != LastVehicle)
                        {
                            OnReserve = false;
                            LastVehicle = CurrentVehicle;
                        }
                    }
                    // Niko is not on the driving seat?
                    else
                    {
                        // If so, he might be taking a lift from a friend or something, right?
                        LastVehicle = null;
                    }
                }
                // Niko is not in a vehicle?
                else
                {
                    // If so...
                    LastVehicle = null;
                }

                // Track player vehicles details...
                if (Player.Character.isGettingIntoAVehicle)
                {
                    // Is he inside of a vehicle?
                    if (Player.Character.isInVehicle() && Player == CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver))
                    {
                        // Log it so we can know what happened.
                        Log("Tick", "Player entered a vehicle: " + CurrentVehicle.Name.ToString() + ", Have " + Convert.ToInt32(CurrentVehicle.Metadata.Fuel) + " litre(s), Capacity - " + Convert.ToInt32(CurrentVehicle.Metadata.MaxTank) + " litre(s), Reserve - " + Convert.ToInt32(CurrentVehicle.Metadata.Reserve) + " litre(s), Drain - " + Convert.ToInt32(CurrentVehicle.Metadata.Drain) + " units.");

                        // Is current vehicle is required for an ingame mission?
                        if (CurrentVehicle.isRequiredForMission && !Settings.GetValueBool("MVDRAIN", "MISC", false))
                        {
                            // If so, fuel should not be drained, otherwise player will face so much trouble...
                            // Specially, if the mission is based on time, he can't go refuel it, can he?
                            if (Settings.GetValueBool("MISSIONREQUIREDTEXT", "TEXTS", true))
                            {
                                Game.DisplayText("Your vehicle is required for a mission, fuel is not draining!", 8000);
                            }

                            Log("DrainFuel", "Fuel is not draining on vehicle: " + CurrentVehicle.Name.ToString() + " as it's required for a mission.");
                        }
                        // Is it a normal vehicle? Using to free roam?
                        else
                        {
                            if (Settings.GetValueBool("VEHICLESTATUSTEXT", "TEXTS", true))
                            {
                                // Calculate fuel percentage.
                                float FuelAvailability = (Convert.ToInt32(CurrentVehicle.Metadata.Fuel) * 100) / Convert.ToInt32(CurrentVehicle.Metadata.MaxTank);

                                // When player gets into a vehicle, so it's status.
                                Game.DisplayText("This vehicle currently holds " + Convert.ToInt32((float)FuelAvailability).ToString() + "% fuel left in it's " + Convert.ToInt32(CurrentVehicle.Metadata.MaxTank) + " litre(s) tank.\n" + (((MaxFuelBottles - UsedFuelBottles) >= 1) ? "You have " + (MaxFuelBottles - UsedFuelBottles) + " emergency fuel bottle" + (((MaxFuelBottles - UsedFuelBottles) == 1) ? "" : "s") + " left." : "You have no emergency fuel bottles left."), 10000);
                            }

                            // Mark it as not damaged by low fuel running.
                            CurrentVehicle.Metadata.NoFuelDamage = (bool)false;
                        }
                    }
                }
            }
            // Log if any errors pops up...
            catch (Exception crap)
            {
                Log("ERROR: Tick", crap.Message);
            }
        }
        /// <summary>
        /// run every frame, devMode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FuelScript_PerFrameDrawing_devMode(object sender, GTA.GraphicsEventArgs e)
        {
            try
            {
                // Take care of lights and engine when refueling...
                if (Refueling)
                {
                    Player.Character.Task.ClearSecondary();
                    GTA.Native.Function.Call("FORCE_CAR_LIGHTS", CurrentVehicle, 0);
                    CurrentVehicle.EngineRunning = false;
                    // CurrentVehicle.HazardLightsOn = true;
                }

                // Set the scaling type.
                e.Graphics.Scaling = FontScaling.ScreenUnits;

                // Get the dashboard location.
                PointF Dashboard = new PointF(Settings.GetValueFloat("X", "DASHBOARD", 0.0f), Settings.GetValueFloat("Y", "DASHBOARD", 0.0f));

                // Is player in vehicle?
                if (Player.Character.isInVehicle())
                {
                    // Try to get the fuel level and draw it.
                    try { e.Graphics.DrawText("FUEL".PadRight(15) + CurrentVehicle.Metadata.Fuel, Dashboard.X, Dashboard.Y + 0.02f); }
                    catch { }

                    // Draw vehicle speed.
                    e.Graphics.DrawText("SPEED".PadRight(15) + "\t" + CurrentVehicle.Speed * 3.6f, Dashboard.X, Dashboard.Y + 0.04f);
                    // Draw vehicle engine health (0-1000 float).
                    e.Graphics.DrawText("ENGINE".PadRight(15) + "\t" + CurrentVehicle.EngineHealth, Dashboard.X, Dashboard.Y + 0.06f);
                    // Draw vehicle RPM (how hard the player push the engine).
                    e.Graphics.DrawText("RPM".PadRight(15) + "\t" + CurrentVehicle.CurrentRPM, Dashboard.X, Dashboard.Y + 0.08f);
                    // Draw vehicle hash code.
                    e.Graphics.DrawText("HASH".PadRight(15) + "\t" + CurrentVehicle.Model.Hash, Dashboard.X, Dashboard.Y + 0.1f);
                    // Draw vehicle's human friendly name.
                    e.Graphics.DrawText("NAME".PadRight(15) + "\t" + CurrentVehicle.Model, Dashboard.X, Dashboard.Y + 0.12f);
                    // Draw drain per second speed (how faster the fuel is draining per second).
                    e.Graphics.DrawText("DRAIN/Sec".PadRight(15) + "\t" + DrainSpeed, Dashboard.X, Dashboard.Y + 0.14f);
                    // Draw vehicle doors status.
                    e.Graphics.DrawText("DOOR".PadRight(15) + "\t" + ((CurrentVehicle.DoorLock == DoorLock.None) ? "UNLOCKED" : ((CurrentVehicle.DoorLock == DoorLock.CanOpenFromInside) ? "OUT LOCK" : "FULL LOCK")), Dashboard.X, Dashboard.Y + 0.16f);
                }

                // Draw player position. X, Y and Z coordinates.
                e.Graphics.DrawText("LOCATION", Dashboard.X, Dashboard.Y + 0.2f);
                e.Graphics.DrawText(Player.Character.Position.X + ", " + Player.Character.Position.Y + ", " + Player.Character.Position.Z, Dashboard.X, Dashboard.Y + 0.22f);

            }
            catch (Exception crap)
            {
                Log("ERROR: devMode", crap.Message);
            }
        }
        /// <summary>
        /// run every frame, digitalMode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FuelScript_PerFrameDrawing_digitalMode(object sender, GTA.GraphicsEventArgs e)
        {
            try
            {
                // Take care of the lights and engine if player is refeuling..
                if (Refueling)
                {
                    GTA.Native.Function.Call("FORCE_CAR_LIGHTS", CurrentVehicle, 0);
                    CurrentVehicle.EngineRunning = false;
                    // CurrentVehicle.HazardLightsOn = true;
                }

                // Set the scaling type.
                e.Graphics.Scaling = FontScaling.ScreenUnits;

                // If player is in a vehicle.
                if (Player.Character.isInVehicle())
                {
                    // Draw fuel level.
                    try
                    {
                        e.Graphics.DrawText("FUEL", Dashboard.X, Dashboard.Y, Color.Beige);
                        e.Graphics.DrawText(Convert.ToInt32((float)CurrentVehicle.Metadata.Fuel).ToString(), Dashboard.X + 0.06f, Dashboard.Y, (CurrentVehicle.Metadata.Fuel <= CurrentVehicle.Metadata.Reserve) ? Color.Red : Color.Green);
                    }
                    catch { }

                    // Draw vehicle speed.
                    e.Graphics.DrawText("SPEED", Dashboard.X, Dashboard.Y + 0.03f);

                    // If current vehicle is a boat, units needs to be converted to knots.
                    if (CurrentVehicle.Model.isBoat)
                    {
                        e.Graphics.DrawText(Convert.ToInt32(CurrentVehicle.Speed * Knots).ToString(), Dashboard.X + 0.06f, Dashboard.Y + 0.03f);
                        e.Graphics.DrawText("Knots", Dashboard.X + 0.09f, Dashboard.Y + 0.03f);

                    }
                    // If it's a normal vehicle, draw it in KPH, or MPH.
                    else
                    {
                        e.Graphics.DrawText(Convert.ToInt32(CurrentVehicle.Speed * SpeedMultiplier).ToString(), Dashboard.X + 0.06f, Dashboard.Y + 0.03f);
                        e.Graphics.DrawText((SpeedMultiplier == 3.6f) ? "KPH" : "MPH", Dashboard.X + 0.09f, Dashboard.Y + 0.03f);
                    }
                }
            }
            catch (Exception crap)
            {
                Log("ERROR: digitalMode", crap.Message);
            }
        }
        /// <summary>
        /// run every frame, digitalMode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FuelScript_PerFrameDrawing_classicMode(object sender, GTA.GraphicsEventArgs e)
        {
            try
            {
                // Take care of the lights and engine when refueling.
                if (Refueling)
                {
                    GTA.Native.Function.Call("FORCE_CAR_LIGHTS", CurrentVehicle, 0);
                    CurrentVehicle.EngineRunning = false;
                    // CurrentVehicle.HazardLightsOn = true;
                }

                // Set the scaling type.
                e.Graphics.Scaling = FontScaling.ScreenUnits;

                // If player is in a vehicle.
                if (Player.Character.isInVehicle())
                {
                    try
                    {
                        // Calculate fuel availablity...
                        float FuelAvailability = (CurrentVehicle.Metadata.Fuel * 100) / CurrentVehicle.Metadata.MaxTank;

                        // NOTE: This is to know how much fuel is remaining, in litres. As we have fuel bottles indicator in place, this has been commented out.
                        // e.Graphics.DrawText(Convert.ToInt32((float)CurrentVehicle.Metadata.Fuel).ToString() + " l", Dashboard.X - 0.035f, Dashboard.Y - 0.012f, (CurrentVehicle.Metadata.Fuel <= CurrentVehicle.Metadata.Reserve) ? ((Flashing < 5) ? GTA.ColorIndex.SmokeSilverPoly : (GTA.ColorIndex)35) : GTA.ColorIndex.SmokeSilverPoly, FuelMeterFont);

                        // Draw the fuel bottles status (such as "2/5").
                        e.Graphics.DrawText(
                            String.Format("{0}/{1}", MaxFuelBottles - UsedFuelBottles, MaxFuelBottles),
                            Dashboard.X - 0.030f, 
                            Dashboard.Y - 0.011f, 
                            (MaxFuelBottles - UsedFuelBottles <= 1) ? ((Flashing < 5) ? GTA.ColorIndex.SmokeSilverPoly : (GTA.ColorIndex)35) : GTA.ColorIndex.SmokeSilverPoly, // at this point, if we have issues about performance the color can very well be select when any of the 3 conditions change in the first place
                            FuelMeterFont);

                        // Draw fuel level status (such as "57%").
                        // I'm not sure about this line and I can't test it. But String.Format("{0:00}%", FuelAvailability) should do the trick and it much faster
                        e.Graphics.DrawText(Convert.ToInt32((float)FuelAvailability).ToString("00") + "%", (Dashboard.X + GaugeWidth) + 0.006f, Dashboard.Y - 0.012f,
                            (CurrentVehicle.Metadata.Fuel <= CurrentVehicle.Metadata.Reserve) ? ((Flashing < 5) ? GTA.ColorIndex.SmokeSilverPoly : (GTA.ColorIndex)35) : GTA.ColorIndex.SmokeSilverPoly, // at this point, if we have issues about performance the color can very well be select when any of the 3 conditions change in the first place
                            FuelMeterFont); 

                        // Draw fuel level meter's black background.
                        e.Graphics.DrawRectangle(new RectangleF(Dashboard.X - 0.0035f, Dashboard.Y - 0.004f, GaugeWidth, 0.0125f), GTA.ColorIndex.Black);

                        // Draw fuel level meter's dark grey foreground.
                        e.Graphics.DrawRectangle(new RectangleF(Dashboard.X, Dashboard.Y, (1 * (GaugeWidth - 0.007f)) / 1, 0.006f), (GTA.ColorIndex)1);

                        // Draw the front rectange widening how much fuel vehicle has.
                        // Green as normal, and red when running on reserved.
                        e.Graphics.DrawRectangle(new RectangleF(Dashboard.X, Dashboard.Y, (CurrentVehicle.Metadata.Fuel * (GaugeWidth - 0.008f)) / CurrentVehicle.Metadata.MaxTank, 0.006f),
                            (CurrentVehicle.Metadata.Fuel <= CurrentVehicle.Metadata.Reserve) ? ((Flashing < 5) ? (GTA.ColorIndex)1 : (GTA.ColorIndex)35) : (GTA.ColorIndex)50); // at this point, if we have issues about performance the color can very well be select when any of the 3 conditions change in the first place

                        // Controls the Flashinging when on reserved fuel.
                        Flashing = (Flashing == 20) ? 0 : Flashing++;
                    }
                    catch { }
                }

                // Draw Beta Watermark if the release is a beta release!
                if (Settings.GetValueBool("BETA", "RELEASE", false))
                {
                    // At top left corner of the screen.
                    e.Graphics.DrawSprite(BetaMark, 0.07f, 0.06f, 0.11f, 0.085f, 0);
                }
            }
            catch (Exception crap)
            {
                Log("ERROR: classicMode", crap.Message);
            }
        }
        #endregion
    }
}