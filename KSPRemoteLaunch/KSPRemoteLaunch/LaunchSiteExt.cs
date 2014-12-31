using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPRemoteLaunch
{

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
        /// <summary>
        /// Config Node that represents a LaunchSiteExt. Can be used in the LaunchSiteExt constructor
        /// </summary>
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

            SetupPQSCity(launchPQS, range, position, orientation, rotation);

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
            CreateLaunchSiteFromVars(lat, lon, theBody, siteName, desc);
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
            //end DRYing

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

            SetupPQSCity(launchPQS, range, position, orientation, rotation);

            Setup();
        }

        //Should this not be Remove/Unset - Just decouple from the planet/moon pqs?
        public void Destroy()
        {
            GameObject.Destroy(SiteObject);
        }

        private void SetupPQSCity(PQSCity launchPQS, PQSCity.LODRange range, Vector3 position, Vector3 orientation, float rotation)
        {
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
        }

    }

}
