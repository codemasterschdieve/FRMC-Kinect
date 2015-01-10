using System;
using System.Windows;
using WMPLib;

namespace FRMC_Kinect
{
    public class MediaPlayerController
    {
        // [ C# ]
        WindowsMediaPlayer Player;

        //private MediaPlayerController()
        //{
        //    Player = new WindowsMediaPlayer();
        //}

        //public MediaPlayerController initMediaPlayer()
        //{
        //    if (Player == null)
        //    {
        //        new MediaPlayerController();
        //    }
        //}

        public MediaPlayerController()
        {
            //Neuen Player nur erstellen, wenn er noch nicht exisitert.
            //Es soll verhindert werden, dass mehrere MediaPlayer gestartet werden können.
            if (Player == null)
            {
                Player = new WindowsMediaPlayer();
            }

        }

        private void PlayFile(String url)
        {
            Player.uiMode = "full";
            Player.PlayStateChange +=
                new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
            Player.MediaError +=
                new WMPLib._WMPOCXEvents_MediaErrorEventHandler(Player_MediaError);
            Player.URL = url;
            //Process.Start("wmplayer.exe");
            Player.controls.play();

        }




        private void Player_PlayStateChange(int NewState)
        {
            if ((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsStopped)
            {
                MessageBox.Show("Close");
            }
            //MessageBox.Show("state changed");
        }

        private void Player_MediaError(object pMediaObject)
        {
            MessageBox.Show("Cannot play media file.");
        }

        public void StartPlayer()
        {
            PlayFile(@"‪C:\Kinect\test.mp3");
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

        public void Playlist(string genre)
        {
            //IWMPPlaylistArray myPlaylist = Player.playlistCollection.getByName("Kinect");
            IWMPPlaylist pl = Player.mediaCollection.getByGenre(genre);
            Player.currentPlaylist = pl;
            Player.controls.play();


            //IWMPMedia objMedia = Player.currentMedia;
            //MessageBox.Show(objMedia.getItemInfo("Genre"));

        }

    }
}
