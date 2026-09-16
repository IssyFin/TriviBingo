using UnityEngine;
// usually plcaed on main canvas
public class UIRoot : MonoBehaviour {
    [Tooltip("Родительский объект, куда будут спавниться окна")]
    public Transform WindowsContainer;
    public Transform HudContainer;
}
