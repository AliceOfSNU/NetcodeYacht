using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace XReal.XTown.Yacht
{


    public class GameManagerMulti : GameManager, ITurnCallbacks
    {


        // Update is called once per frame
        protected override void Update()
        {
            if (NetworkManager.Instance is null) return;
            if (!NetworkManager.Instance.networked)
            { // call base function if not networked.
                base.Update();
                return;
            }
            if (NetworkManager.Instance.MeDone || NetworkManager.Instance.Turn < 1) return;
            base.Update();
        }


        // called by selectScore once score selected.
        public static void TurnFinish()
        {
            if (NetworkManager.Instance.MeDone)
            {
                return;
            }
            // this will set MeDone.
            NetworkManager.Instance.SendFinishTurn();
        }
        public Text turnText;

        
        
        public void SetTurnText(int turn)
        {
            turnText.text = "turn " + turn;
        }


        /// <summary>
        /// Interface ITurnCallbacks impl
        /// </summary>
        public void OnTurnBegins(int turn)
        {
            SetTurnText(turn);

            if (NetworkManager.Instance.MeDone)
            {
                Debug.Log($"I'm done, it's other's turn" + turn);
                return;
            }
            // request ownership
            CupManagerMulti.RequestCupOwnership();
            DiceManagerMulti.RequestDiceOwnership();
            SetGameState(GameState.initializing);
            Debug.Log("GameManager/OnTurnBegins Turn #" + turn);

        }

        public void OnPlayerDiceResult(Player player, int turn, int[] results)
        {
            string msg = "on turn #" + turn + ", player " + player.ActorNumber + " rolled: ";
            foreach (int num in results) msg += num;
            Debug.Log(msg);
        }

        public void OnPlayerStrategySelected(Player player, int turn, int move)
        {
            string msg = "on turn #" + turn + ", player " + player.ActorNumber + " selected: " + move;
        }

        public void OnPlayerFinished(Player player, int turn)
        {
            if (NetworkManager.Instance.Turn != turn)
            {
                Debug.LogError("Turn miss sync");
                return;
            }

            Debug.Log("player #" + player.ActorNumber + "'s turn end synced");

            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) // me finished
            {
                Debug.Log("my turn finished!");
                return;
            }
            // other's turn finished I take control
            Debug.Log("It's my turn now");
            NetworkManager.Instance.BeginTurn();
        }
    }
}

