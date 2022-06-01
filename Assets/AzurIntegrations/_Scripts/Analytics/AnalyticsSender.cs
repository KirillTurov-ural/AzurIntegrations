using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class AnalyticsSender : MonoBehaviourBase, ISubscriber
    {
        [SerializeField]
        private DataString savedData = new DataString("strings.analytics");

        public string Description
        {
            get => name; 
            set => name = value;
        }

        private void Start()
        {
            SendFinishAfterLoading();
        }

        public void SendStart(params object[] parameters)
        {
            var data = new Dictionary<string, object>();
            //var data = GameManager.GetParametersForSendStart(parameters);
            Analytics.SendEvent("level_start", data);
            Analytics.SendBuffer();
            savedData.Value = data.ToJSON();
        }

        public void SendFinish(params object[] parameters)
        {
            var data = new Dictionary<string, object>();
            //var data = GameManager.GetParametersForSendFinish(parameters);
            Analytics.SendEvent("level_finish", data);
            Analytics.SendBuffer();
            savedData.Value ="";
        }

        private void SendFinishAfterLoading()
        {

            var str = savedData.Value;
            if (str.IsNullOrEmpty())
            {
                return;
            }

            var data =str.FromJSON();
            data["result"] = "close";
            Analytics.SendEvent("level_finish", data);
            Analytics.SendBuffer();
        }

        public void Reaction(Message message, params object[] parameters)
        {
            switch (message)
            {
                case Message.ProfileLocalLoaded:
                    SendFinishAfterLoading();
                    break;
            }
        }
    }
}