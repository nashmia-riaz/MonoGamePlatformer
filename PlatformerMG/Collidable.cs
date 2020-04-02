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
        public bool FlaggedForRemoval { get; protected set; }
        
        protected Circle boundingCircle = new Circle(new Vector2(0, 0), 0);
        public Circle BoundingCircle
        {
            get { return boundingCircle; }
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
        #endregion
    }
}
