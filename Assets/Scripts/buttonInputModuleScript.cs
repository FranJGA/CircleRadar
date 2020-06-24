using UnityEngine;
using UnityEngine.EventSystems;

public class buttonInputModuleScript : BaseInputModule
{ 

    public GameObject m_TargetObject;

    public override void Process()
    {
        if (m_TargetObject == null)
            return;
        ExecuteEvents.Execute(m_TargetObject, new BaseEventData(eventSystem), ExecuteEvents.pointerClickHandler);
    }
}
