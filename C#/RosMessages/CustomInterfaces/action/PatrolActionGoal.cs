using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.CustomInterfaces
{
    public class PatrolActionGoal : ActionGoal<PatrolGoal>
    {
        public const string k_RosMessageName = "custom_interfaces/PatrolActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public PatrolActionGoal() : base()
        {
            this.goal = new PatrolGoal();
        }

        public PatrolActionGoal(HeaderMsg header, GoalIDMsg goal_id, PatrolGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static PatrolActionGoal Deserialize(MessageDeserializer deserializer) => new PatrolActionGoal(deserializer);

        PatrolActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = PatrolGoal.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.goal_id);
            serializer.Write(this.goal);
        }


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
