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
        private static Rect windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
        private static Rect errorWindowPos = new Rect(Screen.width / 2 - 125, Screen.height / 2 -50, 10, 10);
        private static string latText = "0.0";
        private static string lonText = "0.0";
        private static string launchText = "";
        private static string descText = "";
        private static string planetText = "";
        private static bool showGUI = true;
        private static string loadedGame = HighLogic.SaveFolder;
        //private Vector2 scrollPos = new Vector2(0, 0);
        
        private static double lat = 0;
        private static double lon = 0;
        //private static double height = 0.5;
        private static bool windowActive = false;
        //private static string result = "";
        //private List<GUIToggleButton> SiteToggleList;
        private static GUIOptionGroup SiteToggleList;
        private static LaunchSiteExt currentLaunchSite = null;

        private static bool hasRunOnce = false;

        private static bool errorWindowActive = false;
        private static string errorMessage = "";
        private void ErrorWindowGUI(int errorWindowID)
        {
            GUIStyle buttonSty = new GUIStyle(GUI.skin.button);
            buttonSty.normal.textColor = buttonSty.focused.textColor = Color.white;
            buttonSty.hover.textColor = buttonSty.active.textColor = Color.yellow;
            buttonSty.onNormal.textColor = buttonSty.onFocused.textColor = buttonSty.onHover.textColor = buttonSty.onActive.textColor = Color.green;
            buttonSty.padding = new RectOffset(8, 8, 8, 8);

            GUIStyle labelSty = new GUIStyle(GUI.skin.label);
            labelSty.normal.textColor = buttonSty.focused.textColor = Color.white;
            labelSty.padding = new RectOffset(8, 8, 8, 8);


            GUI.DragWindow(new Rect(0, 0, 300, 30));

            GUILayout.Label(errorMessage, labelSty, GUILayout.Width(250.0f),GUILayout.ExpandHeight(true));


            if (GUILayout.Button("OK", buttonSty, GUILayout.ExpandWidth(true)))
            {
                RenderingManager.RemoveFromPostDrawQueue(4, new Callback(drawErrorGUI)); //close the GUI
                RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//open the GUI
                errorWindowActive = false;
                windowActive = true;
            }
        }

        private void drawErrorGUI()
        {
            GUI.skin = HighLogic.Skin;
            errorWindowActive = true;
            //windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Remote Launch", GUILayout.MinWidth(250), GUILayout.MinHeight(150));
            errorWindowPos = GUILayout.Window(2, errorWindowPos, ErrorWindowGUI, "Error");
        }

        private void raiseError(string message)
        {
            errorMessage = message;
            RenderingManager.AddToPostDrawQueue(4, new Callback(drawErrorGUI));//open the GUI
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
            windowActive = false;
            errorWindowActive = true;

        }

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

            //GUILayout.Label("Result: " + result, labelSty,totalColWidth);//, GUILayout.ExpandWidth(true));

            descText = GUILayout.TextArea(descText, textSty, totalColWidth);//, GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();

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
                    //result = String.Format("Site '{0}' created @ Lat: {1} , Lon: {2}", launchText, lat, lon);

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
                            //result = e.Message;
                            raiseError(e.Message);
                            throw e;
                        }
                    },
                    delegate()
                    {

                        update(newSite);


                    });
                    //move this to LaunchDriver.CreateCustomLaunchSite
                    LaunchDriver.saveLaunchSite(newSite);
                    
                }
                catch(Exception e)
                {
                    LogDebugOnly(e.Message);
                    //result = e.Message;
                    raiseError(e.Message);

                    
                }

            }

            if (GUILayout.Button("Update", buttonSty, GUILayout.ExpandWidth(true)))
            {
                SiteToggleList.updateActiveButton();

            }

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

        private void update(LaunchSiteExt site)
        {
            LogDebugOnly("-------Begin Update----------");

            LogDebugOnly(currentLaunchSite.name);

            try
            {
                //will throw an exception if the new site is invalid
                LaunchDriver.updateLaunchSite(site, launchText, descText, planetText, double.Parse(latText), double.Parse(lonText));
                SiteToggleList.setActiveButtonText(site.name);
                //move this into update?
                LaunchDriver.saveLaunchSite(site);
            }
            catch (Exception e)
            {
                //undo changes to GUI site details
                planetText = site.body;
                descText = site.description;
                latText = site.lat.ToString();
                lonText = site.lon.ToString();
                launchText = site.name;

                LogDebugOnly(e.Message);
                raiseError(e.Message);
                //result = e.Message;

            }
        }

        private void setLaunchSite(LaunchSiteExt site)
        {
            try
            {
                LaunchDriver.setLaunchSite(site.name);
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
                raiseError(e.Message);
                //result = "Can't find Launch Site";
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
            //LaunchDriver.init();
            
            if (hasRunOnce && loadedGame == HighLogic.SaveFolder)
            {
                LogDebugOnly("Reloading GUI");
                if(showGUI)
                    addWindows();
                //RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//open GUI
                
                //KSP will always revert to the default site for the selected editor
                //if (!(currentLaunchSite.name == "LaunchPad" || currentLaunchSite.name == "Runway"))
                setLaunchSite(currentLaunchSite);
                return;
            }
            LogDebugOnly("Starting GUI");
            loadedGame = HighLogic.SaveFolder;
            hasRunOnce = true;

            LogDebugOnly("Adding Toggle Button Group");
            SiteToggleList = new GUIOptionGroup();
            
            LaunchDriver.getLaunchSites().ForEach(delegate(LaunchSiteExt site)
            {
                LogDebugOnly("Adding Toggle Button: " + site.name);
                if (HighLogic.LoadedScene == GameScenes.EDITOR && site.name == "LaunchPad" || HighLogic.LoadedScene == GameScenes.SPH && site.name == "Runway")
                    SiteToggleList.addActiveToggleButton(site.name, delegate(bool enabled)
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
                            raiseError(e.Message);
                            //result = e.Message;
                            throw e;
                        }
                    },
                    delegate() {

                        update(site);
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
                            raiseError(e.Message);
                            //result = e.Message;
                            throw e;
                        }
                    },
                    delegate()
                    {

                        update(site);
                    });
                }
                
            });
            LogDebugOnly("Site Button Setup Done");
            
            //centre the window
            windowPos.x = Screen.width / 2;
            windowPos.y = Screen.height / 2;

            //RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
            errorWindowActive = false;
            windowActive = true;
            showGUI = true;
            addWindows();
            
            LogDebugOnly("GUI Open: " + showGUI);
            LogDebugOnly("GUI Added");
            

        }

        private void removeWindows()
        {
            if (windowActive)
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
            }
            if (errorWindowActive)
            {
                RenderingManager.RemoveFromPostDrawQueue(4, new Callback(drawErrorGUI)); //close the GUI
            }
        }

        private void addWindows()
        {
            if (windowActive)
            {
                RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//open the GUI
            }
            if (errorWindowActive)
            {
                RenderingManager.AddToPostDrawQueue(4, new Callback(drawErrorGUI));//open the GUI
            }
        }

        void Update()
        {
            if (Input.GetKeyDown("l"))
            {
                LogDebugOnly("KeyDown event detected for 'l' ");
                LogDebugOnly("GUI Open: " + showGUI);
                LogDebugOnly("Main Window Open: " + windowActive);
                LogDebugOnly("Error Window Open: " + errorWindowActive);
                if (showGUI)
                {
                    removeWindows();
                    showGUI = false;
                }
                else
                {
                    addWindows();
                    showGUI = true;
                }

            }


        }


        void onDestroy()
        {
            removeWindows();
            /*
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
            RenderingManager.RemoveFromPostDrawQueue(4, new Callback(drawErrorGUI)); //close the GUI
             * */
        }
    }

    
        
        

}
