using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine.Game
{
    public enum CollisionType
    {
        Box,
        Circle
    }

    public interface ICollision
    {
        public CollisionType Type { get; }
        public int UBound { get; set; }
        public int DBound { get; set; }
        public int RBound { get; set; }
        public int LBound { get; set; }
        public int Radius { get; set; }
        public int Diameter { get
            {
                return Radius * 2;
            }
        }
    }

    public class CollisionBox : ICollision
    {
        public CollisionType Type { get
            {
                return CollisionType.Box;
            }
        }
        public int UBound { get; set; }
        public int DBound { get; set; }
        public int RBound { get; set; }
        public int LBound { get; set; }
        public int Radius { get { return 0; } set { } }
    }

    public class CollisionCircle : ICollision
    {
        public CollisionType Type
        {
            get
            {
                return CollisionType.Circle;
            }
        }
        public int UBound { get { return 0; } set { } }
        public int DBound { get { return 0; } set { } }
        public int RBound { get { return 0; } set { } }
        public int LBound { get { return 0; } set { } }
        public int Radius { get; set; }
    }
}
