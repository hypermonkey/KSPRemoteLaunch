using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPRemoteLaunch
{
    //[KSPAddon(KSPAddon.Startup.Flight, false)]
    [KSPAddon(KSPAddon.Startup.EditorAny,false)]
    public class RemoteLaunchGUI:MonoBehaviourExtended
    {
        //windowPos is used to position the GUI window, lets set it in the center of the screen
        protected Rect windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
        private string latText = "0.0";
        private string lonText = "0.0";
        private string launchText = "";
        private double lat = 0;
        private double lon = 0;
        private double height = 0.5;
        private bool windowActive = false;

        private void WindowGUI(int windowID)
        {
            GUIStyle buttonSty = new GUIStyle(GUI.skin.button);
            buttonSty.normal.textColor = buttonSty.focused.textColor = Color.white;
            buttonSty.hover.textColor = buttonSty.active.textColor = Color.yellow;
            buttonSty.onNormal.textColor = buttonSty.onFocused.textColor = buttonSty.onHover.textColor = buttonSty.onActive.textColor = Color.green;
            buttonSty.padding = new RectOffset(8, 8, 8, 8);

            GUIStyle textSty = new GUIStyle(GUI.skin.textField);
            textSty.normal.textColor = buttonSty.focused.textColor = Color.white;
            textSty.padding = new RectOffset(8, 8, 8, 8);

            GUIStyle labelSty = new GUIStyle(GUI.skin.label);
            labelSty.normal.textColor = buttonSty.focused.textColor = Color.white;
            labelSty.padding = new RectOffset(8, 8, 8, 8);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Latitude:  ", labelSty, GUILayout.ExpandWidth(true));
            latText = GUILayout.TextField(latText, textSty, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Longitude: ", labelSty, GUILayout.ExpandWidth(true));
            lonText = GUILayout.TextField(lonText, textSty, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Name: ", labelSty, GUILayout.ExpandWidth(true));
            launchText = GUILayout.TextField(launchText, textSty, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Add Launch Location", buttonSty, GUILayout.ExpandWidth(true)))
            {
                try
                {
                    lat = double.Parse(latText);
                    lon = double.Parse(lonText);
                    LogDebugOnly("Latitude: {0} , Longitude: {1}", lat, lon);
                    LogDebugOnly("Planet: {0}, Launch Pad: {1}", FlightGlobals.Bodies[1].name,launchText);
                    LaunchDriver.CreateCustomLaunchSite(lat, lon, FlightGlobals.Bodies[1], launchText);
                    
                }
                catch(Exception e)
                {
                    LogDebugOnly(e.Message);
                }

            }

            if (GUILayout.Button("Set Launch Site", buttonSty, GUILayout.ExpandWidth(true)))
            {
                LaunchDriver.SetLaunchSite(launchText);

            }

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 250, 30));
        }


        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Remote Launch", GUILayout.MinWidth(250), GUILayout.MinHeight(150));

        }

        void Start()
        {

            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI

            if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
            }
            windowActive = true;
           
            LogDebugOnly("GUI Added");


        }

        void Update()
        {
            if (Input.GetKeyDown("l"))
            {
                LogDebugOnly("KeyDown event detected for 'l' ");
                if (windowActive)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                    windowActive = false;
                }
                else
                {
                    RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
                    windowActive = true;
                }

            }


        }


        void onDestroy()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI

        }
    }
}
