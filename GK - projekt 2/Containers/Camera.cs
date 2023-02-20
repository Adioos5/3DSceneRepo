using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GK___projekt_2.Containers
{
    public class Camera
    {
        public Vector3 cameraPosition, cameraTarget, cameraUpVector;

        public Camera(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            this.cameraPosition = cameraPosition;
            this.cameraTarget = cameraTarget;
            this.cameraUpVector = cameraUpVector;
        }
    }
}
