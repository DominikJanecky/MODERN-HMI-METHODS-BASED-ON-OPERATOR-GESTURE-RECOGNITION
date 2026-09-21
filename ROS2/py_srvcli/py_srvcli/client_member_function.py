#!/usr/bin/env python3
import rclpy
from rclpy.node import Node
from custom_package.srv import GetWheels

class AngleServiceClient(Node):
    def __init__(self):
        super().__init__('angle_service_client')
        self.client = self.create_client(GetWheels, 'get_wheels')
        while not self.client.wait_for_service(timeout_sec=1.0):
            pass
        self.request = GetWheels.Request()

    def request_wheels_data(self, request_both_angles=True):
        self.request.request_both_angles = request_both_angles
        future = self.client.call_async(self.request)
        rclpy.spin_until_future_complete(self, future)
        return future.result().left_wheel_angle, future.result().right_wheel_angle

def main(args=None):
    rclpy.init(args=args)
    angle_service_client = AngleServiceClient()
    left_angle, right_angle = angle_service_client.request_wheels_data()
    angle_service_client.destroy_node()
    rclpy.shutdown()

if __name__ == '__main__':
    main()
