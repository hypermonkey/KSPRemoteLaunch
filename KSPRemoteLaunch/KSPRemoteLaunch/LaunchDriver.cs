using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace KSPRemoteLaunch
{
    public class LaunchDriver:MonoBehaviourExtended
    {

        public static void SetLaunchSite(String siteName) 
        {
            if (PSystemSetup.Instance.GetLaunchSite(siteName) != null)
                EditorLogic.fetch.launchSiteName = siteName;
            else
                throw new Exception("Can't find Launch Site: '" + siteName + "'");

        }

        public static PSystemSetup.LaunchSite CreateCustomLaunchSite(double lat, double lon, CelestialBody body, string siteName)
        {

            //Check for Polar launch site - KSP has camera bug when vessel is directly above either pole.
            if (lat % 90.0d == 0 && lat != 0)
                throw new Exception("Failed to create Launch Site." + System.Environment.NewLine + "Can't set Launch Sites to Poles!");

            GameObject obj;
            GameObject g = new GameObject("VoidModel_spawn");//the actual transform used
            obj = new GameObject(siteName);
            g.GetComponent<Transform>().parent = obj.transform;
            LogDebugOnly("Created gameobject");
            
            string siteTransform = "VoidModel_spawn";

            Vector3 position = body.GetRelSurfaceNVector(lat, lon); //radial vector indicating position
            double altitude = body.pqsController.GetSurfaceHeight(position) - body.Radius;

            LogDebugOnly("Altitude: {0}", altitude);

            if (altitude < 0)//then launchsite is underwater
            {
                LogDebugOnly("Warning: Launch Site {0} Under Water",siteName);
                //we can't force the game to place the vessel above water when launching.
                //use orbit alteration methods to move vessel above water???
                LogDebugOnly("Launch Site {0} not created!",siteName);
                //need to add GUI message here
                //change method type from void to bool?
                throw new Exception("Failed to create Launch Site." + System.Environment.NewLine + "Can't set Launch Sites over water!");
                //return;
            }
                

            

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
            
            launchPQS = obj.gameObject.AddComponent<PQSCity>(); //Adds a PQSCity to the game object - appears to put it into the PQS[] in PSystemSetup
            LogDebugOnly("Added PQSCity to gameobject");
            launchPQS.lod = new[] { range };
            launchPQS.frameDelta = 1; //Unknown
            launchPQS.repositionToSphere = true; //enable repositioning to sphere and use RadiusOffset as altitude
            launchPQS.repositionToSphereSurface = false; //Snap to surface
            launchPQS.repositionToSphereSurfaceAddHeight = false;//add RadiusOffset to surfaceHeight when using ToSphereSurface
            launchPQS.repositionRadial = position; //position
            launchPQS.repositionRadiusOffset = altitude; //height from surface
            launchPQS.reorientInitialUp = orientation; //orientation
            launchPQS.reorientFinalAngle = rotation; //rotation x axis
            launchPQS.reorientToSphere = true; //adjust rotations to match the direction of gravity
            
            LogDebugOnly("Set PQSCity variables");

            obj.gameObject.transform.parent = body.pqsController.transform;
            launchPQS.sphere = body.pqsController;
            launchPQS.order = 100;
            launchPQS.modEnabled = true;
            launchPQS.OnSetup();
            launchPQS.Orientate();
            LogDebugOnly("Setup PQSCity");


            LogDebugOnly("Creating custom launch site");
            PSystemSetup.LaunchSite newSite = null;
            //this code is copied from kerbtown
            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "LaunchSite[]")
                {
                    PSystemSetup.LaunchSite[] sites = (PSystemSetup.LaunchSite[])fi.GetValue(PSystemSetup.Instance);
                    if (PSystemSetup.Instance.GetLaunchSite(siteName) == null) //logs 'Can not find launch site' to console 
                    {
                        //PSystemSetup.LaunchSite newSite = new PSystemSetup.LaunchSite();
                        newSite = new PSystemSetup.LaunchSite();
                        newSite.launchPadName = siteName + "/" + siteTransform; //is siteTransform nessesary?
                        Debug.Log("Launch Pad Name: " + newSite.launchPadName);
                        newSite.name = siteName;
                        newSite.pqsName = body.bodyName;
                        Debug.Log("PQS Name: " + newSite.pqsName);

                        PSystemSetup.LaunchSite[] newSites = new PSystemSetup.LaunchSite[sites.Length + 1];
                        for (int i = 0; i < sites.Length; ++i)
                        {
                            Debug.Log("Org Name: " + sites[i].name);
                            Debug.Log("Org Launch Pad Name: " + sites[i].launchPadName);
                            Debug.Log("Org PQS Name: " + sites[i].pqsName);
                            newSites[i] = sites[i];
                        }
                        newSites[newSites.Length - 1] = newSite;
                        fi.SetValue(PSystemSetup.Instance, newSites);
                        sites = newSites;

                        Debug.Log("Created launch site \"" + newSite.name + "\" with transform " + newSite.launchPadName);
                    }
                    else
                    {
                        Debug.Log("Launch site " + siteName + " already exists");
                        throw new Exception("Launch Site '" + siteName + "' already exists");
                    }
                }
            }

            MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupLaunchSites", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateSitesMI == null)
            {
                LogDebugOnly("Fail to find SetupLaunchSites().");
                throw new Exception("Fatal Error - Can't find method SetupLaunchSites().");
            }
            else
                updateSitesMI.Invoke(PSystemSetup.Instance, null);

            return newSite;
        }

        //get all launch sites - returns null if no sites exist
        public static List<PSystemSetup.LaunchSite> getLaunchSites()
        {
            List<PSystemSetup.LaunchSite> sites = null;
            //PSystemSetup.LaunchSite[] sites
            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "LaunchSite[]")
                {
                    sites = ((PSystemSetup.LaunchSite[])fi.GetValue(PSystemSetup.Instance)).ToList<PSystemSetup.LaunchSite>();
                    
                }
            }

            return sites;
        }

    }

}
