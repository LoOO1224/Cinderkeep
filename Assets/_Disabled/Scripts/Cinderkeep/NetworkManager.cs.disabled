using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class NetworkManager : MonoBehaviour
    {
        private const string SaveKey = "OODong.Cinderkeep.Save";

        public static NetworkManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SaveRun(CinderkeepRunModel runModel)
        {
            if (runModel == null)
            {
                return;
            }

            PlayerPrefs.SetString(SaveKey, $"{runModel.CurrentDay}|{runModel.Phase}|{runModel.ResultMessage}");
            PlayerPrefs.Save();
        }

        public string LoadRunSummary()
        {
            return PlayerPrefs.GetString(SaveKey, string.Empty);
        }
    }
}
