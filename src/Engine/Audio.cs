using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SDL2.SDL;
using static SDL2.SDL_mixer;

namespace GEngine.Engine
{
    public class AudioEngine
    {
        //private ResourceCollection _playingMusic, _playingEffects, _pausedMusic;
        private ResourceManager _resources;

        public bool FadeMusic { get; set; }
        public int FadeMs { get; set; }

        public AudioEngine(ResourceManager resources)
        {
            _resources = resources;
            FadeMusic = true;
            FadeMs = 300;
        }
        public void Init()
        {
            SDL_InitSubSystem(SDL_INIT_AUDIO);
            //Mix_Init(0);
        }
        public void PlayMusic(string audioResource, int loops = -1)
        {
            var res = _resources.GetAudioResource(audioResource);
            if (res.AudioType != AudioType.Music) throw new EngineException($"Audio '{audioResource}' is not music." ,"AudioEngine.PlayMusic()");
            if (Mix_PlayingMusic() != 0)
            {
                StopMusic();
                Mix_PlayMusic(_resources.GetAudioResource(audioResource).DataPtr[0], loops);
            } else if (Mix_PausedMusic() != 0)
            {
                Mix_ResumeMusic();
            } else
            {
                Mix_PlayMusic(_resources.GetAudioResource(audioResource).DataPtr[0], loops);
            }
            Console.WriteLine(SDL_GetError());
        }
        public void PauseMusic()
        {
            if (Mix_PlayingMusic() != 0)
            {
                Mix_PauseMusic();
            }
        }
        public void StopMusic()
        {
            if (FadeMusic)
            {
                Mix_FadeOutMusic(FadeMs);
            }
            else
            {
#pragma warning disable CA1806 // Do not ignore method results
                Mix_HaltMusic(); //Method always return 0, pretty silly imo.
#pragma warning restore CA1806 // Do not ignore method results
            }
        }
        public void PlayEffect(string audioResource, int loops = 0, int audioChannel = 0)
        {
            var res = _resources.GetAudioResource(audioResource);
            if (res.AudioType != AudioType.Effect) throw new EngineException($"Audio '{audioResource}' is not an effect.", "AudioEngine.PlayEffect()");
            if(Mix_PlayChannel(audioChannel, res.DataPtr[0], loops) == -1)
            {
                Debug.Log("AudioEngine.PlayEffectTimed()", "Error playing effect, could either be caused by no free channels or a fatal error.");
            }
        }
        public void PlayEffectTimed(string audioResource, int playingTimeMs, int loops = 0, int audioChannel = 0)
        {
            var res = _resources.GetAudioResource(audioResource);
            if (res.AudioType != AudioType.Effect) throw new EngineException($"Audio '{audioResource}' is not an effect.", "AudioEngine.PlayEffect()");
            if (Mix_PlayChannelTimed(audioChannel, res.DataPtr[0], loops, playingTimeMs) == -1)
            {
                Debug.Log("AudioEngine.PlayEffectTimed()","Error playing effect, could either be caused by no free channels or a fatal error.");
            }
        }
        public void StopChannel(int audioChannel = 0)
        {
#pragma warning disable CA1806 // Do not ignore method results
            Mix_HaltChannel(audioChannel); //Method always return 0, pretty silly imo.
#pragma warning restore CA1806 // Do not ignore method results
        }
        public void PauseChannel(int audioChannel = 0)
        {
            if (Mix_Paused(audioChannel) != 1)
            {
                Mix_Pause(audioChannel);
            }
        }
        public void ResumeChannel(int audioChannel = 0)
        {
            if (Mix_Paused(audioChannel) != 0)
            {
                Mix_Resume(audioChannel);
            }
        }
        public void Quit()
        {
            Mix_Quit();
            SDL_QuitSubSystem(SDL_INIT_AUDIO);
        }
    }
}
