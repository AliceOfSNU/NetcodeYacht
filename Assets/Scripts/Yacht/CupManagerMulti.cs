using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XReal.XTown.Yacht
{
    public class CupManagerMulti : CupManager
    {


        private PhotonView view;

        // Start is called before the first frame update
        protected override void Start()
        {
            view = GetComponent<PhotonView>();
            if (!NetworkManager.Instance.networked)
            {
                view.enabled = false;
                base.Start();
                return;
            }
            base.Start();
        }


    }
}

