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
        private static string SavePath = "";//KSPUtil.ApplicationRootPath + "/Saves/" + HighLogic.SaveFolder + "/";
        private static string SaveFile = "Persistent-LaunchSites.sfs";
        private static List<LaunchSiteExt> launchSites = new List<LaunchSiteExt>();


        public static bool hasLoadedGameChanged()
        {
            if (SavePath != KSPUtil.ApplicationRootPath + "/Saves/" + HighLogic.SaveFolder + "/")
                return true;
            else
                return false;
        }

        void Start()
        {
            
            LogDebugOnly("---Launch Driver Start---");
            if (!hasLoadedGameChanged())
            {
                
                LogDebugOnly(SavePath);
                return;
            }
            SavePath = KSPUtil.ApplicationRootPath + "/Saves/" + HighLogic.SaveFolder + "/";

            launchSites = new List<LaunchSiteExt>();
            //if (!firstTime)
            //    return;
            //change default launch sites to extended version?
            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "SpaceCenterFacility[]")
                {
                    PSystemSetup.SpaceCenterFacility[] sites = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);
                    LogDebugOnly("Begin LaunchPad setup");
                    LaunchSiteExt LaunchPad = new LaunchSiteExt();
                    LaunchPad.description = "The Launch Pad!";
                    //LaunchPad.launchPadName = sites[0].launchPadName;
                    LaunchPad.facilityTransformName = sites[0].facilityTransformName;

                    //LaunchPad.launchPadPQS = sites[0].launchPadPQS;
                    LaunchPad.facilityPQS = sites[0].facilityPQS;

                    //LaunchPad.launchPadTransform = sites[0].launchPadTransform;
                    LaunchPad.facilityTransform = sites[0].facilityTransform;
                    LaunchPad.facilityName = sites[0].facilityName;
                    LaunchPad.spawnPoints = sites[0].spawnPoints;

                    LaunchPad.name = sites[0].name;
                    LaunchPad.pqsName = sites[0].pqsName;
                    

                    //PQSCity cty = LaunchPad.launchPadTransform.parent.gameObject.GetComponent<PQSCity>();
                    //cty = LaunchPad.launchPadTransform.localPosition
                    //LogDebugOnly(FlightGlobals.Bodies[1].GetLongitude(LaunchPad.launchPadTransform.position).ToString());
                    //LaunchPad.launchPadPQS.SetTarget(LaunchPad.launchPadTransform);
                    LaunchPad.facilityPQS.SetTarget(LaunchPad.spawnPoints[0].spawnPointTransform);

                    //PSystemSetup.Instance.SetPQSActive(LaunchPad.launchPadPQS);
                    //PSystemSetup.Instance.SetPQSActive(LaunchPad.facilityPQS);

                    //LogDebugOnly(FlightGlobals.Bodies[1].GetLongitude(LaunchPad.launchPadTransform.position).ToString());
                    //cty.Orientate();

                    LaunchPad.lon = FlightGlobals.Bodies[1].GetLongitude(LaunchPad.GetSpawnPoint(LaunchPad.name).spawnPointTransform.position);
                    LaunchPad.lat = FlightGlobals.Bodies[1].GetLatitude(LaunchPad.GetSpawnPoint(LaunchPad.name).spawnPointTransform.position);
                    
                    //LaunchPad.lat = FlightGlobals.Bodies[1].GetLatitude(cty.transform.position);
                    //this is redundant - pqsName == body
                    LaunchPad.body = "Kerbin";
                    LogDebugOnly("End LaunchPad setup");
                    LaunchSiteExt Runway = new LaunchSiteExt();
                    Runway.description = "The Runway!";
                    //Runway.launchPadName = sites[1].launchPadName;
                    Runway.facilityTransformName = sites[1].facilityTransformName;

                    //Runway.launchPadPQS = sites[1].launchPadPQS;
                    Runway.facilityPQS = sites[1].facilityPQS;

                    //Runway.launchPadTransform = sites[1].launchPadTransform;
                    Runway.facilityTransform = sites[1].facilityTransform;
                    Runway.facilityName = sites[1].facilityName;
                    Runway.spawnPoints = sites[1].spawnPoints;

                    Runway.name = sites[1].name;
                    Runway.pqsName = sites[1].pqsName;

                    //cty = Runway.launchPadTransform.parent.gameObject.GetComponent<PQSCity>();
                    //cty.Orientate();
                    //Runway.lon = FlightGlobals.Bodies[1].GetLongitude(cty.transform.position);
                    //Runway.lat = FlightGlobals.Bodies[1].GetLatitude(cty.transform.position);

                    //Runway.facilityPQS.SetTarget(Runway.spawnPoints[0].spawnPointTransform);
                    Runway.facilityPQS.SetTarget(Runway.GetSpawnPoint(Runway.name).spawnPointTransform);
                    //PSystemSetup.Instance.SetPQSActive(Runway.facilityPQS);
                    Runway.lon = FlightGlobals.Bodies[1].GetLongitude(Runway.GetSpawnPoint(Runway.name).spawnPointTransform.position);
                    Runway.lat = FlightGlobals.Bodies[1].GetLatitude(Runway.GetSpawnPoint(Runway.name).spawnPointTransform.position);
                    //PSystemSetup.Instance.SetPQSInactive();
                    Runway.body = "Kerbin";

                    LogDebugOnly("Sites Created");
                    //sites[0] = LaunchPad;
                    //sites[1] = Runway;
                    launchSites.Add(LaunchPad);
                    launchSites.Add(Runway);

                    PSystemSetup.SpaceCenterFacility[] defaultSites = new PSystemSetup.SpaceCenterFacility[] { sites[0], sites[1],sites[2],sites[3],sites[4],sites[5],sites[6],sites[7],sites[8],sites[9] };
                    fi.SetValue(PSystemSetup.Instance, defaultSites);
                    

                    //firstTime = false;

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
                    double lat = double.Parse(c.GetValue("Lat"));
                    double lon = double.Parse(c.GetValue("Lon"));
                    CelestialBody theBody = FlightGlobals.Bodies.Find(body => body.name == c.GetValue("Body"));
                    string siteName = c.GetValue("Name");
                    string desc = c.GetValue("Description");
                    CreateCustomLaunchSite(lat, lon, theBody, siteName, desc);
                }
            }
            catch//(Exception e)
            {
                //Log(e.Message);
            }
            


        }

        private static void saveLaunchSites()
        {
            ConfigNode launchSiteSave = new ConfigNode("LaunchSites");


            foreach (LaunchSiteExt saveSite in launchSites.FindAll(site => site.name != "LaunchPad" && site.name != "Runway"))
            {
                ConfigNode site = new ConfigNode("LaunchSite");
                site.AddValue("Name", saveSite.name);
                site.AddValue("Lat", saveSite.lat);
                site.AddValue("Lon", saveSite.lon);
                site.AddValue("Body", saveSite.body);
                site.AddValue("Description", saveSite.description);
                launchSiteSave.AddNode(site);
            }


            launchSiteSave.Save(SavePath + SaveFile);

        }

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
                //edit existing
                confSite.SetValue("Name", site.name);
                confSite.SetValue("Lat", site.lat.ToString());
                confSite.SetValue("Lon", site.lon.ToString());
                confSite.SetValue("Body", site.body);
                confSite.SetValue("Description", site.description);
            }
            else
            {
                //add new
                confSite = new ConfigNode("LaunchSite");
                confSite.AddValue("Name", site.name);
                confSite.AddValue("Lat", site.lat);
                confSite.AddValue("Lon", site.lon);
                confSite.AddValue("Body", site.body);
                confSite.AddValue("Description", site.description);
                launchSiteLoad.AddNode(confSite);
            }
            launchSiteLoad.Save(SavePath + SaveFile);
        }

        public static void setLaunchSite(String siteName) 
        {
            if (PSystemSetup.Instance.GetSpaceCenterFacility(siteName) != null)
                EditorLogic.fetch.launchSiteName = siteName;
            else
                throw new Exception("Can't find Launch Site: '" + siteName + "'");

        }

        public static void deleteLaunchSite(LaunchSiteExt siteToDelete)
        {
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

            //object that defines launch site - created during creation logic
            GameObject.Destroy(siteToDelete.facilityTransform.gameObject);
            
            launchSites.Remove(siteToDelete);
            saveLaunchSites();
        }

        public static void updateLaunchSite(LaunchSiteExt site, string siteName,string description,string planet,double lat, double lon)
        {
            //make sure it exists before we try to edit it
            launchSites.Single<LaunchSiteExt>(l => l == site);

            setValues(siteName, description, planet, lat, lon, site);

            CelestialBody body = FlightGlobals.Bodies.Single<CelestialBody>(b => b.name == site.body);

            

            Vector3 position = body.GetRelSurfaceNVector(site.lat, site.lon); //radial vector indicating position
            double altitude = body.pqsController.GetSurfaceHeight(position) - body.Radius;

            Vector3 orientation = Vector3.up;
            float rotation = 0; //Don't know how to work this out from vessel rotation.
            float visibleRange = 5000;
            
            LogDebugOnly("Set location variables");
            
            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = new GameObject[0],
                objects = new GameObject [0],
                visibleRange = visibleRange
            };

            
            PQSCity launchPQS;
            launchPQS = site.facilityTransform.GetComponent<PQSCity>();
            //launchPQS = site.launchPadTransform.parent.gameObject.GetComponent<PQSCity>();
            //UnityEngine.Object.Destroy(launchPQS);
            //launchPQS = site.launchPadTransform.parent.gameObject.AddComponent<PQSCity>();

            LogDebugOnly("Added PQSCity to gameobject");
            launchPQS.lod = new[] { range };
            launchPQS.frameDelta = 1; //Unknown
            launchPQS.repositionToSphere = false; //enable repositioning to sphere and use RadiusOffset as altitude
            launchPQS.repositionToSphereSurface = true; //Snap to surface
            launchPQS.repositionToSphereSurfaceAddHeight =true;//add RadiusOffset to surfaceHeight when using ToSphereSurface
            launchPQS.repositionRadial = position; //position
            launchPQS.repositionRadiusOffset = 100.0d; //height from surface
            //launchPQS.repositionRadiusOffset = 1250.0d;//safety distance
            launchPQS.reorientInitialUp = orientation; //orientation
            launchPQS.reorientFinalAngle = rotation; //rotation x axis
            launchPQS.reorientToSphere = true; //adjust rotations to match the direction of gravity
            
            LogDebugOnly("Set PQSCity variables");

            //GameObject obj = site.launchPadTransform.parent.gameObject;
            GameObject obj = site.facilityTransform.gameObject;
            obj.name = site.name;
            obj.transform.parent = body.pqsController.transform;


            launchPQS.sphere = body.pqsController;
            launchPQS.order = 100;
            launchPQS.modEnabled = true;
            launchPQS.OnSetup();
            launchPQS.Orientate();
            LogDebugOnly("Setup PQSCity");


            MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupFacilities", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateSitesMI == null)
            {
                LogDebugOnly("Fail to find SetupLaunchSites().");
                throw new Exception("Fatal Error - Can't find method SetupLaunchSites().");
            }
            else
                updateSitesMI.Invoke(PSystemSetup.Instance, null);
            

        }

        public static LaunchSiteExt CreateCustomLaunchSite(double lat, double lon, CelestialBody body, string siteName,string description)
        {
            
            LaunchSiteExt newSite = new LaunchSiteExt();
            setValues(siteName, description, body.name, lat, lon, newSite);

            GameObject obj;
            GameObject g = new GameObject("VoidModel_spawn");//the actual transform used
            obj = new GameObject(siteName);
            g.GetComponent<Transform>().parent = obj.transform;
            LogDebugOnly("Created gameobject");

            Vector3 position = body.GetRelSurfaceNVector(lat, lon); //radial vector indicating position
            double altitude = body.pqsController.GetSurfaceHeight(position) - body.Radius;
            string siteTransform = "VoidModel_spawn";

            Vector3 orientation = Vector3.up;
            float rotation = 0; //Don't know how to work this out from vessel rotation.
            float visibleRange = 5000;
            LogDebugOnly("Set location variables");

            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = new GameObject[0],
                objects = new GameObject [0],
                visibleRange = visibleRange
            };

            PQSCity launchPQS;
            
            launchPQS = obj.AddComponent<PQSCity>(); //Adds a PQSCity to the game object - appears to put it into the PQS[] in PSystemSetup
            LogDebugOnly("Added PQSCity to gameobject");
            launchPQS.lod = new[] { range };
            launchPQS.frameDelta = 2; //Unknown
            launchPQS.repositionToSphere = false; //enable repositioning to sphere and use RadiusOffset as altitude
            launchPQS.repositionToSphereSurface = true; //Snap to surface
            launchPQS.repositionToSphereSurfaceAddHeight = true;//add RadiusOffset to surfaceHeight when using ToSphereSurface
            launchPQS.repositionRadial = position; //position
            launchPQS.repositionRadiusOffset = 100.0d;//altitude + 10.0d; //height from surface
            //launchPQS.repositionRadiusOffset = 0.0d;//safety distance
            launchPQS.reorientInitialUp = orientation; //orientation
            launchPQS.reorientFinalAngle = rotation; //rotation x axis
            launchPQS.reorientToSphere = true; //adjust rotations to match the direction of gravity
            
            LogDebugOnly("Set PQSCity variables");

            obj.transform.parent = body.pqsController.transform;
            launchPQS.sphere = body.pqsController;
            launchPQS.order = 100;
            launchPQS.modEnabled = true;
            launchPQS.OnSetup();
            launchPQS.Orientate();
            LogDebugOnly("Setup PQSCity");

            
            LogDebugOnly("Creating custom launch site");
            //PSystemSetup.LaunchSite newSite = null;
            
            //this code is copied from kerbtown
            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "SpaceCenterFacility[]")
                {
                    PSystemSetup.SpaceCenterFacility[] sites = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);
                    
                    //PSystemSetup.LaunchSite newSite = new PSystemSetup.LaunchSite();
                    //newSite = new LaunchSiteExt();
                    //newSite.launchPadName = siteName + "/" + siteTransform; //is siteTransform nessesary?
                    //Debug.Log("Launch Pad Name: " + newSite.launchPadName);
                    //newSite.name = siteName;
                    //newSite.description = description;
                    //newSite.pqsName = body.bodyName;
                    //newSite.lat = lat;
                    //newSite.lon = lon;
                    //newSite.body = body.name;
                    Debug.Log("PQS Name: " + newSite.pqsName);

                    PSystemSetup.SpaceCenterFacility[] newSites = new PSystemSetup.SpaceCenterFacility[sites.Length + 1];
                    for (int i = 0; i < sites.Length; ++i)
                    {
                        //Debug.Log("Org Name: " + sites[i].name);
                        //Debug.Log("Org Launch Pad Name: " + sites[i].launchPadName);
                        //Debug.Log("Org PQS Name: " + sites[i].pqsName);
                        newSites[i] = sites[i];
                    }
                    newSites[newSites.Length - 1] = (PSystemSetup.SpaceCenterFacility)newSite;
                    fi.SetValue(PSystemSetup.Instance, newSites);
                    //sites = newSites;
                    Debug.Log("Created launch site \"" + newSite.name + "\" with transform " + newSite.spawnPoints[0].spawnTransformURL);
                    
                }
            }

            MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupFacilities", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateSitesMI == null)
            {
                LogDebugOnly("Fail to find SetupLaunchSites().");
                throw new Exception("Fatal Error - Can't find method SetupLaunchSites().");
            }
            else
                updateSitesMI.Invoke(PSystemSetup.Instance, null);

            launchSites.Add(newSite);
            //SaveLaunchSite(newSite);
            return newSite;
        }

        //get all launch sites - returns null if no sites exist
        public static List<LaunchSiteExt> getLaunchSites()
        {
            LogDebugOnly("Getting Launch Sites");

            return launchSites;
        }

        private static void setValues(string newName, string newDesc, string newBody, double newLat, double newLon, LaunchSiteExt site)
        {

            if (PSystemSetup.Instance.GetSpaceCenterFacility(newName) != null && site.name != newName)//logs 'Can not find launch site' to console when null
                throw new Exception("Launch Site '" + newName + "' already exists");

            //Can't use this on defualt launch sites
            if (site.name == "LaunchPad" || site.name == "Runway")
                throw new Exception("Can't edit default launch sites!");

            //throws exception if a body is not found
            CelestialBody body = FlightGlobals.Bodies.Single<CelestialBody>(b => b.name == newBody);

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

            site.name = newName;
            site.lat = newLat;
            site.lon = newLon;
            site.body = newBody;
            site.pqsName = newBody;
            site.description = newDesc;
            site.facilityTransformName = newName;
            PSystemSetup.SpaceCenterFacility.SpawnPoint point = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
            point.name = newName;
            point.spawnTransformURL = "VoidModel_spawn";
            site.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[] { point };

            //site.launchPadName = newName + "/" + "VoidModel_spawn";

        }

    }

    public class LaunchSiteExt : PSystemSetup.SpaceCenterFacility
    {
        public string description;
        public double lat;
        public double lon;
        public string body;

        

    }

}
