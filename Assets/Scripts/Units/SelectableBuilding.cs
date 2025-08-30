using UnityEngine;

public class SelectableBuilding : Selectable
{
    // לוגיקה מיוחדת למבנים
    public void OpenPanel()
    {
        Debug.Log("Building panel opened for " + gameObject.name);
        // כאן תפתח UI של בנייה / ייצור יחידות וכו'
    }
}
