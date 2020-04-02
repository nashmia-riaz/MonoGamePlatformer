using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace TexasJames
{
    public class SoundManager
    {
        Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
        
        public SoundManager(ContentManager Content)
        {
            sounds.Add("GemCollected", Content.Load<SoundEffect>("Sounds/GemCollected"));
            sounds.Add("ExitReached", Content.Load<SoundEffect>("Sounds/ExitReached"));
            sounds.Add("MonsterKilled", Content.Load<SoundEffect>("Sounds/MonsterKilled"));
            sounds.Add("PlayerJump", Content.Load<SoundEffect>("Sounds/PlayerJump"));
            sounds.Add("PlayerKilled", Content.Load<SoundEffect>("Sounds/PlayerKilled"));
            sounds.Add("PlayerFall", Content.Load<SoundEffect>("Sounds/PlayerFall"));
        } 

        public void PlaySound(string soundName) {
            sounds[soundName].Play();
        }
    }                               
}
