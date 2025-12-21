using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace PacmanVoice.UI
{
    /// <summary>
    /// Manages game sound effects and their playback.
    /// </summary>
    public class SoundEffectManager
    {
        private readonly Dictionary<string, SoundEffect> _soundEffects = new();
        private readonly ContentManager _contentManager;

        public SoundEffectManager(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        /// <summary>
        /// Load sound effects from the content pipeline.
        /// </summary>
        public void LoadSounds()
        {
            try
            {
                _soundEffects["death"] = _contentManager.Load<SoundEffect>("Audio/DEATH");
                _soundEffects["eatfruit"] = _contentManager.Load<SoundEffect>("Audio/EATFRUIT");
                _soundEffects["eatghost"] = _contentManager.Load<SoundEffect>("Audio/EATGHOST");
                _soundEffects["freeman"] = _contentManager.Load<SoundEffect>("Audio/FREEMAN");
                _soundEffects["theme"] = _contentManager.Load<SoundEffect>("Audio/THEME");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load sound effect: {ex.Message}");
            }
        }

        /// <summary>
        /// Play a sound effect by name.
        /// </summary>
        public void PlaySound(string soundName, float volume = 1.0f)
        {
            if (_soundEffects.TryGetValue(soundName.ToLower(), out var sound))
            {
                sound.Play(volume, 0.0f, 0.0f);
            }
        }

        /// <summary>
        /// Dispose all loaded sound effects.
        /// </summary>
        public void Dispose()
        {
            foreach (var sound in _soundEffects.Values)
            {
                sound?.Dispose();
            }
            _soundEffects.Clear();
        }
    }
}
