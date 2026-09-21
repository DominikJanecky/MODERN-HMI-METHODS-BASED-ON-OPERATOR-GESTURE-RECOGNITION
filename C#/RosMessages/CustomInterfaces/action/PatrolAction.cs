using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.CustomInterfaces
{
    public class PatrolAction : Action<PatrolActionGoal, PatrolActionResult, PatrolActionFeedback, PatrolGoal, PatrolResult, PatrolFeedback>
    {
        public const string k_RosMessageName = "custom_interfaces/PatrolAction";
        public override string RosMessageName => k_RosMessageName;


        public PatrolAction() : base()
        {
            this.action_goal = new PatrolActionGoal();
            this.action_result = new PatrolActionResult();
            this.action_feedback = new PatrolActionFeedback();
        }

        public static PatrolAction Deserialize(MessageDeserializer deserializer) => new PatrolAction(deserializer);

        PatrolAction(MessageDeserializer deserializer)
        {
            this.action_goal = PatrolActionGoal.Deserialize(deserializer);
            this.action_result = PatrolActionResult.Deserialize(deserializer);
            this.action_feedback = PatrolActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
