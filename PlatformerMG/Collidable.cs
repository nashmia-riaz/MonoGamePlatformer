using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TexasJames
{
    public class Collidable
    {
        #region Fields
        public bool FlaggedForRemoval;
        
        public Rectangle boundingRectangle = new Rectangle();
        public Rectangle BoundingRectangle
        {
            get { return boundingRectangle; }
        }

        public Collidable wasCollidingWith;
        public virtual bool CollisionExitTest()
        {
            return false;
        }
        #endregion

        #region Initialization
        public Collidable()
        {
            FlaggedForRemoval = false;
        }
        #endregion

        #region Member Functions
        public virtual bool CollisionTest(Collidable obj)
        {
            return false;
        }

        public virtual void OnCollision(Collidable obj)
        {
        }


        public virtual void OnCollisionExit(Collidable obj)
        {
        }
        #endregion
    }
}
