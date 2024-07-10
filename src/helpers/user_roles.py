from enum import Flag


class UserRoles(Flag):
    User = 1
    Banned = 2
    Special = 4
    Admin = 8
