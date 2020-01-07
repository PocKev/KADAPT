using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;

public class InvokeConversation : MonoBehaviour
{
    public BehaviorMecanim playerA;
    public BehaviorMecanim playerB;

    private BehaviorAgent behaviorAgent;

    /*
     * Currently, unsure how to implement this behavior with the wander, 
     * and how to prioritize behaviors
     */
    protected Node Converse()
    {
        return new Sequence(
            new LeafTrace("Start conversing"),
            playerA.Node_HandAnimation("WAVE", true),
            new LeafWait(1000),
            playerB.Node_HandAnimation("WAVE", true),
            new LeafTrace("Finished conversing"));
    }

    protected Node EyeContact(Val<Vector3> playerAPos, Val<Vector3> playerBPos)
    {
        Vector3 height = new Vector3(0f, 1.85f, 0f);
        Val<Vector3> playerAHead = Val.V(() => playerAPos.Value + height);
        Val<Vector3> playerBHead = Val.V(() => playerBPos.Value + height);

        return new SequenceParallel(
            playerB.Node_HeadLook(playerAHead),
            playerA.Node_HeadLook(playerBHead));
    }

    protected Node EyeContactAndConverse(Val<Vector3> playerAPos, Val<Vector3> playerBPos)
    {
        return new Sequence(
            new LeafTrace("Begin EyeContact and Converse"),
            //this.EyeContact(playerAPos, playerBPos),
            this.Converse());
    }

    protected Node ApproachAndOrient(Val<Vector3> playerAPos, Val<Vector3> playerBPos)
    {
        Val<Vector3> approxPlayerBPos = Val.V(() => playerAPos.Value + new Vector3(1, 0, 1)); 
        
        return new Sequence(
            playerB.Node_GoTo(approxPlayerBPos), //originally: Node_GoTo(PlayerBPos, 1.0f)
            new SequenceParallel(
                playerB.Node_OrientTowards(playerAPos),
                playerA.Node_OrientTowards(playerBPos),
                new LeafTrace("Finished orienting")));
    }

    protected Node ConversationTree()
    {
        Val<Vector3> playerAPos = Val.V(() => playerA.transform.position);
        Val<Vector3> playerBPos = Val.V(() => playerB.transform.position);

        return new Sequence(
            this.ApproachAndOrient(playerAPos, playerBPos),
            this.EyeContactAndConverse(playerAPos, playerBPos));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            behaviorAgent = new BehaviorAgent(this.ConversationTree());
            BehaviorManager.Instance.Register(behaviorAgent);
            behaviorAgent.StartBehavior();
            //BehaviorEvent.Run(this.ConversationTree());
        }
    }
}
