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
    public enum GameState
    {
        initializing,
        ready,
        shaking,
        pouring,
        rolling,
        selecting,
        waiting,
        finish
    }

    public class GameManager : MonoBehaviour, ITurnCallbacks
    {
        public static GameManager instance;
        public static Quaternion[] rotArray = new Quaternion[6];
        public static int turnCount = 1;
        public static bool rollTrigger = false;
        public static GameState currentGameState = GameState.initializing;

        public UnityEvent onInitialize;
        public UnityEvent onReadyStart;
        public UnityEvent onReadyToSelect;
        public UnityEvent onShakingStart;
        public UnityEvent onPouringStart;
        public UnityEvent onRollingStart;
        public UnityEvent onRollingFinish;
        public UnityEvent onFinish;


        protected bool readyTrigger = false;


        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                // DontDestroyOnLoad(gameObject);
            }
            /*
            else
            {
                if (instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
            */

            /* dice rotatons */
            rotArray[0] = Quaternion.Euler(90f, 0f, 0f);
            rotArray[1] = Quaternion.Euler(0f, 0f, 0f);
            rotArray[2] = Quaternion.Euler(0f, 90f, 90f);
            rotArray[3] = Quaternion.Euler(0f, 0f, -90f);
            rotArray[4] = Quaternion.Euler(180f, 0f, 0f);
            rotArray[5] = Quaternion.Euler(-90f, 90f, 0f);
        }

        /*
        // Update is called once per frame
        void Update()
        {
            // �ֻ��� �� ��Ÿ ���� �ʱ�ȭ �� ready�� �̵�
            if (currentGameState == GameState.initializing)
            {
                SetGameState(GameState.ready);
                turnCount = 1;
                onInitialize.Invoke();
                readyTrigger = true;
                // TODO ShownSlot �ʱ�ȭ�� onInitialize �̺�Ʈ�� �߰��ؾ� ��
            }

            // ready
            if (currentGameState == GameState.ready && readyTrigger)
            {
                onReadyStart.Invoke();
                readyTrigger = false;
            }

            // ready���� X ������ selecting���� ��ȯ. �̰� ù ��° �ֻ��� ���� ���� �Ұ���
            if (Input.GetKey(KeyCode.X) && currentGameState == GameState.ready && turnCount > 1 && CupManager.playingAnim == false)
            {
                CupManager.playingAnim = true;
                SetGameState(GameState.selecting);
                onReadyToSelect.Invoke();
                Debug.Log("ready to selecting");
            }

            // ready���� �����̽��� ������ shaking���� ��ȯ.
            if (Input.GetKeyDown(KeyCode.Space) && currentGameState == GameState.ready && CupManager.playingAnim == false)
            {
                bool moreThanOne = DiceScript.diceInfoList.Any(x => x.keeping == false);

                if (moreThanOne)
                {
                    SetGameState(GameState.shaking);
                    onShakingStart.Invoke();
                }

            }

            // shaking���� �����̽��� ���� pouring���� ��ȯ.
            if (Input.GetKeyUp(KeyCode.Space) && currentGameState == GameState.shaking)
            {
                SetGameState(GameState.pouring);
                onPouringStart.Invoke();
            }

            // rolling���� �ٲ�� ����
            if (currentGameState == GameState.rolling && rollTrigger == true)
            {
                rollTrigger = false;
                onRollingStart.Invoke();
            }


            bool rollingFinished = !DiceScript.diceInfoList.Any(x => x.diceNumber == 0);

            // ��� �ֻ����� rolling�� ������ selecting���� ��ȯ
            if (currentGameState == GameState.rolling && rollingFinished)
            {
                SetGameState(GameState.selecting);
                onRollingFinish.Invoke();
                turnCount += 1;
            }

            // 3�� �� ������ �� �Ŀ��� selecting���� finish�� ��ȯ
            if (currentGameState == GameState.selecting && turnCount > 3)
            {
                SetGameState(GameState.finish);
                onFinish.Invoke();
            }

            // selecting �ܰ迡�� X ������ ready �ܰ�� ��ȯ. �̰� �ֻ��� �� �� �� ������ �Ұ���
            if (Input.GetKey(KeyCode.X) && currentGameState == GameState.selecting && turnCount <= 3 && CupManager.playingAnim == false)
            {
                bool moreThanOne = DiceScript.diceInfoList.Any(x => x.keeping == false);

                if (moreThanOne)
                {
                    SetGameState(GameState.ready);
                    readyTrigger = true;
                }
            }
        }
        
        */

        public static void SetGameState(GameState newGameState)
        {
            if (Enum.IsDefined(typeof(GameState), newGameState))
            {
                currentGameState = newGameState;
                Debug.Log("game state update: newGameState");
            }
            
        }
        

        /* multiplay */
        /* test UI */
        public void OnClick_randomDice()
        {
            int[] intarr = new int[5];
            for (int i = 0; i < intarr.Length; ++i)
            {
                intarr[i] = UnityEngine.Random.Range(0, 9);
            }
            NetworkManager.Instance.SendDiceResult(intarr);
        }

        public void Onclick_endTurn()
        {
            NetworkManager.Instance.SendFinishTurn();
            if (NetworkManager.Instance.MeDone)
            {
                diceBtn.interactable = false;
                endBtn.interactable = false;
            }
        }
        /* multiplay test */


        /* impl */
        public Text turnText;

        public Button diceBtn;
        public Button endBtn;
        
        public void SetTurnText(int turn)
        {
            turnText.text = "turn " + turn;
        }


        public void OnTurnBegins(int turn)
        {
            Debug.Log("synced Turn: " + turn);
            SetTurnText(turn);
            if (!NetworkManager.Instance.MeDone)
            {
                diceBtn.interactable = true;
                endBtn.interactable = true;
            }
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

            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) // me finished
            {
                return;
            }

            Debug.Log("player #" + player.ActorNumber + "'s turn end synced");
            Debug.Log("player #" + PhotonNetwork.LocalPlayer.ActorNumber + " beginning turn");

            NetworkManager.Instance.BeginTurn();
        }
    }
}

