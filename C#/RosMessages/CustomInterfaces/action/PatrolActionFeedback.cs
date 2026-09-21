using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.CustomInterfaces
{
    public class PatrolActionFeedback : ActionFeedback<PatrolFeedback>
    {
        public const string k_RosMessageName = "custom_interfaces/PatrolActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public PatrolActionFeedback() : base()
        {
            this.feedback = new PatrolFeedback();
        }

        public PatrolActionFeedback(HeaderMsg header, GoalStatusMsg status, PatrolFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static PatrolActionFeedback Deserialize(MessageDeserializer deserializer) => new PatrolActionFeedback(deserializer);

        PatrolActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = PatrolFeedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
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
