namespace LB
{
    public enum DeviceCommandResult
    {
        Success,
        NotConnected,
        Failed
    }

    public interface IRehabDeviceCommandGateway
    {
        bool IsAvailable { get; }
        float? UnloadingKg { get; }
        DeviceCommandResult DecreaseUnloading();
        DeviceCommandResult ZeroUnloading();
        DeviceCommandResult MoveToHome();
    }

    public static class RehabDeviceCommands
    {
        public static IRehabDeviceCommandGateway Gateway { get; set; }

        public static bool IsAvailable
        {
            get { return Gateway != null && Gateway.IsAvailable; }
        }

        public static float? CurrentUnloadingKg
        {
            get { return Gateway != null && Gateway.IsAvailable ? Gateway.UnloadingKg : null; }
        }

        public static DeviceCommandResult DecreaseUnloading()
        {
            return Gateway != null && Gateway.IsAvailable ? Gateway.DecreaseUnloading() : DeviceCommandResult.NotConnected;
        }

        public static DeviceCommandResult ZeroUnloading()
        {
            return Gateway != null && Gateway.IsAvailable ? Gateway.ZeroUnloading() : DeviceCommandResult.NotConnected;
        }

        public static DeviceCommandResult MoveToHome()
        {
            return Gateway != null && Gateway.IsAvailable ? Gateway.MoveToHome() : DeviceCommandResult.NotConnected;
        }

        public static string GetResultMessage(DeviceCommandResult result)
        {
            switch (result)
            {
                case DeviceCommandResult.Success: return "设备已确认执行成功";
                case DeviceCommandResult.NotConnected: return "通讯未接入，无法自动执行；请按设备手册完成并人工确认";
                default: return "设备执行失败，请检查设备状态后重试";
            }
        }
    }
}
