using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.CustomInterfaces
{
    public class PatrolActionResult : ActionResult<PatrolResult>
    {
        public const string k_RosMessageName = "custom_interfaces/PatrolActionResult";
        public override string RosMessageName => k_RosMessageName;


        public PatrolActionResult() : base()
        {
            this.result = new PatrolResult();
        }

        public PatrolActionResult(HeaderMsg header, GoalStatusMsg status, PatrolResult result) : base(header, status)
        {
            this.result = result;
        }
        public static PatrolActionResult Deserialize(MessageDeserializer deserializer) => new PatrolActionResult(deserializer);

        PatrolActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = PatrolResult.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
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
