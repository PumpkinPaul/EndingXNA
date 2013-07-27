using System;
using System.Collections.Generic;
using flash;
using Math = flash.Math;

namespace com.robotacid.sound
{
    /// <summary>
    /// To reduce the number of calls to the SoundManager, this object builds a queue of all the sound
	/// events to take place in a given frame - adjusting the volume of repeat calls to a sound instead
	/// of issuing multiple calls.
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class SoundQueue {

        public Dictionary<string, Number>  sounds;
		public new Dictionary<string, string> groups;
		public Dictionary<string, int> delays;

        public SoundQueue(){
			sounds = new Dictionary<string, Number>();
			groups = new Dictionary<string, string>();
			delays = new Dictionary<string, int>();
		}

        /* Add a sound to the queue, an optional delay will play the sound after a given number of frames */
		public void add(String name, double volume = 1.0, int delay = 0) {
			if(delay > 0){
				delays[name] = delay;
			} else {
				if(sounds.ContainsKey(name)){
					if(sounds[name] < volume) sounds[name] = volume;
				} else {
					sounds[name] = volume;
				}
			}
		}

        /* Provides a means to play a from selection random sounds, but lock to one sound per frame and boost it's volume for multiple calls */
		public void addRandom(String key, Array<string> choices, double volume = 1.0) {
			if(groups.ContainsKey(key) == false){
				groups[key] = choices[(Math.random() * choices.length) >> 0];
			}
			add(groups[key], volume);
		}

        /* Play a random sound immediately instead of waiting for the update */
		public void playRandom(Array<string> choices, double volume = 1.0) {
			var key = choices[(Math.random() * choices.length) >> 0];
			SoundManager.playSound(key, volume);
		}
		
		/* Play all buffered sounds calls, then clear the buffer */
		public void play() {
			
			foreach(var key in sounds.Keys){
				SoundManager.playSound(key, sounds[key]);
			}
			
			sounds.Clear();
			groups.Clear();
			
			foreach(var key in delays.Keys){
				if(delays.ContainsKey(key)) delays[key]--;
				else {
					add(key);
					delays.Remove(key);
				}
			}
		}
		
		/* Flushes the entire queue */
		public void clear() {
			sounds.Clear();
			groups.Clear();
			delays.Clear();
		}
    }
}
