using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasJames
{
    class Key : Powerup
    {
        public Key(Level level, Vector2 pos, string textPath) : base(level, pos, textPath)
        {
        }

        public override void OnCollected()
        {
            level.OnKeyCollected(this);
        }
    }
}
