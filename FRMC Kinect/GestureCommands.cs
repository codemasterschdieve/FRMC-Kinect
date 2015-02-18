using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


///@author Tobias Moser, Jan Plank, Stefan Sonntag

namespace FRMC_Kinect
{
    public class GestureCommands
    {
        private bool isPlaying = false;
        private bool isStarted = false;
        private bool commandModeActive = false;
        private string currentGestureAction;
        private string logMessage = "";
        private MediaPlayerController mediaPlayer;
        private List<User> currentUserList;

        public GestureCommands()
        {
            mediaPlayer = new MediaPlayerController();
        }

        /// <summary>
        /// Initialisiert alle Gesten
        /// </summary>
        /// <param name="gestureAction"></param>
        /// <returns></returns>
        public string InitializeMediaPlayerActions(string gestureAction, List<User> currentUserList)
        {
            //Aktuelle User zuweisen
            this.currentUserList = currentUserList;

            //Aktuellen Status String zuweisen
            currentGestureAction = gestureAction;

            //Zeigerfiner hoch aktiviert den Command Mode
            commandModeAction();

            //Startet den Player
            startPlayPauseAction();

            //Pausiert den Player
            stopAction();

            return logMessage;
        }

        /// <summary>
        /// Aktiviert den Command Mode für einen bestimmten Zeitraum. 
        /// </summary>
        private void commandModeAction()
        {
            if (currentGestureAction == "Lasso")
            {
                commandModeActive = true;
                leaveCommandMode(5);
                logMessage = "Ready for command";
            }
        }

        /// <summary>
        /// Wenn der Player noch nicht gestarte ist wird er gestartet, wenn er schon läuft wird pausiert.
        /// </summary>
        private void startPlayPauseAction()
        {
            if (commandModeActive && currentGestureAction == "Open" && !isStarted && !isPlaying )
            {
                    List<List<string>> currentUserGenres = new List<List<string>>(); ;
                    foreach (User user in currentUserList)
                    {
                        currentUserGenres.Add(user.MusicGenreNames);
                    }

                    string genreToPlay = GenreFinder.FindMatch(currentUserGenres);

                    if (genreToPlay != null)
                    {
                        mediaPlayer.PlayGenrePlaylist(genreToPlay);
                        logMessage = "Start Media Player mit Genre: " + genreToPlay;
                        commandModeActive = false;
                        isStarted = true;
                        isPlaying = true;
                    }
                    else
                    {
                        logMessage = "Kein Genre gefunden. Fehler: " + genreToPlay;
                        MessageBox.Show(logMessage);
                    }
                    
            }
            //Pausieren, wenn der Player schon läuft
            else if (commandModeActive && currentGestureAction == "Open" && isStarted && isPlaying)
            {
                mediaPlayer.Pause();
                logMessage = "Pause Media Player";
                commandModeActive = false;
                isPlaying = false;
            //Starten wenn Player pausiert
            } else if (commandModeActive && currentGestureAction == "Open" && isStarted && !isPlaying) {
                mediaPlayer.Play();
                logMessage = "Play Media Player";
                commandModeActive = false;
                isPlaying = true;
            }
        }

        /// <summary>
        /// Pausiert den Player wenn der Command Mode aktiv ist, 
        /// der Player ein Song abspielt und die Geste der Pausegeste entspricht
        /// </summary>
        private void stopAction()
        {
            if (commandModeActive && isStarted && currentGestureAction == "Closed")
            {
                mediaPlayer.Stop();
                isPlaying = false;
                isStarted = false;
                logMessage = "Stop Media Player";
                commandModeActive = false;
            }
        }



        /// <summary>
        /// Deaktiviert den Command mode nach 5 Sekunden
        /// http://stackoverflow.com/questions/18372355/how-can-i-perform-a-short-delay-in-c-sharp-without-using-sleep
        /// </summary>
        /// <param name="delayInSeconds">Verzögerung in Sekunden angebeben</param>
        private async void leaveCommandMode(int delayInSeconds)
        {
            int delayInMilliSeconds = delayInSeconds * 1000;
            await Task.Delay(delayInMilliSeconds);
            commandModeActive = false;
            logMessage = "Exit Command Mode";
        }
    }
}
