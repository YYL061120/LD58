using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace DebtJam
{
    public class VisitPanel : MonoBehaviour
    {
        public Transform listRoot;
        public Button itemPrefab;
        public TalkUIHub hub;

        void OnEnable() { Refresh(); }

        public void Refresh()
        {
            foreach (Transform t in listRoot) Destroy(t.gameObject);
            var cm = CaseManager.I;
            var list = cm.runtimeById.Values.Where(r => r.isVisible && r.hasAddress);

            foreach (var rt in list)
            {
                var so = cm.GetSO(rt.debtorId);
                var btn = Instantiate(itemPrefab, listRoot);
                btn.GetComponentInChildren<Text>().text = rt.displayName;
                var img = btn.GetComponentInChildren<Image>(); if (img) img.sprite = so.portrait;

                btn.onClick.AddListener(() => hub.OpenCall(rt.debtorId, so.callCard));
            }
        }
    }
}
