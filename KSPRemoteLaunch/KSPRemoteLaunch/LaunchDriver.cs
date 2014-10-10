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
            EditorLogic.fetch.launchSiteName = siteName;
        }

        public static CelestialBody getCelestialBody(String name)
        {
            CelestialBody[] bodies = GameObject.FindObjectsOfType(typeof(CelestialBody)) as CelestialBody[];
            foreach (CelestialBody body in bodies)
            {
                if (body.bodyName == name)
                    return body;
            }
            Debug.Log("Couldn't find body \"" + name + "\"");
            return null;
        }
       
        //private static 

        public static void CreateCustomLaunchSite(double lat, double lon, CelestialBody body, string siteName)
        {
            GameObject obj;
            GameObject g = new GameObject("VoidModel_spawn");//the actual transform used
            obj = new GameObject(siteName);
            g.GetComponent<Transform>().parent = obj.transform;
            LogDebugOnly("Created gameobject");
            
            string siteTransform = "VoidModel_spawn";

            Vector3 position = body.GetRelSurfaceNVector(lat, lon); //radial vector indicating position
            double altitude = body.pqsController.GetSurfaceHeight(position) - body.Radius;
            Vector3 orientation = Vector3.up;
            float rotation = 0; //Don't know how to work this out from vessel rotation.
            float visibleRange = 5000;
            LogDebugOnly("Set location variables");

            Transform[] gameObjectList = obj.GetComponentsInChildren<Transform>();
            List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.renderer != null select t.gameObject).ToList();

            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = rendererList.ToArray(),
                objects = new GameObject[0],
                visibleRange = visibleRange
            };
            

            PQSCity launchPQS;
            
            launchPQS = obj.gameObject.AddComponent<PQSCity>(); //Adds a PQSCity to the game object - appears to put it into the PQS[] in PSystemSetup
            LogDebugOnly("Added PQSCity to gameobject");
            launchPQS.lod = new[] { range };
            launchPQS.frameDelta = 1; //Unknown
            launchPQS.repositionToSphere = true; //enable repositioning
            launchPQS.repositionToSphereSurface = false; //Snap to surface?
            launchPQS.repositionRadial = position; //position
            launchPQS.repositionRadiusOffset = altitude; //height
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

            foreach (GameObject renderer in rendererList)
            {
                renderer.renderer.enabled = true;
            }


            Debug.Log("Creating custom launch site");


            foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.Name == "LaunchSite[]")
                {
                    PSystemSetup.LaunchSite[] sites = (PSystemSetup.LaunchSite[])fi.GetValue(PSystemSetup.Instance);
                    if (PSystemSetup.Instance.GetLaunchSite(siteName) == null) //logs 'Can not find launch site' to console 
                    {
                        PSystemSetup.LaunchSite newSite = new PSystemSetup.LaunchSite();
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

                        //this is part of another mod???
                        /*
                        Texture logo = defaultLaunchSiteLogo;
                        Texture icon = null;
                        if (obj.siteLogo != "")
                            logo = GameDatabase.Instance.GetTexture(obj.siteLogo, false);
                        if (obj.siteIcon != "")
                            icon = GameDatabase.Instance.GetTexture(obj.siteIcon, false);
                        launchSites.Add(new LaunchSite(obj.siteName, (obj.siteAuthor != "") ? obj.siteAuthor : obj.model.author, obj.siteType, logo, icon, obj.siteDescription));
                        */
                        Debug.Log("Created launch site \"" + newSite.name + "\" with transform " + newSite.launchPadName);
                    }
                    else
                    {
                        Debug.Log("Launch site " + siteName + " already exists");
                    }
                }
            }

            MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupLaunchSites", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateSitesMI == null)
                Debug.Log("Fail to find SetupLaunchSites().");
            else
                updateSitesMI.Invoke(PSystemSetup.Instance, null);
        }

    }

}
