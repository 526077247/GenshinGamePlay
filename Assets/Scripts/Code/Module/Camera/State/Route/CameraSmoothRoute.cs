using System;
using System.Collections.Generic;
using Cinemachine;

namespace TaoTie
{
    public class CameraSmoothRoute
    {
        public CinemachineSmoothPath.Waypoint[] points;

        public int resolution;
        public bool loop;

        public CameraSmoothRoute(ConfigCameraRoute config)
        {
            loop = config.loop;
            resolution = config.resolution;
            points = new CinemachineSmoothPath.Waypoint[config.points.Length];
            if (config.points.Length > 0)
            {
                for (int i = 0; i < config.points.Length; i++)
                {
                    points[i] = new CinemachineSmoothPath.Waypoint()
                    {
                        position = config.points[i].position,
                        roll = config.points[i].roll,
                    };
                }
            }
        }
    }
}