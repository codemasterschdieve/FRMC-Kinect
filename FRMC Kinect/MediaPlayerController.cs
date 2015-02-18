using System;
using System.Windows;
using WMPLib;


///@author Tobias Moser, Jan Plank, Stefan Sonntag

namespace FRMC_Kinect
{
    public class MediaPlayerController
    {
        // [ C# ]
        WindowsMediaPlayer Player;

        public MediaPlayerController()
        {
            //Neuen Player nur erstellen, wenn er noch nicht exisitert.
            //Es soll verhindert werden, dass mehrere MediaPlayer gestartet werden können.
            if (Player == null)
            {
                Player = new WindowsMediaPlayer();
            }

        }

        /// <summary>
        /// Spiel angegebene Datei ab. Vor allem für Testzwecke geeignet.
        /// </summary>
        /// <param name="url"></param>
        private void PlayFile(String url)
        {
            Player.uiMode = "full";
            Player.PlayStateChange +=
                new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
            Player.MediaError +=
                new WMPLib._WMPOCXEvents_MediaErrorEventHandler(Player_MediaError);
            Player.URL = url;
            Player.controls.play();

        }

        private void Player_PlayStateChange(int NewState)
        {
            if ((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsStopped)
            {
                MessageBox.Show("Close");
            }
        }

        private void Player_MediaError(object pMediaObject)
        {
            MessageBox.Show("Cannot play media file.");
        }

        /// <summary>
        /// Statische Datei Abspielen
        /// </summary>
        public void StartPlayer()
        {
            PlayFile(@"C:\Kinect\test.mp3");
        }

        public void Pause()
        {
            Player.controls.pause();
        }

        public void Play()
        {
            Player.controls.play();
        }

        public void Stop()
        {
            Player.close();
        }

        public void FastForward()
        {
            Player.controls.fastForward();
        }

        /// <summary>
        /// Spielt das übergebene Genre ab. Falls es kein Song mit dem Genre gibt,
        /// wird eine Fehlermeldung ausgegeben.
        /// </summary>
        /// <param name="genre"></param>
        public void PlayGenrePlaylist(string genre)
        {
            IWMPPlaylist pl = Player.mediaCollection.getByGenre(genre);
            if (pl.count > 1)
            {
                Player.currentPlaylist = pl;
                Player.controls.play();
            }
            else
            {
                MessageBox.Show("Keine Playlist gefunden");
            }

        }

    }
}
