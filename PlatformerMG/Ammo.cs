using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasJames
{
    class Ammo : Powerup
    {
        public Ammo(Level level, Vector2 pos, string textPath) : base(level, pos, textPath) {
            
        }
        
        public override bool CollisionExitTest()
        {
            return base.CollisionExitTest();
        }

        public override bool CollisionTest(Collidable obj)
        {
            return base.CollisionTest(obj);
        }

        public override void OnCollected()
        {
            level.AmmoCollected();
        }

        public override void OnCollision(Collidable obj)
        {
            base.OnCollision(obj);
        }

        public override void OnCollisionExit(Collidable obj)
        {
            base.OnCollisionExit(obj);
        }
    }
}
