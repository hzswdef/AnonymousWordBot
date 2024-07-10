namespace AnonymousWordBackend.Extensions;

[Flags]
public enum Roles
{
    User = 1,
    Banned = 2,
    Special = 4,
    Admin = 8
}