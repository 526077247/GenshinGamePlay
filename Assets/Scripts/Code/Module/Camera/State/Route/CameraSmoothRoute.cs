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
            loop = config.Loop;
            resolution = config.Resolution;
            points = new CinemachineSmoothPath.Waypoint[config.Points.Length];
            if (config.Points.Length > 0)
            {
                for (int i = 0; i < config.Points.Length; i++)
                {
                    points[i] = new CinemachineSmoothPath.Waypoint()
                    {
                        position = config.Points[i].Position,
                        roll = config.Points[i].Roll,
                    };
                }
            }
        }
    }
}