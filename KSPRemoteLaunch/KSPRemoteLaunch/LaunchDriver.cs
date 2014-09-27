using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPRemoteLaunch
{
    public class LaunchDriver
    {

        public static void SetLaunchLocation(double lat, double lon, double extraHeight, Vessel vessel, CelestialBody planet)
        {
            Vector3d radialVector = planet.GetRelSurfaceNVector(lat, lon);
            double terrainHeight = planet.pqsController.GetSurfaceHeight(radialVector) - planet.Radius;

            Vector3d posLocal;

            //Check if location is over water - we can't put the ship under water (game bugs out)
            if (terrainHeight < 0)
            {
                posLocal = planet.GetRelSurfacePosition(lat,lon,vessel.heightFromTerrain + extraHeight);

            }
            else
            {
                posLocal = planet.GetRelSurfacePosition(lat, lon, vessel.heightFromTerrain + terrainHeight + extraHeight);
            }

            //Velocity set to match the planets rotation a the position specified - so that vessel is stationary on planet surface
            //We add the planets position because getRFrmVel requires world vector

            Vector3d velVector = planet.getRFrmVel(posLocal + planet.position);

            Vector3d r = posLocal.xzy;
            Vector3d v = velVector.xzy;

            Orbit newOrbit = new Orbit();
            newOrbit.UpdateFromStateVectors(r, v, planet, Planetarium.GetUniversalTime());

            

            ChangeVesselOrbit(vessel, newOrbit);

            //Set rotation to be normal to the planet - so that the ship stands upright (like on the launchpad)

            vessel.SetRotation(QuaternionD.AngleAxis(270.0d + lat, velVector.normalized));
        }

        public static void SetLaunchRotationToPlanetNormal(Vessel vessel)
        {

            //vessel.SetRotation(QuaternionD.AngleAxis(270.0d, vessel.mainBody.getRFrmVel(vessel.GetWorldPos3D())));
        }

        private static void ChangeVesselOrbit(Vessel vessel, Orbit newOrbit)
        {
            //Is the new orbit beyond SOI of target planet? - If it is then newOrbit has the wrong celestialBody specified
            if (newOrbit.getRelativePositionAtUT(Planetarium.GetUniversalTime()).magnitude > newOrbit.referenceBody.sphereOfInfluence)
            {
                return;
            }

            vessel.Landed = false;
            vessel.Splashed = false;
            vessel.landedAt = string.Empty;

            //remove all Launch clamps - Can't find a way to rotate these
            List<Part> partsList = vessel.parts.FindAll(p => p.Modules.OfType<LaunchClamp>().Any());
            
            partsList.ForEach(delegate(Part p) { p.Die();});
            
            try
            {
                OrbitPhysicsManager.HoldVesselUnpack(1);
            }
            catch (NullReferenceException)
            {
                
                Debug.Log("Failed to hold Vessel Unpack");
            }
            
            if (!vessel.packed)
                vessel.GoOnRails();
            //foreach (var v in (FlightGlobals.fetch == null ? (IEnumerable<Vessel>)new[] { vessel } : FlightGlobals.Vessels).Where(v => v.packed == false))
            //    v.GoOnRails();

            HardsetOrbit(vessel.orbit, newOrbit);

            vessel.orbitDriver.pos = vessel.orbit.pos.xzy;
            vessel.orbitDriver.vel = vessel.orbit.vel;


            //vessel.orbitDriver.orbit = newOrbit;


        }

        //vessel.orbit is a read only field - so we have to modify the orbits parameters instead
        private static void HardsetOrbit(Orbit orbit, Orbit newOrbit)
        {
            orbit.inclination = newOrbit.inclination;
            orbit.eccentricity = newOrbit.eccentricity;
            orbit.semiMajorAxis = newOrbit.semiMajorAxis;
            orbit.LAN = newOrbit.LAN;
            orbit.argumentOfPeriapsis = newOrbit.argumentOfPeriapsis;
            orbit.meanAnomalyAtEpoch = newOrbit.meanAnomalyAtEpoch;
            orbit.epoch = newOrbit.epoch;
            orbit.referenceBody = newOrbit.referenceBody;
            orbit.Init();
            orbit.UpdateFromUT(Planetarium.GetUniversalTime());
        }
    }
}
