using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TreeSharpPlus;

public class InvokeConversation : MonoBehaviour
{
    public GameObject playerA;
    public GameObject playerB;
    public Transform wander1, wander2, wander3;

    private BehaviorAgent behaviorAgentWander, behaviorAgentConverse;
    private bool canGo;

    public void Awake()
    {
        behaviorAgentConverse = new BehaviorAgent(this.ConversationTree());
        behaviorAgentWander = new BehaviorAgent(this.BuildTreeRoot());
        canGo = false;
    }
    public void Start()
    {
        behaviorAgentWander.StartBehavior();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            canGo = true;
            //BehaviorManager.Instance.Register(behaviorAgentWander); //useless
            //BehaviorManager.Instance.Register(behaviorAgentConverse); //useless
            //behaviorAgentWander.StopBehavior(); //playerA stops wandering and freezes.
            //behaviorAgentConverse.StartBehavior(); //doesn't work as expected. playerB starts, but playerA is frozen
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            canGo = false;
        }
    }

    /* This behavior tree starts with playerA wandering to 3 points, then
     * playerB approaches playerA and waves, then repeats
     * 
     * Currently, unsure how to implement this behavior with the wander, 
     * and how to prioritize behaviors.
     * Unsure how to create adaptive behaviors, only scripting works. 
     * How do I get one behavior tree to execute, stop and execute another
     * behavior tree? 
     */
    #region Conversation Tree
    protected Node Converse()
    {
        return new Sequence(
            new LeafTrace("Start conversing"),
            playerA.GetComponent<BehaviorMecanim>().Node_HandAnimation("WAVE", true), //start waving
            playerB.GetComponent<BehaviorMecanim>().Node_HandAnimation("WAVE", true),
            new LeafWait(5000),
            playerA.GetComponent<BehaviorMecanim>().Node_HandAnimation("WAVE", false), //stop waving
            playerB.GetComponent<BehaviorMecanim>().Node_HandAnimation("WAVE", false),
            new LeafTrace("Finished conversing"));
    }

    protected Node EyeContact(Val<Vector3> playerAPos, Val<Vector3> playerBPos)
    {
        Vector3 height = new Vector3(0f, 1.85f, 0f);
        Val<Vector3> playerAHead = Val.V(() => playerAPos.Value + height);
        Val<Vector3> playerBHead = Val.V(() => playerBPos.Value + height);

        return new SequenceParallel(
            playerB.GetComponent<BehaviorMecanim>().Node_HeadLook(playerAHead),
            playerA.GetComponent<BehaviorMecanim>().Node_HeadLook(playerBHead));
    }

    protected Node EyeContactAndConverse(Val<Vector3> playerAPos, Val<Vector3> playerBPos)
    {
        return new Sequence(
            new LeafTrace("Begin EyeContact and Converse"),
            //this.EyeContact(playerAPos, playerBPos), //doesn't work
            this.Converse());
    }

    protected Node ApproachAndOrient(Val<Vector3> playerAPos, Val<Vector3> playerBPos)
    {
        Val<Vector3> approxPlayerBPos = Val.V(() => playerAPos.Value + new Vector3(1, 0, 1)); 
        
        return new Sequence(
            playerB.GetComponent<BehaviorMecanim>().Node_GoTo(approxPlayerBPos), //originally: Node_GoTo(PlayerBPos, 1.0f)
            new SequenceParallel(
                playerB.GetComponent<BehaviorMecanim>().Node_OrientTowards(playerAPos),
                playerA.GetComponent<BehaviorMecanim>().Node_OrientTowards(playerBPos),
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
    #endregion

    #region Wander Tree
    protected Node ST_ApproachAndWait(Transform target)
    {
        Val<Vector3> position = Val.V(() => target.position);
        return new Sequence(
            playerA.GetComponent<BehaviorMecanim>().Node_GoTo(position),
            new LeafWait(1000));
    }

    protected Node BuildTreeRoot()
    {
        Func<bool> act = () => (!canGo);
        Node roaming = new DecoratorLoop(
            new Sequence(
                new SequenceShuffle(
                    this.ST_ApproachAndWait(this.wander1),
                    this.ST_ApproachAndWait(this.wander2),
                    this.ST_ApproachAndWait(this.wander3)),
                ConversationTree()));

        Node trigger = new DecoratorLoop(new LeafAssert(act));
        Node root = new DecoratorLoop(
            new DecoratorForceStatus(RunStatus.Success,
                new SequenceParallel(trigger, roaming)));
        return root;
    }

    #endregion
}
