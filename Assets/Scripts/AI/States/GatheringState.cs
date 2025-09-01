using UnityEngine;

public class GatheringState : IAIState
{
    private AIController ai;

    public GatheringState(AIController ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.Agent.isStopped = true; // עומד במקום ואוסף
    }

    public void Update()
    {
        // כאן תוסיף לוגיקה של איסוף משאבים
        // לדוגמה: ספירה לאחור, הורדת amount מה-ResourceNode וכו'

        // אחרי סיום איסוף אפשר לחזור לאיידל
        // ai.SwitchState(new IdleState(ai));
    }

    public void Exit() { }
}
