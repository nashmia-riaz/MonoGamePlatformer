﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasJames
{
    class CollisionManager
    {
        private List<Collidable> m_Collidables = new List<Collidable>();
        private HashSet<Collision> m_Collisions = new HashSet<Collision>(new CollisionComparer());

        private HashSet<Collision> m_Collision_Exit = new HashSet<Collision>(new CollisionComparer());

        public void AddCollidable(Collidable c)
        {
            m_Collidables.Add(c);
        }

        public void RemoveCollidable(Collidable c)
        {
            m_Collidables.Remove(c);
        }

        public void Update()
        {
            UpdateCollisions();
            ResolveCollisions();
        }

        private void UpdateCollisions()
        {
            if (m_Collisions.Count > 0)
            {
                m_Collisions.Clear();
            }
            if(m_Collision_Exit.Count > 0)
            {
                m_Collision_Exit.Clear();
            }

            // Iterate through collidable objects and test for collisions between each one
            for (int i = 0; i < m_Collidables.Count; i++)
            {
                for (int j = 0; j < m_Collidables.Count; j++)
                {
                    Collidable collidable1 = m_Collidables[i];
                    Collidable collidable2 = m_Collidables[j];

                    // Make sure we're not checking an object with itself
                    if (!collidable1.Equals(collidable2))
                    {
                        // If the two objects are colliding then add them to the set
                        if (collidable1.CollisionTest(collidable2))
                        {
                            m_Collisions.Add(new Collision(collidable1, collidable2));
                        }
                    }

                    if (collidable1.CollisionExitTest())
                    {
                        m_Collision_Exit.Add(new Collision(collidable1, collidable1.wasCollidingWith));
                    }
                }
            }
        }
        
        private void ResolveCollisions()
        {
            foreach (Collision c in m_Collisions)
            {
                c.Resolve();
            }

            foreach(Collision c in m_Collision_Exit)
            {
                c.ResolveCollisionExit();
            }
        }
    }
}
