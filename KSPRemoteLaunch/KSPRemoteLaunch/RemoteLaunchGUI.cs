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
        private string descText = "";
        private string planetText = "";
        //private Vector2 scrollPos = new Vector2(0, 0);

        private double lat = 0;
        private double lon = 0;
        private double height = 0.5;
        private bool windowActive = false;
        private string result = "";
        //private List<GUIToggleButton> SiteToggleList;
        private GUIOptionGroup SiteToggleList;
        private LaunchSiteExt currentLaunchSite = null;

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


            GUI.DragWindow(new Rect(0, 0, 500, 30));

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            

            GUILayout.Label("Launch Sites:", labelSty, GUILayout.Width(210.0f));
            

            SiteToggleList.setStyle(toggleSty);
            SiteToggleList.checkSelected();

            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            //float labelColWidth = 100.0f;
            GUILayoutOption labelColWidth = GUILayout.Width(100.0f);
            GUILayoutOption textColWidth = GUILayout.Width(150.0f);
            GUILayoutOption totalColWidth = GUILayout.Width(250.0f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Name: ", labelSty, labelColWidth);//, GUILayout.ExpandWidth(true));
            launchText = GUILayout.TextField(launchText, textSty, textColWidth);//, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Planet:   ", labelSty, labelColWidth);//, GUILayout.ExpandWidth(true));
            planetText = GUILayout.TextField(planetText, textSty, textColWidth);//, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Latitude:  ", labelSty, labelColWidth);//, GUILayout.ExpandWidth(true));
            latText = GUILayout.TextField(latText, textSty, textColWidth);//, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Longitude: ", labelSty, labelColWidth);//, GUILayout.ExpandWidth(true));
            lonText = GUILayout.TextField(lonText, textSty, textColWidth);//, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.Label("Result: " + result, labelSty,totalColWidth);//, GUILayout.ExpandWidth(true));

            descText = GUILayout.TextArea(descText, textSty, totalColWidth);//, GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
#if DEBUG
            if (GUILayout.Button("Add", buttonSty, GUILayout.ExpandWidth(true)))//, GUILayout.ExpandWidth(true)))
            {
                try
                {
                    lat = double.Parse(latText);
                    lon = double.Parse(lonText);
                    LogDebugOnly("Latitude: {0} , Longitude: {1}", lat, lon);
                    LogDebugOnly("Planet: {0}, Launch Pad: {1}", FlightGlobals.Bodies[1].name,launchText);
                    
                    //PSystemSetup.LaunchSite newSite = LaunchDriver.CreateCustomLaunchSite(lat, lon, FlightGlobals.Bodies[1], launchText,descText);
                    CelestialBody planet = FlightGlobals.Bodies.Single<CelestialBody>(body => body.name == planetText);
                    LaunchSiteExt newSite = LaunchDriver.CreateCustomLaunchSite(lat, lon, planet, launchText, descText);

                    //result = "Site '" + launchText + "' created!";
                    result = String.Format("Site '{0}' created @ Lat: {1} , Lon: {2}", launchText, lat, lon);

                    SiteToggleList.addActiveToggleButton(launchText, delegate(bool enabled) {
                        LogDebugOnly("Site: {0} | Enabled: {1}",newSite.name, enabled);
                        setLaunchSite(newSite);
                        currentLaunchSite = newSite;

                    }, delegate()
                    {
                        try
                        {
                            LaunchDriver.deleteLaunchSite(newSite);
                        }
                        catch(Exception e)
                        {
                            LogDebugOnly(e.Message);
                            result = e.Message;
                            throw e;
                        }
                    });
                    //move this to LaunchDriver.CreateCustomLaunchSite
                    LaunchDriver.SaveLaunchSite(newSite);
                    
                }
                catch(Exception e)
                {
                    LogDebugOnly(e.Message);
                    result = e.Message;
                    
                }

            }

            if (GUILayout.Button("Update", buttonSty, GUILayout.ExpandWidth(true)))
            {
                try
                {
                    currentLaunchSite.body = planetText;
                    currentLaunchSite.description = descText;
                    currentLaunchSite.lat = double.Parse(latText);
                    currentLaunchSite.lon = double.Parse(lonText);
                    currentLaunchSite.name = launchText;
                    LaunchDriver.UpdateLaunchSite(currentLaunchSite);
                    //move this into update?
                    LaunchDriver.SaveLaunchSite(currentLaunchSite);
                }
                catch(Exception e)
                {
                    LogDebugOnly(e.Message);
                    result = e.Message;
                }
            }

#endif
            if (GUILayout.Button("Delete", buttonSty, GUILayout.ExpandWidth(true)))
            {
                SiteToggleList.deleteActiveButton();
            }
            

            if (GUILayout.Button("Clear Fields", buttonSty,GUILayout.ExpandWidth(true)))
            {
                descText = "";
                latText = "";
                lonText = "";
                launchText = "";
                planetText = "";
            
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
        }

        private void setLaunchSite(LaunchSiteExt site)
        {
            try
            {
                LaunchDriver.SetLaunchSite(site.name);
                descText = site.description;
                latText = site.lat.ToString();
                lonText = site.lon.ToString();
                launchText = site.name;
                planetText = site.body;

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
            //windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Remote Launch", GUILayout.MinWidth(250), GUILayout.MinHeight(150));
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Remote Launch");
        }

        void Start()
        {
            

            SiteToggleList = new GUIOptionGroup();
            
            LaunchDriver.getLaunchSites().ForEach(delegate(LaunchSiteExt site)
            {
                if (HighLogic.LoadedScene == GameScenes.EDITOR && site.name == "LaunchPad" || HighLogic.LoadedScene == GameScenes.SPH && site.name == "Runway")
                    SiteToggleList.addActiveToggleButton(site.name, delegate(bool enabled)
                    {
                        LogDebugOnly("Site: {0} | Enabled: {1}", site.name, enabled);
                        setLaunchSite(site);
                        currentLaunchSite = site;
                    }, delegate() {
                        try
                        {
                            LaunchDriver.deleteLaunchSite(site);
                        }
                        catch (Exception e)
                        {
                            LogDebugOnly(e.Message);
                            result = e.Message;
                            throw e;
                        }
                    });
                else
                {
                    SiteToggleList.addToggleButton(site.name, delegate(bool enabled)
                    {
                        LogDebugOnly("Site: {0} | Enabled: {1}", site.name, enabled);
                        setLaunchSite(site);
                        currentLaunchSite = site;
                    }, delegate()
                    {
                        try
                        {
                            LaunchDriver.deleteLaunchSite(site);
                        }
                        catch (Exception e)
                        {
                            LogDebugOnly(e.Message);
                            result = e.Message;
                            throw e;
                        }
                    });
                }
                
            });
            LogDebugOnly("Site Button Setup Done");
            
            //centre the window
            windowPos.x = Screen.width / 2;
            windowPos.y = Screen.height / 2;

            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI

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
                    RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//open the GUI
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
        private List<GUIOptionButton> buttons;
        private GUIOptionButton activeButton;
        private Vector2 scrollPos = new Vector2(0, 0);
        public delegate void onSelected(bool enabled);
        public delegate void onDelete();

        public GUIOptionGroup()
        {
            buttons = new List<GUIOptionButton>();
        }

        public void setStyle(GUIStyle style)
        {
            foreach (GUIOptionButton btn in buttons)
            {
                btn.setStyle(style);
            }
        }
        public void deleteActiveButton()
        {
            activeButton.delete();
            buttons.Remove(activeButton);
        }

        public void addToggleButton(string text, onSelected action, onDelete delAction)
        {
            buttons.Add(new GUIOptionButton(text, action, delAction));

        }

        public void setActiveToggleButton(GUIOptionButton toggleButton)
        {
            changeActiveToggleButton(toggleButton);
        }

        public void addActiveToggleButton(string text, onSelected action, onDelete delAction)
        {
            GUIOptionButton toggleButton = new GUIOptionButton(text, action, delAction);
            buttons.Add(toggleButton);
            changeActiveToggleButton(toggleButton);
        }

        //add GUI params?
        public void checkSelected()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUI.skin.scrollView, GUILayout.Width(210.0f));
            foreach (GUIOptionButton btn in buttons)
            {
                if (btn.CheckPressed() && btn != activeButton)
                {
                    Debug.Log("Previous Active Button: " + activeButton);
                    activeButton.dissable();
                    btn.enable();
                    activeButton = btn;
                    Debug.Log("Current Active Button: " + activeButton);
                }
            }
            GUILayout.EndScrollView();
        }

        private void changeActiveToggleButton(GUIOptionButton toggleButton)
        {
            if (activeButton != null)
            {
                activeButton.dissable();
            }
            toggleButton.enable();
            activeButton = toggleButton;
        }


        internal class GUIOptionButton
        {
            private GUIStyle buttonStyle;
            private string GUIText;
            private bool enabled = false;
            private onSelected selectedAction;
            private onDelete deleteAction;

            public GUIOptionButton(string text, onSelected action, onDelete delAction)
            {
                selectedAction = action;
                deleteAction = delAction;
                GUIText = text;

            }

            public void setStyle(GUIStyle style)
            {
                buttonStyle = style;
            }

            
            //Button must be enabled to be pressed
            public bool CheckPressed()
            {
                if (GUILayout.Toggle(enabled, GUIText, buttonStyle, GUILayout.Width(174.0f)))
                {
                    return true;
                }
                return false;
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

            public void delete()
            {
                deleteAction();
            }

            public override string ToString()
            {
                return GUIText;
            }
        }
    }
    
    
        
        

}
