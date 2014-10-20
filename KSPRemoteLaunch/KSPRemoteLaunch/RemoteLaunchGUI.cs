﻿using System;
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
        private string descText = "";
        private double lat = 0;
        private double lon = 0;
        private double height = 0.5;
        private bool windowActive = false;
        private string result = "";
        //private List<GUIToggleButton> SiteToggleList;
        private GUIOptionGroup SiteToggleList;

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

            GUIStyle toggleSty = new GUIStyle(GUI.skin.button);
            toggleSty.normal.textColor = buttonSty.focused.textColor = Color.white;
            toggleSty.padding = new RectOffset(8, 8, 8, 8);
            //toggleSty.onNormal.textColor = Color.green;


            GUILayout.BeginVertical();
            

            GUILayout.Label("Launch Sites:", labelSty, GUILayout.ExpandWidth(true));
            
            //LaunchDriver.getLaunchSites().ForEach(delegate(PSystemSetup.LaunchSite site)
            //{
                //GUILayout.Label(site.name, labelSty, GUILayout.ExpandWidth(true));
                //Boolean enabled = true;
                //if (GUILayout.Toggle(enabled,site.name, toggleSty, GUILayout.ExpandWidth(true)))
                //{
                //    enabled = true;
                //    setLaunchSite(site);
                //}
            //    new GUIToggleButton().CheckPressed(site.name, delegate(bool enabled) { if (!enabled) setLaunchSite(site); });
            //});
            SiteToggleList.setStyle(toggleSty);
            SiteToggleList.checkSelected();

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

            GUILayout.Label("Result: " + result, labelSty, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Add Launch Location", buttonSty, GUILayout.ExpandWidth(true)))
            {
                try
                {
                    lat = double.Parse(latText);
                    lon = double.Parse(lonText);
                    LogDebugOnly("Latitude: {0} , Longitude: {1}", lat, lon);
                    LogDebugOnly("Planet: {0}, Launch Pad: {1}", FlightGlobals.Bodies[1].name,launchText);
                    
                    //PSystemSetup.LaunchSite newSite = LaunchDriver.CreateCustomLaunchSite(lat, lon, FlightGlobals.Bodies[1], launchText,descText);

                    LaunchSiteExt newSite = LaunchDriver.CreateCustomLaunchSite(lat, lon, FlightGlobals.Bodies[1], launchText, descText);

                    //result = "Site '" + launchText + "' created!";
                    result = String.Format("Site '{0}' created @ Lat: {1} , Lon: {2}", launchText, lat, lon);

                    SiteToggleList.addToggleButton(new GUIToggleButton(launchText, delegate(bool enabled) {
                        LogDebugOnly("Site: {0} | Enabled: {1}",newSite.name, enabled);
                        //if (!enabled)
                            setLaunchSite(newSite);
                            
                    }));

                }
                catch(Exception e)
                {
                    LogDebugOnly(e.Message);
                    result = e.Message;
                    
                }

            }

            descText = GUILayout.TextArea(descText, textSty, GUILayout.ExpandHeight(true));


            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 250, 30));
        }

        private void setLaunchSite(LaunchSiteExt site)
        {
            try
            {
                LaunchDriver.SetLaunchSite(site.name);
                descText = site.description;
                //result = "Launch Site Loaded: '" + site.name + "'";
            }
            catch (Exception e)
            {
                LogDebugOnly(e.Message);
                result = "Can't find Launch Site";
            }

        }


        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Remote Launch", GUILayout.MinWidth(250), GUILayout.MinHeight(150));

        }

        void Start()
        {
            //this is not needed now we have the GUIOptionGroup
            /*
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
                result = "Launch Site Loaded: 'LaunchPad'";
            else
                result = "Launch Site Loaded: 'Runway'";
            */
            LogDebugOnly("Starting OptionGroup setup");

            //GUIStyle toggleSty = new GUIStyle(GUI.skin.button);
            //toggleSty.normal.textColor = Color.white;
            //toggleSty.padding = new RectOffset(8, 8, 8, 8);

            LogDebugOnly("Style Setup Done");

            SiteToggleList = new GUIOptionGroup();
            LogDebugOnly("Created empty optionGroup");
            
            LaunchDriver.getLaunchSites().ForEach(delegate(LaunchSiteExt site)
            {
                GUIToggleButton btn = new GUIToggleButton(site.name,delegate(bool enabled) {
                    
                    LogDebugOnly("Site: {0} | Enabled: {1}", site.name, enabled);
                    //if (!enabled)
                        setLaunchSite(site);
                });

                SiteToggleList.addToggleButton(btn);

                if (HighLogic.LoadedScene == GameScenes.EDITOR && site.name == "LaunchPad" || HighLogic.LoadedScene == GameScenes.SPH && site.name == "Runway")
                    SiteToggleList.setActiveToggleButton(btn);

            });
            LogDebugOnly("Site Button Setup Done");
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

    internal class GUIOptionGroup
    {
        private List<GUIToggleButton> buttons;
        private GUIToggleButton activeButton;
        //private GUIStyle buttonStyle;

        public GUIOptionGroup()
        {
            buttons = new List<GUIToggleButton>();
        }

        public void setStyle(GUIStyle style)
        {
            foreach (GUIToggleButton btn in buttons)
            {
                btn.setStyle(style);
            }
        }

        public void addToggleButton(GUIToggleButton toggleButton)
        {
            buttons.Add(toggleButton);
        }

        public void setActiveToggleButton(GUIToggleButton toggleButton)
        {
            activeButton = toggleButton;
            activeButton.enable();
        }

        public void addActiveToggleButton(GUIToggleButton toggleButton)
        {
            buttons.Add(toggleButton);
            activeButton = toggleButton;
            activeButton.enable();
        }

        public void checkSelected()
        {
            foreach (GUIToggleButton btn in buttons)
            {
                if (btn.CheckPressed() && btn != activeButton)
                {
                    //if not active button
                    //then set active button to be inactive
                    //set current button to be active
                    Debug.Log("Previous Active Button: " + activeButton);
                    activeButton.dissable();
                    btn.enable();
                    activeButton = btn;
                    Debug.Log("Current Active Button: " + activeButton);
                    //activeButton.enable();
                }
            }

        }
        

    }
    
    internal class GUIToggleButton
    {
        private GUIStyle buttonStyle;
        private string GUIText;
        private bool enabled = false;
        //private params GUILayoutOption[] options;
        private onSelected selectedAction;

        public GUIToggleButton(string text, onSelected action)
        {
            selectedAction = action;
            GUIText = text;
            
        }

        public void setStyle(GUIStyle style)
        {
            buttonStyle = style;
        }

        public delegate void onSelected(bool enabled);

        public bool CheckPressed()
        {
            bool pressed = false;
            if (GUILayout.Toggle(enabled, GUIText, buttonStyle, GUILayout.ExpandWidth(true)))
            {
                
                //enabled = !enabled;
                pressed = true;
                
                
                
            }
            return pressed;
            
        }

        public void dissable()
        {
            enabled = false;
        }
        public void enable()
        {
            enabled = true;
            selectedAction(enabled);
        }

        public override string ToString() 
        {
            return GUIText;
        }
    }
        
        

}
