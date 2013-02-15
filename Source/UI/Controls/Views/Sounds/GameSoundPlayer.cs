using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows;
using System.Threading;

namespace Sanguosha.UI.Controls
{
    public class GameSoundPlayer
    {
        private static MediaPlayer _bgmPlayer;
        private static List<MediaPlayer> _effectPlayers;

        private static int _lastGrabbedPlayer;

        public static int PoolSize
        {
            get
            {
                if (_effectPlayers == null) return 0;
                return _effectPlayers.Count;
            }
            set
            {
                _effectPlayers = new List<MediaPlayer>();
                if (value < 1) value = 1;
                for (int i = 0; i < value; i++)
                {
                    _effectPlayers.Add(new MediaPlayer());
                }
                _lastGrabbedPlayer = 0;
            }
        }

        private static bool _isMute;
        public static bool IsMute
        {
            get
            {
                return _isMute;
            }
            set
            {
                if (_isMute == value) return;
                Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    if (value)
                    {
                        if (_bgmPlayer != null)
                        {
                            _bgmPlayer.Stop();
                        }
                        if (_effectPlayers != null)
                        {
                            foreach (var player in _effectPlayers)
                            {
                                player.Stop();
                            }
                        }
                    }
                    else
                    {
                        if (_bgmPlayer != null)
                        {
                            _bgmPlayer.Play();
                        }
                    }
                });
                _isMute = value;
            }
        }

        private static Uri currentBgm;

        public static void PlayBackgroundMusic(Uri uri)
        {
            if (_bgmPlayer == null) _bgmPlayer = new MediaPlayer();
            if (uri == null)
            {
                _bgmPlayer.Stop();
                return;
            }
            currentBgm = uri;
            _bgmPlayer.Open(uri);
            if (!_isMute) _bgmPlayer.Play();
            _bgmPlayer.MediaEnded += new EventHandler(_bgmPlayer_MediaEnded);

        }

        static void _bgmPlayer_MediaEnded(object sender, EventArgs e)
        {
            _bgmPlayer.Position = new TimeSpan(0);
            if (!_isMute) _bgmPlayer.Play();
        }

        public static void PlaySoundEffect(Uri uri)
        {
            Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
            {
                if (uri == null || _isMute) return;
                if (_effectPlayers == null)
                {
                    _effectPlayers = new List<MediaPlayer>();
                    PoolSize = 3;
                }
                _effectPlayers[_lastGrabbedPlayer].Open(uri);
                _effectPlayers[_lastGrabbedPlayer].Play();
                _lastGrabbedPlayer = (_lastGrabbedPlayer + 1) % PoolSize;
            });

        }
    }
}
