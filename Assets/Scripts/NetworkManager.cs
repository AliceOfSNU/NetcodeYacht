using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using XReal.XTown.Yacht;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    /* singleton */
    public static NetworkManager Instance { get; private set; }
    private NetworkManager() { }

    // set in function BeginTurn and SendFinishTurn.
    private bool _meDone = true;
    public bool MeDone
    {
        get => _meDone;
        private set { _meDone = value; }
    }
    // multiplay?
    public ITurnCallbacks TurnListener;
    public bool networked = false;
    /// Monobehaviour callbacks
    private void Awake()
    {
        Instance = this;
        
    }

    private void Start()
    {
        networked = PhotonNetwork.IsConnected;
        Debug.Log(networked ? "----networked----" : "----not networked----");
        if (networked)
        {
            TurnListener = GetComponent<GameManagerMulti>();
            ScoreTableMulti.InitMultiTable();
            Destroy(GameObject.Find("MugCup"));

        }
        else{
            Debug.LogWarning("NOT CONNECTED TO NETWORK: singleplay mode");
            ScoreTable.InitSingleTable();
            // No synchronization!
            CupManagerMulti.DisableCupView();
            return;
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("NetworkManager: alone in the room.");
        }
        else
        {
            Debug.Log("Two players: Starting game");
            // the second player will instantiate the cup and the dices.
            PhotonNetwork.Instantiate("MugCup", new Vector3(7.14f, 1.31f, 0f), Quaternion.Euler(0f, 90f, 0f));
            Debug.Log("Cup initially owned by" + PhotonNetwork.LocalPlayer.ActorNumber);

            // only second player will start the game
            BeginTurn();
        }
    }

    /// UI callbacks
    public void OnClick_LeaveRoom()
    {
        RoomsManager.Instance.LeaveRoom();
    }



    /// turn passing methods
    // defining turn
    public int Turn
    {
        get { return PhotonNetwork.CurrentRoom.GetTurn(); }
        private set { PhotonNetwork.CurrentRoom.SetTurn(value); }
    }

    // Eventcodes: an integer range 0 - 255
    public const byte TurnEventOffset = 0;
    public const byte EvDiceResult = 1 + TurnEventOffset;
    public const byte EvStrategySelected = 2 + TurnEventOffset;
    public const byte EvFinishTurn = 3 + TurnEventOffset;

    // public methods
    public void BeginTurn()
    {
        Turn = Turn + 1;
        MeDone = false;
    }

    public void SendDiceResult(int[] diceResults)
    {
        if (MeDone)
        {
            Debug.LogWarning("player" + PhotonNetwork.LocalPlayer.ActorNumber + "trying to send move against turn.");
            return;
        }

        // send info
        Hashtable ht = new Hashtable();
        ht.Add("turn", Turn);
        ht.Add("move", System.Array.ConvertAll(diceResults, itm => (object)itm));


        PhotonNetwork.RaiseEvent(EvDiceResult, ht, new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache }, SendOptions.SendReliable);
        // will I listen to this event as well? if so, this must be called.
        // a local call like this may disrupt order, but, yeah.
        ProcessOnEvent(EvDiceResult, ht, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void SendStrategySelected(int strategy)
    {
        if (MeDone)
        {
            Debug.LogWarning("player" + PhotonNetwork.LocalPlayer.ActorNumber + "trying to send move against turn.");
            return;
        }

        Hashtable ht = new Hashtable();
        ht.Add("turn", Turn);
        ht.Add("move", (object)strategy);

        PhotonNetwork.RaiseEvent(EvStrategySelected, ht, new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache }, SendOptions.SendReliable);
        ProcessOnEvent(EvStrategySelected, ht, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void SendFinishTurn()
    {
        if (MeDone)
        {
            Debug.LogWarning("player" + PhotonNetwork.LocalPlayer.ActorNumber + "trying to send move against turn.");
            return;
        }

        Hashtable ht = new Hashtable();
        ht.Add("turn", Turn);

        // I retire..
        MeDone = true;
        Debug.Log("player" + PhotonNetwork.LocalPlayer.ActorNumber + "finished turn sent");

        PhotonNetwork.RaiseEvent(EvFinishTurn, ht, new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache }, SendOptions.SendReliable);
        ProcessOnEvent(EvFinishTurn, ht, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    /* event parsing methods */
    public void ProcessOnEvent(byte evCode, object data, int senderActorNumber)
    {
        Player sender = PhotonNetwork.CurrentRoom.GetPlayer(senderActorNumber);
        switch (evCode)
        {
            case EvDiceResult:
                {
                    // conversion party!!! events need object type arguments!!
                    Hashtable evTable = data as Hashtable;
                    object[] objArr = (object[])evTable["move"];
                    int[] diceResults = System.Array.ConvertAll(objArr, obj => (int)obj);
                    int turn = (int)evTable["turn"];
                    TurnListener.OnPlayerDiceResult(sender, turn, diceResults);

                    break;
                }
            case EvStrategySelected:
                {
                    Hashtable evTable = data as Hashtable;
                    int turn = (int)evTable["turn"];
                    int strategy = (int)evTable["move"];

                    TurnListener.OnPlayerStrategySelected(sender, turn, strategy);
                    break;
                }
            case EvFinishTurn:
                {
                    Hashtable evTable = data as Hashtable;
                    int turn = (int)evTable["turn"];

                    TurnListener.OnPlayerFinished(sender, turn);
                    break;
                }
        }
    }


    /* event callbacks */

    // custom event
    public void OnEvent(EventData photonEvent)
    {
        ProcessOnEvent(photonEvent.Code, photonEvent.CustomData, photonEvent.Sender);
    }
    // Turn - room property event
    public override void OnRoomPropertiesUpdate(Hashtable props)
    {

        if (props != null && props.ContainsKey("Turn"))
        {
            TurnListener.OnTurnBegins(Turn);
        }
    }
}



public interface ITurnCallbacks
{
    void OnTurnBegins(int turn);

    void OnPlayerDiceResult(Player player, int turn, int[] results);

    void OnPlayerStrategySelected(Player player, int turn, int move);

    void OnPlayerFinished(Player player, int turn);
}

public static class TurnExtensions
{
    public static readonly string TurnPropKey = "Turn";


    public static void SetTurn(this Room room, int turn)
    {
        if (room == null || room.CustomProperties == null)
        {
            Debug.LogError("NetworkManager: Cannot set turn information to custorm properties");
            return;
        }
        Hashtable turnProps = new Hashtable();
        turnProps[TurnPropKey] = turn;

        room.SetCustomProperties(turnProps);
    }

    public static int GetTurn(this RoomInfo room)
    {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnPropKey))
        {
            return 0;
        }

        return (int)room.CustomProperties[TurnPropKey];
    }
}
