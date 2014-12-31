using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace KSPRemoteLaunch
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class LaunchDriver:MonoBehaviourExtended
    {
        private static string SavePath = "";
        private static string SaveFile = "Persistent-LaunchSites.sfs";
        private static List<LaunchSiteExt> launchSites = new List<LaunchSiteExt>();


        public static bool hasLoadedGameChanged()
        {
            if (SavePath != KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/")
                return true;
            else
                return false;
        }

        void Start()
        {
            
            LogDebugOnly("---Launch Driver Start---");
            //We don't want to reload every time the user goes to the SpaceCentre scene??
            if (!hasLoadedGameChanged())
            {
                
                LogDebugOnly(SavePath);
                return;
            }
            SavePath = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/";

            launchSites = new List<LaunchSiteExt>();

            //change default launch sites to extended version
            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "SpaceCenterFacility[]")
                {
                    PSystemSetup.SpaceCenterFacility[] sites = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);
                    LogDebugOnly("Begin LaunchPad setup");
                    LaunchSiteExt LaunchPad = new LaunchSiteExt(sites[0],"The LaunchPad!", FlightGlobals.Bodies[1]);
                    
                    LogDebugOnly("End LaunchPad setup");
                    LaunchSiteExt Runway = new LaunchSiteExt(sites[1], "The Runway", FlightGlobals.Bodies[1]);
                    
                    LogDebugOnly("Sites Created");
                    //default launch sites are already in the game array - so we don't need to add them
                    launchSites.Add(LaunchPad);
                    launchSites.Add(Runway);

                    PSystemSetup.SpaceCenterFacility[] defaultSites = new PSystemSetup.SpaceCenterFacility[] { sites[0], sites[1],sites[2],sites[3],sites[4],sites[5],sites[6],sites[7],sites[8],sites[9] };
                    fi.SetValue(PSystemSetup.Instance, defaultSites);
                    

                    loadLaunchSites();
                    LogDebugOnly("Sites Loaded");
                }
            }
        }

        private static void loadLaunchSites()
        {
            LogDebugOnly("Loading Launch Sites...");
            
            try
            {
                
                ConfigNode launchSiteLoad = ConfigNode.Load(SavePath + SaveFile);
                if (launchSiteLoad == null)
                    return;//Nothing to load
                LogDebugOnly("Launch Sites File Found");
                foreach (ConfigNode c in launchSiteLoad.GetNodes("LaunchSite"))
                {
                    AddLaunchSiteExt(new LaunchSiteExt(c));
                }
            }
            catch
            {

            }
            


        }

        private static void saveLaunchSites()
        {
            ConfigNode launchSiteSave = new ConfigNode("LaunchSites");


            foreach (LaunchSiteExt saveSite in launchSites.FindAll(site => site.name != "LaunchPad" && site.name != "Runway"))
            {
                launchSiteSave.AddNode(saveSite.configNode);
            }


            launchSiteSave.Save(SavePath + SaveFile);

        }

        //this should be removed/replced with save all - as it is effectivly the equivilant? - it does preserve user edits though, but these edits won't be loaded
        public static void saveLaunchSite(LaunchSiteExt site)
        {
            LogDebugOnly("Start Save");
            ConfigNode launchSiteLoad = null;
            try
            {
                launchSiteLoad = ConfigNode.Load(SavePath + SaveFile);
            }
            catch
            {
                Log("Persistant-LaunchSites empty. Creating new file.");
            }

            if (launchSiteLoad == null)
            {
                //we need to create the file
                LogDebugOnly("Create New File");
                launchSiteLoad = new ConfigNode("LaunchSites");
            }
            //although there should be only one node with the specified name - we can't guarentee this (user may edit file)
            LogDebugOnly("Find saved Launch site");
            ConfigNode confSite = launchSiteLoad.GetNodes("LaunchSite").ToList<ConfigNode>().FirstOrDefault(conf => conf.GetValue("Name") == site.name);
            if (confSite != null)
            {
                confSite.ClearData();
                confSite.AddData(site.configNode);
            }
            else
            {
                launchSiteLoad.AddNode(site.configNode);
            }
            LogDebugOnly("Saving To File...");
            LogDebugOnly("Description: " + launchSiteLoad.GetNodes("LaunchSite").ToList<ConfigNode>().FirstOrDefault(conf => conf.GetValue("Name") == site.name).GetValue("Description"));
            launchSiteLoad.Save(SavePath + SaveFile);
        }

        /// <summary>
        /// Sets the Launch Site that a vessel will launch from when the launch button in the VAB/SPH editor is clicked
        /// </summary>
        /// <param name="siteName"></param>
        public static void setLaunchSite(String siteName) 
        {
            if (PSystemSetup.Instance.GetSpaceCenterFacility(siteName) != null)
                EditorLogic.fetch.launchSiteName = siteName;
            else
                throw new Exception("Can't find Launch Site: '" + siteName + "'");

        }

        //Change to Delete(<LaunchSite>T)?
        public static void deleteLaunchSite(LaunchSiteExt siteToDelete)
        {
            LogDebugOnly("Deleting Site: " + siteToDelete.name);
            //we should get a list of the default launch sites incase they change!
            if (siteToDelete.name == "LaunchPad" || siteToDelete.name == "Runway")
                throw new Exception("Can't delete default launch sites!");

            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "SpaceCenterFacility[]")
                {
                    PSystemSetup.SpaceCenterFacility[] sites = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);
                    if (PSystemSetup.Instance.GetSpaceCenterFacility(siteToDelete.name) != null) //logs 'Can not find launch site' to console 
                    {
                        PSystemSetup.SpaceCenterFacility[] newSites = new PSystemSetup.SpaceCenterFacility[sites.Length - 1];
                        for(int i = 0; i < sites.Length; i++)
                        {
                            if(sites[i].name != siteToDelete.name)
                                newSites[i] = sites[i];
                        }
                        fi.SetValue(PSystemSetup.Instance, newSites);
                    }
                    else
                        throw new Exception("Site not found!");
                }
                
            }

            launchSites.Remove(siteToDelete);
            //We need to destroy the GameObjects created behind the scenes
            siteToDelete.Destroy();
            saveLaunchSites();
        }
        
        //do we need this? - could just expose launchSites?
        public static void AddLaunchSiteExt(LaunchSiteExt site)
        {
            LogDebugOnly("Adding Launch Site: " + site.name);
            //checkValues(site.name, site.description, site.pqsName, site.lat, site.lon, newSite);
            AddSpaceCenterFacility(site);
            site.Setup();
            launchSites.Add(site);
        }

        /// <summary>
        /// Adds a space centre to the games array of space centres. The array is used to load space centres during launch. 
        /// The launch sites added by this method are not returned by getLaunchSites.
        /// </summary>
        /// <typeparam name="T">Any class that inherits from PSystemSetup.SpaceCenterFacility</typeparam>
        /// <param name="SpaceCenterFacility"></param>
        public static void AddSpaceCenterFacility<T>(T SpaceCenterFacility) where T : PSystemSetup.SpaceCenterFacility
        {
            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "SpaceCenterFacility[]")
                {
                    PSystemSetup.SpaceCenterFacility[] sites = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);

                    Debug.Log("PQS Name: " + SpaceCenterFacility.pqsName);

                    PSystemSetup.SpaceCenterFacility[] newSites = new PSystemSetup.SpaceCenterFacility[sites.Length + 1];
                    for (int i = 0; i < sites.Length; ++i)
                    {
                        newSites[i] = sites[i];
                    }
                    newSites[newSites.Length - 1] = (PSystemSetup.SpaceCenterFacility)SpaceCenterFacility;
                    fi.SetValue(PSystemSetup.Instance, newSites);
                    
                    Debug.Log("Created launch site \"" + SpaceCenterFacility.name + "\" with transform " + SpaceCenterFacility.spawnPoints[0].spawnTransformURL);

                }
            }
        }

        //get all launch sites - returns null if no sites exist
        public static List<LaunchSiteExt> getLaunchSites()
        {
            LogDebugOnly("Getting Launch Sites");

            return launchSites;
        }

        //should this be in model or controller? - controller
        public static void checkValues(string newName, string newDesc, CelestialBody newBody, double newLat, double newLon, LaunchSiteExt site)
        {
            //this needs moving to add
            if (PSystemSetup.Instance.GetSpaceCenterFacility(newName) != null && site.name != newName)//logs 'Can not find launch site' to console when null
                throw new Exception("Launch Site '" + newName + "' already exists");
            
            //Can't use this on defualt launch sites
            if (site.name == "LaunchPad" || site.name == "Runway")
                throw new Exception("Can't edit default launch sites!");

            //throws exception if a body is not found
            CelestialBody body = FlightGlobals.Bodies.Single<CelestialBody>(b => b.name == newBody.name);

            //Check for Polar launch site - KSP has camera bug when vessel is directly above either pole.
            //Validity checks should be moved into LaunchSiteExt class
            //LogDebugOnly("Remainder: " + (site.lat % 180.0d));
            if ((newLat % 180.0d) % 90.0d == 0 && newLat != 0)
                throw new Exception("Failed to create Launch Site." + System.Environment.NewLine + "Can't set Launch Sites to Poles!");

            //Game bugs out if a launch site has no name
            if (newName.Equals(""))
                throw new Exception("Failed to create Launch Site." + System.Environment.NewLine + "Launch Site must have a name!");


            Vector3 position = body.GetRelSurfaceNVector(newLat, newLon); //radial vector indicating position
            double altitude = body.pqsController.GetSurfaceHeight(position) - body.Radius;

            //LogDebugOnly("Altitude: {0}", altitude);

            if (altitude < 0)//then launchsite is underwater
            {
                throw new Exception("Failed to create/update Launch Site." + System.Environment.NewLine + "Can't set Launch Sites over water!");
            }

        }

    }

}
