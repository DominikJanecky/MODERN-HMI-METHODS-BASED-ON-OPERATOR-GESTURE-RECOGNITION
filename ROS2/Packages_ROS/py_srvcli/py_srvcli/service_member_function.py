#!/usr/bin/env python3
import rclpy
from rclpy.node import Node
from custom_package.srv import GetWheels

class AngleService(Node):
    def __init__(self):
        super().__init__('angle_service')
        self.subscription = self.create_subscription(
            JointState,
            '/joint_states', 
            self.joint_states_callback,
            10
        )
        self.subscription 

        self.current_left_angle = None
        self.current_right_angle = None
        self.srv = self.create_service(GetWheels, 'get_wheels', self.get_wheels_callback)

    def joint_states_callback(self, msg):
        if 'left_wheel_joint' in msg.name:
            self.current_left_angle = msg.position[msg.name.index('left_wheel_joint')]
        if 'right_wheel_joint' in msg.name:
            self.current_right_angle = msg.position[msg.name.index('right_wheel_joint')]

    def get_wheels_callback(self, request, response):
        if request.request_both_angles:
            response.left_wheel_angle = self.current_left_angle
            response.right_wheel_angle = self.current_right_angle

def main(args=None):
    rclpy.init(args=args)
    angle_service = AngleService()
    rclpy.spin(angle_service)
    rclpy.shutdown()

if __name__ == '__main__':
    main()
