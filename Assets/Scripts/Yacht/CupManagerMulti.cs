using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XReal.XTown.Yacht
{
    public class CupManagerMulti : CupManager
    {


        private static PhotonTransformView transformView;
        private static PhotonAnimatorView animView;
        private static PhotonView view;
        protected override void Awake()
        {
            base.Awake();
            Debug.Log("Awake for Cup called!");
            transformView = GetComponent<PhotonTransformView>();
            animView = GetComponent<PhotonAnimatorView>();
            view = GetComponent<PhotonView>();
        }


        // Start is called before the first frame update
        protected override void Start()
        {

            if (!NetworkManager.Instance.networked)
            {
                base.Start();
                DisableCupView();
                return;
            }
            base.Start();
            // avoid animation collisions!
            // DisableAnimator();
        }

        /// <summary>
        /// photon view methods
        /// </summary>
        public static void DisableCupView()
        {
            if (transformView.enabled) transformView.enabled = false;
            if (animView.enabled) animView.enabled = false;
            if (view.enabled) view.enabled = false;
        }

        public static void RequestCupOwnership()
        {
            if (view.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber) return;
            view.RequestOwnership();
        }

        /// photon callbacks
        public void OnOwnershipRequest(object[] viewAndPlayer)
        {
            PhotonView view = viewAndPlayer[0] as PhotonView;
            Player requestingPlayer = viewAndPlayer[1] as Player;

            
            if (view.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Debug.Log("handing over cup control to: player#" + requestingPlayer.ActorNumber);
                view.TransferOwnership(requestingPlayer);
            }
        }
    }
}

