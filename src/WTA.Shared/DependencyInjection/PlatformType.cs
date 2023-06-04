namespace WTA.Shared.DependencyInjection;

[Flags]
public enum PlatformType
{
    Windows = 0x1,
    Linux = 0x2,
    OSX = 0x4,
    All = Windows | Linux | OSX,
}
