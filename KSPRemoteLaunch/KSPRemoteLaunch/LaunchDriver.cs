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
                site.AddValue("Body", saveSite.pqsName);
                site.AddValue("Description", saveSite.description);
                launchSiteSave.AddNode(site);
            }


            launchSiteSave.Save(SavePath + SaveFile);

        }

        //this should be removed/replced with save all - as it is effectivly the equivilant? - it does preserve user edits though, but it wont load them
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

        //This is redundant - get rid!
        public static void updateLaunchSite(LaunchSiteExt site, string siteName,string description,CelestialBody body,double lat, double lon)
        {
            LogDebugOnly("UpdateLaunchSite - Args: {0}, {1}, {2}, {3}, {4}, {5}",site.ToString(), siteName, description,body.name,lat,lon);
            //make sure it exists before we try to edit it
            launchSites.Single<LaunchSiteExt>(l => l == site);
            //this should be moved out of here?
            checkValues(siteName, description, body, lat, lon, site);
            //this should be called by the controller - not in here
            site.Update(lat, lon, body, siteName, description);

        }

        public static LaunchSiteExt CreateCustomLaunchSite(double lat, double lon, CelestialBody body, string siteName,string description)
        {
            LogDebugOnly("Creating launch site: " + siteName);
            
            LaunchSiteExt newSite = new LaunchSiteExt(lat, lon, body, siteName, description);
            checkValues(siteName, description, body, lat, lon,newSite);
            
            //this code is copied from kerbtown
            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "SpaceCenterFacility[]")
                {
                    PSystemSetup.SpaceCenterFacility[] sites = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);
                    
                    Debug.Log("PQS Name: " + newSite.pqsName);

                    PSystemSetup.SpaceCenterFacility[] newSites = new PSystemSetup.SpaceCenterFacility[sites.Length + 1];
                    for (int i = 0; i < sites.Length; ++i)
                    {
                        newSites[i] = sites[i];
                    }
                    newSites[newSites.Length - 1] = (PSystemSetup.SpaceCenterFacility)newSite;
                    fi.SetValue(PSystemSetup.Instance, newSites);
                    //sites = newSites;
                    Debug.Log("Created launch site \"" + newSite.name + "\" with transform " + newSite.spawnPoints[0].spawnTransformURL);
                    
                }
            }

            newSite.Setup();
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

        private static void checkValues(string newName, string newDesc, CelestialBody newBody, double newLat, double newLon, LaunchSiteExt site)
        {
            
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

    public class LaunchSiteExt : PSystemSetup.SpaceCenterFacility
    {
        public string description;
        private GameObject SiteObject;
        private CelestialBody body;

        public double lat 
        {
            get 
            {
                if (body == null)
                    return 0;
                
                return body.GetLatitude(this.GetSpawnPoint(this.name).spawnPointTransform.position); 
            
            }
        }
        

        public double lon
        {
            get
            {
                if (body == null)
                    return 0;
                return body.GetLongitude(this.GetSpawnPoint(this.name).spawnPointTransform.position);
            }
        }

        //private ConfigNode saveData = null;
        public ConfigNode configNode
        {
            get
            {
                ConfigNode confSite = new ConfigNode("LaunchSite");
                confSite.AddValue("Name", this.name);
                confSite.AddValue("Lat", this.lat);
                confSite.AddValue("Lon", this.lon);
                confSite.AddValue("Body", this.pqsName);
                confSite.AddValue("Description", this.description);
                return confSite;
            }
        }

        private void CreateLaunchSiteFromVars(double lat, double lon, CelestialBody body, string siteName, string description)
        {
            string siteTransform = "VoidModel_spawn";

            this.name = siteName;
            this.pqsName = body.name;
            this.body = body;
            this.description = description;
            this.facilityTransformName = siteName;
            PSystemSetup.SpaceCenterFacility.SpawnPoint point = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
            point.name = siteName;
            point.spawnTransformURL = siteTransform;
            this.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[] { point };


            GameObject g = new GameObject(siteTransform);//the actual transform used
            SiteObject = new GameObject(siteName);
            g.GetComponent<Transform>().parent = SiteObject.transform;

            Vector3 position = body.GetRelSurfaceNVector(lat, lon); //radial vector indicating position
            Vector3 orientation = Vector3.up;
            float rotation = 0; //Don't know how to work this out from vessel rotation.

            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = new GameObject[0],
                objects = new GameObject[0],
                visibleRange = 5000.0f
            };

            PQSCity launchPQS;

            launchPQS = SiteObject.AddComponent<PQSCity>(); //Adds a PQSCity to the game object - appears to put it into the PQS[] in PSystemSetup
            launchPQS.lod = new[] { range };
            launchPQS.frameDelta = 1; //Unknown
            launchPQS.repositionToSphere = false; //enable repositioning to sphere and use RadiusOffset as altitude
            launchPQS.repositionToSphereSurface = true; //Snap to surface
            launchPQS.repositionToSphereSurfaceAddHeight = true;//add RadiusOffset to surfaceHeight when using ToSphereSurface
            launchPQS.repositionRadial = position; //position
            launchPQS.repositionRadiusOffset = 100.0d;//altitude + 10.0d; //height from surface
            //launchPQS.repositionRadiusOffset = 0.0d;//safety distance
            launchPQS.reorientInitialUp = orientation; //orientation
            launchPQS.reorientFinalAngle = rotation; //rotation x axis
            launchPQS.reorientToSphere = true; //adjust rotations to match the direction of gravity
            
        }

        public LaunchSiteExt(double lat, double lon, CelestialBody body, string siteName, string description)
        {
            CreateLaunchSiteFromVars(lat, lon, body, siteName, description);
        }

        /// <summary>
        /// Convert base class object to LaunchSiteExt class object
        /// </summary>
        /// <param name="oldLaunchSite"></param>
        /// <param name="description"></param>
        /// <param name="body"></param>
        public LaunchSiteExt(PSystemSetup.SpaceCenterFacility oldLaunchSite, string description, CelestialBody body)
        {
            //dont need body - we can work this out?
            this.description = description;
            this.body = body;
            this.SiteObject = oldLaunchSite.facilityTransform.gameObject;

            this.facilityTransformName = oldLaunchSite.facilityTransformName;

            this.facilityPQS = oldLaunchSite.facilityPQS;

            this.facilityTransform = oldLaunchSite.facilityTransform;
            this.facilityName = oldLaunchSite.facilityName;
            this.spawnPoints = oldLaunchSite.spawnPoints;

            this.name = oldLaunchSite.name;
            this.pqsName = oldLaunchSite.pqsName;


            this.facilityPQS.SetTarget(this.spawnPoints[0].spawnPointTransform);
        }
        
        
        /// <summary>
        /// Create a launch site from a Config Node - used for loading saved launch sites
        /// </summary>
        /// <param name="configNode">The Config Node to use
        /// Format:
        /// Lat - The Latitude of the Launch Site
        /// Lon - The Longitude of the Launch Site
        /// Body - The name of the planet/moon
        /// Name - Name of the Launch Site
        /// Description - A description of the Launch Site</param>
        public LaunchSiteExt(ConfigNode configNode)
        {
            double lat = double.Parse(configNode.GetValue("Lat"));
            double lon = double.Parse(configNode.GetValue("Lon"));
            CelestialBody theBody = FlightGlobals.Bodies.Find(body => body.name == configNode.GetValue("Body"));
            string siteName = configNode.GetValue("Name");
            string desc = configNode.GetValue("Description");
            CreateLaunchSiteFromVars(lat, lon,theBody, siteName, desc);
        }

        
        /// <summary>
        /// Connect Launch Site to the rest of the games objects and enable it to be used
        /// </summary>
        public void Setup()
        {
            SiteObject.transform.parent = body.pqsController.transform;
            
            PQSCity launchPQS;
            launchPQS = SiteObject.GetComponent<PQSCity>();
            Debug.Log("Got PQSCity");
            launchPQS.sphere = body.pqsController;
            launchPQS.order = 100;
            launchPQS.modEnabled = true;
            launchPQS.OnSetup();
            launchPQS.Orientate();
            
            PQS[] pqs = { body.pqsController };
            base.Setup(pqs);

        }

        public void Update(double lat, double lon, CelestialBody body, string siteName, string description)
        {
            
            string siteTransform = "VoidModel_spawn";

            //this needs DRYing
            this.name = siteName;
            this.pqsName = body.name;
            this.body = body;
            this.description = description;
            this.facilityTransformName = siteName;
            PSystemSetup.SpaceCenterFacility.SpawnPoint point = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
            point.name = siteName;
            point.spawnTransformURL = siteTransform;
            this.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[] { point };

            SiteObject.name = this.facilityTransform.gameObject.name;

            Vector3 position = body.GetRelSurfaceNVector(lat, lon); //radial vector indicating position

            Vector3 orientation = Vector3.up;
            float rotation = 0; //Don't know how to work this out from vessel rotation.
            float visibleRange = 5000;

            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = new GameObject[0],
                objects = new GameObject[0],
                visibleRange = visibleRange
            };


            PQSCity launchPQS;
            launchPQS = this.facilityTransform.GetComponent<PQSCity>();

            //this needs DRYing
            launchPQS.lod = new[] { range };
            launchPQS.frameDelta = 1; //Unknown
            launchPQS.repositionToSphere = false; //enable repositioning to sphere and use RadiusOffset as altitude
            launchPQS.repositionToSphereSurface = true; //Snap to surface
            launchPQS.repositionToSphereSurfaceAddHeight = true;//add RadiusOffset to surfaceHeight when using ToSphereSurface
            launchPQS.repositionRadial = position; //position
            launchPQS.repositionRadiusOffset = 100.0d; //height from surface
            //launchPQS.repositionRadiusOffset = 1250.0d;//safety distance
            launchPQS.reorientInitialUp = orientation; //orientation
            launchPQS.reorientFinalAngle = rotation; //rotation x axis
            launchPQS.reorientToSphere = true; //adjust rotations to match the direction of gravity

            Setup();
        }

        public void Delete()
        {
            throw new NotImplementedException("NotImplamentedException -> LaunchSiteExt:Update");
        }

        private void SetupPQSCity(PQSCity pqs, double lat, double lon, CelestialBody body)
        {
            throw new NotImplementedException("NotImplamentedException -> LaunchSiteExt:SetupPQSCity");
        }

    }

}
