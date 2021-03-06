﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectHandTracking
{
    public class GestureCommands
    {
        private bool isPlaying = false;
        private  bool isPaused = false;
        private  bool commandModeActive = false;
        //private  string lastGestureAction;
        private string currentGestureAction;
        private string logMessage = "";
        private MediaPlayerController mediaPlayer;

        public GestureCommands()
        {
            mediaPlayer = new MediaPlayerController();
        }

        /// <summary>
        /// Initialisiert alle Gesten
        /// </summary>
        /// <param name="gestureAction"></param>
        /// <returns></returns>
        public string InitializeMediaPlayerActions(string gestureAction)
        {
            //Aktuellen Status String zuweisen
            currentGestureAction = gestureAction;

            //Zeigerfiner hoch aktiviert den Command Mode
            commandModeAction();

            //Startet den Player
            startPlayAction();

            //Pausiert den Player
            pauseAction();

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
        /// Wenn der Player noch nicht gestarte ist wird er gestartet, wenn er nur pausiert ist wird Play
        /// abgespielt.
        /// </summary>
        private void startPlayAction()
        {
            if (commandModeActive &&  !isPlaying && currentGestureAction == "Open")
            {
                //todo 
                if (!isPlaying)
                {
                    mediaPlayer.StartPlayer();
                    logMessage = "Start";
                    commandModeActive = false;
                    isPlaying = true;

                }
            } else if (commandModeActive && isPlaying && currentGestureAction == "Open") {
                mediaPlayer.Play();
                logMessage = "Play";
                commandModeActive = false;
                isPlaying = true;
            }
        }

        /// <summary>
        /// Pausiert den Player wenn der Command Mode aktiv ist, 
        /// der Player ein Song abspielt und die Geste der Pausegeste entspricht
        /// </summary>
        private void pauseAction()
        {
            if (commandModeActive && isPlaying && currentGestureAction == "Closed")
            {
                mediaPlayer.Pause();
                isPlaying = false;
                isPaused = true;
                logMessage = "Pause";
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
