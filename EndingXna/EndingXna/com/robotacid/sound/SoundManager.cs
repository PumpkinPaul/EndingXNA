using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

using flash;

namespace com.robotacid.sound
{
    /// <summary>
    /// A static class that plays sounds.
    /// 
	/// Must first be initialised to create a library to play from
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class SoundManager {
        
        public static Boolean active = true;
		public static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
        public static Dictionary<string, SoundEffectInstance> soundChannels = new Dictionary<string, SoundEffectInstance>();
		public static Dictionary<string, double > volumes = new Dictionary<string, double >();

        /* Reads values for music and sfx toggles from the SharedObject */
		public static void init() {
			active = UserData.settings.sfx;
			SoundLibrary.init();
		}

        /* Adds a sound to the sounds hash. Use this method to add all sounds to a project */
		public static void addSound(SoundEffect sound, String name, double volume = 1.0) {
			sounds[name] = sound;
			volumes[name] = volume;
		}
		
		/* Plays a sound once */
		public static void playSound(String name, double volume = 1.0) {
			if(!active || sounds.ContainsKey(name) == false) return;
			var sound = sounds[name];
			var soundTransform = volumes[name] * volume;
            var instance = sound.CreateInstance();
			soundChannels[name] = sound.CreateInstance();
            instance.Volume = (float)soundTransform;
            instance.Play();
		}
		
		/* Stops a sound, deleting any loop or fading operation */
		public static void stopSound(String name) {
			if(soundChannels.ContainsKey(name)) {
				soundChannels[name].Stop();
				soundChannels.Remove(name);
			}
		}
		
		/* Stops all sounds (except the currentMusic) */
		public static void stopAllSounds() {
			foreach(var key in soundChannels.Keys){
				stopSound(key);
			}
		}
    }
}
